using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace ElephantSDK
{
    public class ElephantStorageManager
    {
        private IElephantStorage _modelReference;
        private Storage _loadedStorage;
        private readonly StorageOps _storageOps = new StorageOps();
        private readonly CollectibleOps _collectibleOps = new CollectibleOps();
        private Queue<Collectible> collectiblesQueue = new Queue<Collectible>();

        private static ElephantStorageManager _instance;
        
        public static ElephantStorageManager GetInstance()
        {
            return _instance ?? (_instance = new ElephantStorageManager());
        }

        public void RequestStorage()
        {
            LoadLocalStorage();

            if (!ElephantCore.Instance.GetOpenResponse().internal_config.storage_remote_enabled)
            {
                return;
            }

            var request = _storageOps.StorageDownload(
                _loadedStorage.version,
                response =>
                {
                    if (response.version > _loadedStorage.version)
                    {
                        var parameters = Params.New().CustomString(JsonConvert.SerializeObject(response));
                        Elephant.Event("storage_restored", -1, parameters);
                        
                        _loadedStorage = Storage.FromDownloadResponse(response);
                        SaveLocalStorage();
                        ElephantCore.Instance.isStorageRequestDone = true;
                    }
                    else if (response.version == _loadedStorage.version)
                    {
                        ElephantCore.Instance.isStorageRequestDone = true;
                        ElephantLog.Log("ELEPHANT-StorageManager",
                            "Storage in sync.");
                    }
                    else
                    {
                        ElephantCore.Instance.isStorageRequestDone = true;
                        ElephantLog.Log("ELEPHANT-StorageManager",
                            "Storage is out of sync.");
                    }
                },
                error =>
                {
                    ElephantCore.Instance.isStorageRequestDone = true;
                    ElephantLog.LogError("ELEPHANT-StorageManager", "Failed to download storage due to: " + error);
                });

            ElephantCore.Instance.StartCoroutine(request);


            var collectiblesRequest = _collectibleOps.GetCollectibles(
                EnqueueCollectibles,
                error =>
                {
                    ElephantCore.Instance.isCollectiblesRequestDone = true;
                    ElephantLog.LogError("ELEPHANT-StorageManager", "Failed to get collectibles due to: " + error);
                });

            ElephantCore.Instance.StartCoroutine(collectiblesRequest);
        }

        public void LoadStorage<T>(T model) where T : IElephantStorage
        {
            if (ElephantCore.Instance.isStorageLoaded)
            {
                ElephantLog.LogError("ELEPHANT-StorageManager",
                    "Storage is already loaded!");
                return;
            }
            
            var saveIntervalSeconds = RemoteConfig.GetInstance().GetFloat("storage_save_interval_seconds", 30.0f);
            _modelReference = model;
            if (_loadedStorage.storageData != "" && _loadedStorage.version > 0)
            {
                PopulateReference();
            }
            else
            {
                ElephantLog.Log("ELEPHANT-StorageManager",
                    "No valid storage data available to initialize the model.");
            }

            ElephantCore.Instance.isStorageLoaded = true;

            ShowNextCollectiblePopUp();

            ElephantCore.Instance.StartCoroutine(PeriodicSaveCoroutine(saveIntervalSeconds));
        }


        public void SaveStorage()
        {
            if (_modelReference == null)
            {
                ElephantLog.LogError("ELEPHANT-StorageManager", "Storage not loaded yet!");
                return;
            }
            var modelSerialized = JsonConvert.SerializeObject(_modelReference);

            if (_loadedStorage.storageData == modelSerialized)
            {
                ElephantLog.Log("ELEPHANT-StorageManager", "No changes to save!");
                return;
            }

            _loadedStorage.version++;
            _loadedStorage.storageData = modelSerialized;
            SaveLocalStorage();
            
            if (!ElephantCore.Instance.GetOpenResponse().internal_config.storage_remote_enabled)
            {
                return;
            }

            var request = _storageOps.StorageUpload(_loadedStorage);
            ElephantCore.Instance.StartCoroutine(request);
        }

        private void PopulateReference()
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };

                JsonConvert.PopulateObject(_loadedStorage.storageData, _modelReference, settings);            }
            catch
            {
                ElephantLog.LogError("ELEPHANT-StorageManager", "Storage json can't be parsed!");
            }
        }

        private void SaveLocalStorage()
        {
            PlayerPrefs.SetInt(ElephantConstants.STORAGE_VERSION, _loadedStorage.version);
            PlayerPrefs.SetString(ElephantConstants.STORAGE_LOCAL, _loadedStorage.storageData);
            PlayerPrefs.Save();
        }

        private void LoadLocalStorage()
        {
            _loadedStorage = new Storage
            {
                storageData = PlayerPrefs.GetString(ElephantConstants.STORAGE_LOCAL, ""),
                version = PlayerPrefs.GetInt(ElephantConstants.STORAGE_VERSION, 0)
            };
        }

        private IEnumerator PeriodicSaveCoroutine(float intervalSeconds)
        {
            while (true)
            {
                yield return new WaitForSeconds(intervalSeconds);
                SaveStorage();
            }
        }

        #region Collectible

        private void EnqueueCollectibles(List<Collectible> collectibles)
        {
            foreach (var collectible in collectibles)
            {
                collectiblesQueue.Enqueue(collectible);
            }

            ElephantCore.Instance.isCollectiblesRequestDone = true;
        }

        private void ShowNextCollectiblePopUp()
        {
            if (collectiblesQueue.Count > 0)
            {
                var collectible = collectiblesQueue.Peek();
                ShowCollectiblePopUpView(collectible.message, collectible.button_name);
            }
            else
            {
                ElephantLog.Log("ELEPHANT-StorageManager", "No more collectibles to show.");
            }
        }

        public void ReceiveCollectibleResponse()
        {
            if (collectiblesQueue.Count > 0)
            {
                var collectible = collectiblesQueue.Dequeue();
                Claim(collectible);
            }
            else
            {
                ElephantLog.LogError("ELEPHANT-StorageManager", "No collectible in queue but ReceiveCollectibleResponse was called.");
            }
        }

        private void ShowCollectiblePopUpView(string message, string buttonText)
        {
#if UNITY_EDITOR
            ReceiveCollectibleResponse();

#elif UNITY_IOS
            ElephantIOS.showCollectiblePopUpView(message, buttonText);
#elif UNITY_ANDROID
            ElephantAndroid.ShowCollectibleDialog(message, buttonText);
#endif
        }

        private void Claim(Collectible collectible)
        {
            foreach (var kv in collectible.payload)
            {
                var properties = kv.key.Split('/');
                object currentObject = _modelReference;

                MemberInfo memberInfo = null;
                for (var i = 0; i < properties.Length; i++)
                {
                    if (currentObject == null)
                        break;

                    var currentType = currentObject.GetType();
                    memberInfo = GetMember(currentType, properties[i]);

                    if (memberInfo == null)
                    {
                        ElephantLog.LogError("ELEPHANT-StorageManager", $"Member '{properties[i]}' not found.");
                        break;
                    }

                    if (i < properties.Length - 1)
                    {
                        currentObject = GetValue(currentObject, memberInfo);
                    }
                }

                if (currentObject == null || memberInfo == null)
                    continue;

                var currentValue = GetValue(currentObject, memberInfo);
                var memberType = GetMemberType(memberInfo);
                var newValue = Convert.ChangeType(kv.value, memberType);

                if (!string.IsNullOrEmpty(kv.operation))
                {
                    switch (kv.operation.ToLower())
                    {
                        case "add":
                            if (currentValue is int || currentValue is float || currentValue is double || currentValue is decimal)
                            {
                                var result = Convert.ToDecimal(currentValue) + Convert.ToDecimal(newValue);
                                newValue = Convert.ChangeType(result, memberType);
                            }
                            else
                            {
                                ElephantLog.LogError("ELEPHANT-StorageManager", $"Cannot perform 'add' operation on non-numeric type for key: {kv.key}");
                                continue;
                            }
                            break;
                        case "set":
                            break;
                        default:
                            ElephantLog.LogError("ELEPHANT-StorageManager", $"Unknown operation: {kv.operation} for key: {kv.key}");
                            continue;
                    }
                }
                else
                {
                    if (currentValue is int || currentValue is float || currentValue is double || currentValue is decimal)
                    {
                        var result = Convert.ToDecimal(currentValue) + Convert.ToDecimal(newValue);
                        newValue = Convert.ChangeType(result, memberType);
                    }
                }

                SetValue(currentObject, memberInfo, newValue);
            }

            var request = _collectibleOps.NotifyClaimed(collectible.id.ToString());
            ElephantCore.Instance.StartCoroutine(request);

            ShowNextCollectiblePopUp();
        }

        private static MemberInfo GetMember(Type type, string name)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            
            var member = type.GetMembers(bindingFlags)
                .FirstOrDefault(m => GetMemberName(m) == name);

            if (member == null)
            {
                member = type.GetProperty(name, bindingFlags) as MemberInfo
                         ?? type.GetField(name, bindingFlags);
            }

            return member;
        }

        private static string GetMemberName(MemberInfo memberInfo)
        {
            var jsonPropertyAttribute = memberInfo.GetCustomAttribute<JsonPropertyAttribute>();
            return jsonPropertyAttribute != null ? jsonPropertyAttribute.PropertyName : memberInfo.Name;
        }

        private static object GetValue(object obj, MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo propertyInfo
                ? propertyInfo.GetValue(obj)
                : (memberInfo as FieldInfo)?.GetValue(obj);
        }

        private static void SetValue(object obj, MemberInfo memberInfo, object value)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    propertyInfo.SetValue(obj, value);
                    break;
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(obj, value);
                    break;
            }
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo propertyInfo
                ? propertyInfo.PropertyType
                : (memberInfo as FieldInfo)?.FieldType;
        }

        #endregion
    }
}
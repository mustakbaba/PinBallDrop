using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ElephantSDK
{
    public class Utils
    {
        private static readonly HashSet<string> EeaCountryCodes = new HashSet<string>
        {
            // EU countries
            "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
            "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
            "PL", "PT", "RO", "SK", "SI", "ES", "SE",
        
            // Non-EU EEA countries
            "IS", // Iceland
            "LI", // Liechtenstein
            "NO",  // Norway
            "CH", //Switzerland
            
            "UK", "GB" // UK is not in EEA but is an exception for CMP issues
        };
        
        public static bool IsEeaCountry(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                return false;
            }

            return EeaCountryCodes.Contains(countryCode.ToUpper());
        }
        
        public static string GetISOCODE(SystemLanguage lang)
        {
            string res = "EN";

            switch (lang)
            {
                case SystemLanguage.Afrikaans:
                    res = "AF";
                    break;
                case SystemLanguage.Arabic:
                    res = "AR";
                    break;
                case SystemLanguage.Basque:
                    res = "EU";
                    break;
                case SystemLanguage.Belarusian:
                    res = "BY";
                    break;
                case SystemLanguage.Bulgarian:
                    res = "BG";
                    break;
                case SystemLanguage.Catalan:
                    res = "CA";
                    break;
                case SystemLanguage.Chinese:
                    res = "ZH";
                    break;
                case SystemLanguage.Czech:
                    res = "CS";
                    break;
                case SystemLanguage.Danish:
                    res = "DA";
                    break;
                case SystemLanguage.Dutch:
                    res = "NL";
                    break;
                case SystemLanguage.English:
                    res = "EN";
                    break;
                case SystemLanguage.Estonian:
                    res = "ET";
                    break;
                case SystemLanguage.Faroese:
                    res = "FO";
                    break;
                case SystemLanguage.Finnish:
                    res = "FI";
                    break;
                case SystemLanguage.French:
                    res = "FR";
                    break;
                case SystemLanguage.German:
                    res = "DE";
                    break;
                case SystemLanguage.Greek:
                    res = "EL";
                    break;
                case SystemLanguage.Hebrew:
                    res = "IW";
                    break;
                case SystemLanguage.Hungarian:
                    res = "HU";
                    break;
                case SystemLanguage.Icelandic:
                    res = "IS";
                    break;
                case SystemLanguage.Indonesian:
                    res = "IN";
                    break;
                case SystemLanguage.Italian:
                    res = "IT";
                    break;
                case SystemLanguage.Japanese:
                    res = "JA";
                    break;
                case SystemLanguage.Korean:
                    res = "KO";
                    break;
                case SystemLanguage.Latvian:
                    res = "LV";
                    break;
                case SystemLanguage.Lithuanian:
                    res = "LT";
                    break;
                case SystemLanguage.Norwegian:
                    res = "NO";
                    break;
                case SystemLanguage.Polish:
                    res = "PL";
                    break;
                case SystemLanguage.Portuguese:
                    res = "PT";
                    break;
                case SystemLanguage.Romanian:
                    res = "RO";
                    break;
                case SystemLanguage.Russian:
                    res = "RU";
                    break;
                case SystemLanguage.SerboCroatian:
                    res = "SH";
                    break;
                case SystemLanguage.Slovak:
                    res = "SK";
                    break;
                case SystemLanguage.Slovenian:
                    res = "SL";
                    break;
                case SystemLanguage.Spanish:
                    res = "ES";
                    break;
                case SystemLanguage.Swedish:
                    res = "SV";
                    break;
                case SystemLanguage.Thai:
                    res = "TH";
                    break;
                case SystemLanguage.Turkish:
                    res = "TR";
                    break;
                case SystemLanguage.Ukrainian:
                    res = "UK";
                    break;
                case SystemLanguage.Unknown:
                    res = "EN";
                    break;
                case SystemLanguage.Vietnamese:
                    res = "VI";
                    break;
                default:
                    break;
            }

            return res;
        }

        public static long Timestamp()
        {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }


        public static void SaveToFile(string filename, string text)
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, filename);
                ElephantLog.Log("SaveToFile","filename: " + filename + "text: " + text + "path: " + path);
                File.WriteAllText(path, text);
            }
            catch (Exception e)
            {
                ElephantLog.LogError("SaveToFile",e.Message);
            }
        }

        public static bool IsFileExists(string filename)
        {
            var path = Path.Combine(Application.persistentDataPath, filename);
            return File.Exists(path);
        }

        public static string ReadFromFile(string filename)
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, filename);
                ElephantLog.Log("ReadFromFile", "filename: " + filename + " path: " + path);
        
                if (File.Exists(path))
                {
                    var content = File.ReadAllText(path);

                    const int maxPreviewLength = 200;
                    var preview = content;

                    if (!string.IsNullOrEmpty(preview))
                    {
                        preview = preview.Replace("\r", "\\r").Replace("\n", "\\n");
                        if (preview.Length > maxPreviewLength)
                        {
                            preview = preview.Substring(0, maxPreviewLength) + "...(truncated)";
                        }
                    }

                    ElephantLog.Log("ReadFromFile", 
                        $"Read file '{filename}'. Content preview: '{preview}'");

                    return content;
                }
        
                ElephantLog.Log("ReadFromFile", "File not found: " + filename);
                return null;
            }
            catch (Exception e)
            {
                ElephantLog.LogError("ReadFromFile", e.Message);
            }

            return null;
        }

        
        public static string GetFullPath(string filename)
        {
            return Path.Combine(Application.persistentDataPath, filename);
        }
        
        public static void SaveImageToFile(string filename, byte[] imageData)
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, filename);
                File.WriteAllBytes(path, imageData);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        
        public static string GetFileNameFromUrl(string url)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(url);
            return Convert.ToBase64String(plainTextBytes);
        }
        
        public static string GetSubdirectoryPath()
        {
            var dirPath = Path.Combine(Application.persistentDataPath, "OfferAssets");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return Path.Combine(Application.persistentDataPath, "OfferAssets");
        }

        public static bool IsFileExistsInSubdirectory(string filename)
        {
            var path = Path.Combine(GetSubdirectoryPath(), filename);
            return File.Exists(path);
        }

        public static void SaveToFileInSubdirectory(string filename, string content)
        {
            var dirPath = GetSubdirectoryPath();
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var path = Path.Combine(dirPath, filename);
            File.WriteAllText(path, content);
        }

        public static void SaveImageToFileInSubdirectory(string filename, byte[] imageData)
        {
            var dirPath = GetSubdirectoryPath();
            var path = Path.Combine(dirPath, filename);
            File.WriteAllBytes(path, imageData);
        }

        public static string ReadFromFileInSubdirectory(string filename)
        {
            var path = Path.Combine(GetSubdirectoryPath(), filename);
            return File.ReadAllText(path);
        }

        public static string SignString(string data, string secretKey)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            Byte[] textBytes = encoding.GetBytes(data);
            Byte[] keyBytes = encoding.GetBytes(secretKey);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        
        public static string ReplaceEscapeCharsForUrl(string url)
        {
            return UnityWebRequest.EscapeURL(url).Replace("+","%20");
        }

        public static string GetDeviceCpuArch()
        {
            if (SystemInfo.processorType == SystemInfo.unsupportedIdentifier)  return "unsupported_identifier";
                    
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM", CompareOptions.IgnoreCase) >= 0)
            {
                return Environment.Is64BitProcess ? "ARM64" : "ARM";
            }
            
            return Environment.Is64BitProcess ? "x86_64" : "x86";
        }

        public static bool IsConnected()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public static void PauseGame()
        {
            Time.timeScale = 0;
        }

        public static void ResumeGame()
        {
            Time.timeScale = 1;
        }
        
        public static string GetEnumMemberValue<T>(T value)
            where T : struct, IConvertible
        {
            try
            {
                return typeof(T)
                    .GetTypeInfo()
                    .DeclaredMembers
                    .SingleOrDefault(x => x.Name == value.ToString())
                    ?.GetCustomAttribute<EnumMemberAttribute>(false)
                    ?.Value;
            }
            catch (Exception e)
            {
                ElephantLog.Log("UTILS", e.Message);
                return value.ToString();
            }
            
        }
        
        public static long ReadLongFromFile(string filename, long defaultValue = 0)
        {
            var raw = ReadFromFile(filename);

            if (string.IsNullOrWhiteSpace(raw))
            {
                ElephantLog.LogError("ReadLongFromFile",
                    $"File '{filename}' is null/empty/whitespace. Raw: '{raw}'");
                return defaultValue;
            }

            raw = raw.Trim();

            if (!long.TryParse(raw, out var value))
            {
                ElephantLog.LogError("ReadLongFromFile",
                    $"File '{filename}' contains invalid long. Raw: '{raw}'");
                return defaultValue;
            }

            return value;
        }
    }
}
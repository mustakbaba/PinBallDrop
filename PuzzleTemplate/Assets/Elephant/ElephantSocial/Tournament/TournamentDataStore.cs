using System;
using System.Collections.Generic;
using System.Linq;
using ElephantSDK;
using ElephantSocial.Model;
using ElephantSocial.Tournament.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSocial.Tournament
{
    public class TournamentDataStore
    {
        private static readonly Lazy<TournamentDataStore> _instance =
            new Lazy<TournamentDataStore>(() => new TournamentDataStore());

        public static TournamentDataStore Instance => _instance.Value;

        private const string TournamentResponseDataStoreKey = "TournamentResponseStoreKey";
        private const string MyTournamentsResponseDataStoreKey = "MyTournamentsResponseKey";
        private const string MyTournamentResultsDataStoreKey = "MyTournamentResultsResponseKey";
        private const string TournamentBoardResponseStoreKey = "TournamentBoardResponseKey";

        private static void Save<T>(string saveKey, T data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(saveKey, jsonData);
        }

        private static bool Load<T>(string key, out T loadedData)
        {
            var storedData = PlayerPrefs.GetString(key, "");
            if (string.IsNullOrEmpty(storedData))
            {
                loadedData = default;
                return false;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(storedData);
                loadedData = data;
                return true;
            }
            catch (JsonException jsonException)
            {
                ElephantLog.LogError("TournamentDataStoreInternalJson", jsonException.Message);
                loadedData = default;
                return false;
            }
            catch (Exception e)
            {
                ElephantLog.LogError("TournamentDataStoreInternal", e.Message);
                loadedData = default;
                return false;
            }
        }

        public void SetTournaments(TournamentsResponse tournamentResponse)
        {
            Save(TournamentResponseDataStoreKey, tournamentResponse);
        }

        public TournamentsResponse GetTournaments()
        {
            return Load(TournamentResponseDataStoreKey, out TournamentsResponse data) ? data : null;
        }

        public void SetMyTournamentsResponse(MyTournamentsResponse myTournamentsResponse)
        {
            Save(MyTournamentsResponseDataStoreKey, myTournamentsResponse);
        }

        public MyTournamentsResponse GetMyTournamentsResponse()
        {
            return Load(MyTournamentsResponseDataStoreKey, out MyTournamentsResponse data) ? data : null;
        }

        public void SetMyTournamentResults(MyTournamentResultsResponse data)
        {
            Save(MyTournamentResultsDataStoreKey, data);
        }

        public MyTournamentResultsResponse GetMyTournamentResults()
        {
            return Load(MyTournamentResultsDataStoreKey, out MyTournamentResultsResponse data) ? data : null;
        }

        private string PrepareBoardPrefKey(int tournamentId, int scheduleId)
        {
            return TournamentBoardResponseStoreKey + "-" + tournamentId + "-" + scheduleId;
        }

        public TournamentBoardResponse GetTournamentBoardResponse(int tournamentId, int scheduleId)
        {
            return Load(PrepareBoardPrefKey(tournamentId, scheduleId), out TournamentBoardResponse data) ? data : null;
        }

        public void SetTournamentBoardResponse(int tournamentId, int scheduleId,
            TournamentBoardResponse tournamentBoardResponse)
        {
            Save(PrepareBoardPrefKey(tournamentId, scheduleId), tournamentBoardResponse);
        }

        #region Offline Scores

        private const string OfflineScoresKeyPrefix = "OfflineScores_Tournament_";
        private const string OfflineScoresKeyIndex = "OfflineScores_KeyIndex";

        public class OfflineScore
        {
            public int Score { get; set; }
            public long Timestamp { get; set; }
        }

        private string GetOfflineScoresKey(int tournamentId, int scheduleId)
        {
            var key = $"{OfflineScoresKeyPrefix}{tournamentId}_{scheduleId}";
            AddKeyToIndex(key);
            return key;
        }

        public void SaveOfflineScore(int tournamentId, int scheduleId, int score)
        {
            var scores = GetOfflineScores(tournamentId, scheduleId);
            scores.Add(new OfflineScore
            {
                Score = score,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            Save(GetOfflineScoresKey(tournamentId, scheduleId), scores);
        }

        public List<OfflineScore> GetOfflineScores(int tournamentId, int scheduleId)
        {
            return Load(GetOfflineScoresKey(tournamentId, scheduleId), out List<OfflineScore> data)
                ? data
                : new List<OfflineScore>();
        }

        public void DeleteOfflineScores(int tournamentId, int scheduleId)
        {
            var key = GetOfflineScoresKey(tournamentId, scheduleId);
            PlayerPrefs.DeleteKey(key);
            RemoveKeyFromIndex(key);
            PlayerPrefs.Save();
        }

        public List<(int tournamentId, int scheduleId)> GetAllOfflineScoreTournaments()
        {
            var result = new List<(int tournamentId, int scheduleId)>();
            foreach (var key in GetAllKeysFromIndex())
            {
                if (ParseTournamentKey(key, out int tournamentId, out int scheduleId))
                {
                    result.Add((tournamentId, scheduleId));
                }
                else
                {
                    PlayerPrefs.DeleteKey(key);
                    RemoveKeyFromIndex(key);
                }
            }

            return result;
        }

        private string GetStorageKey(int tournamentId, int scheduleId)
        {
            var key = $"{OfflineScoresKeyPrefix}{tournamentId}_{scheduleId}";
            AddKeyToIndex(key);
            return key;
        }

        private void AddKeyToIndex(string key)
        {
            var keys = GetAllKeysFromIndex();
            if (!keys.Contains(key))
            {
                keys.Add(key);
                SaveKeyIndex(keys);
            }
        }

        private void RemoveKeyFromIndex(string key)
        {
            var keys = GetAllKeysFromIndex();
            if (keys.Remove(key))
            {
                SaveKeyIndex(keys);
            }
        }

        private List<string> GetAllKeysFromIndex()
        {
            if (!PlayerPrefs.HasKey(OfflineScoresKeyIndex))
                return new List<string>();

            try
            {
                var json = PlayerPrefs.GetString(OfflineScoresKeyIndex);
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveKeyIndex(List<string> keys)
        {
            var json = JsonConvert.SerializeObject(keys);
            PlayerPrefs.SetString(OfflineScoresKeyIndex, json);
            PlayerPrefs.Save();
        }

        public void CleanupAllOfflineScores()
        {
            var keys = GetAllKeysFromIndex();
            foreach (var key in keys)
            {
                if (key.StartsWith(OfflineScoresKeyPrefix))
                {
                    PlayerPrefs.DeleteKey(key);
                    RemoveKeyFromIndex(key);
                }
            }

            PlayerPrefs.Save();
            ElephantLog.Log("TOURNAMENT", "Cleaned up all offline tournament scores");
        }

        private bool ParseTournamentKey(string key, out int tournamentId, out int scheduleId)
        {
            tournamentId = 0;
            scheduleId = 0;

            try
            {
                var parts = key.Replace(OfflineScoresKeyPrefix, "").Split('_');
                if (parts.Length == 2)
                {
                    tournamentId = int.Parse(parts[0]);
                    scheduleId = int.Parse(parts[1]);
                    return true;
                }
            }
            catch (Exception e)
            {
                ElephantLog.LogError("TOURNAMENT", $"Failed to parse tournament key: {key} + {e.Message}");
            }

            return false;
        }

        #endregion
    }
}
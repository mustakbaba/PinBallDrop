using System;
using System.Collections.Generic;
using System.Linq;
using ElephantSDK;
using ElephantSocial.Leaderboard.Model;
using ElephantSocial.Leaderboard.Network;
using ElephantSocial.Model;
using UnityEngine;

namespace ElephantSocial.Leaderboard
{
    public class Leaderboard : GenericResponseOps
    {
        private readonly LeaderboardOps _leaderboardOps = new LeaderboardOps();
        private LeaderboardContainerCache _leaderboardContainerCache;
        private int leaderboardId;
        private BoardPlayer boardPlayer;
        private bool _isInitializeRequested;
        private long nextUnixSeconds = 0;

        /// <summary>
        /// Creates a new instance of Leaderboard for the specified leaderboard ID.
        /// </summary>
        /// <param name="leaderboardId">The unique identifier for this leaderboard</param>
        public Leaderboard(int leaderboardId)
        {
            this.leaderboardId = leaderboardId;
        }

        /// <summary>
        /// Initializes this leaderboard instance.
        /// </summary>
        /// <param name="onResponse">Callback action that is invoked if the operation is successful.</param>
        /// <param name="onError">Callback action that is invoked with an error message if the operation fails.</param>
        public void Init(Action onResponse, Action<string> onError)
        {
            if (_isInitializeRequested)
            {
                ElephantLog.LogError("Leaderboard", "Multiple init requested");
                return;
            }

            InitPlayer(responseBoardPlayer =>
            {
                boardPlayer = responseBoardPlayer;
                InitLeaderboardCache(
                    RequestLeaderboardContainer,
                    () =>
                    {
                        _isInitializeRequested = true;
                        onResponse?.Invoke();
                    },
                    cachingError => ElephantLog.LogError("LeaderboardCache", cachingError));
            }, error => { onError?.Invoke(error); });
        }

        /// <summary>
        /// Sets the score for the player in this leaderboard.
        /// </summary>
        /// <param name="score">Score value to set</param>
        /// <param name="onResponse">Callback action that is invoked with the updated player if successful</param>
        /// <param name="onError">Callback action that is invoked with an error message if the operation fails</param>
        public void SetScore(int score,
            Action<BoardPlayer> onResponse,
            Action<string> onError)
        {
            if (boardPlayer == null)
            {
                onError.Invoke("Board player not initialized!");
                ElephantLog.LogError("Leaderboard", "Board player not initialized!");
                return;
            }
            
            if(nextUnixSeconds != 0)
            {
                onError.Invoke("Can not set score to scheduled tournament!");
                ElephantLog.LogError("Leaderboard", "Can not set score to scheduled tournament!");
                return;
            }

            boardPlayer.score = score;
            var setScoreJob = _leaderboardOps.UpdateScore(boardPlayer, leaderboardId, "set", boardPlayer.score,
                response => HandleResponse(response, x => { onResponse?.Invoke(boardPlayer.Clone()); }, onError),
                onError);

            ElephantCore.Instance.StartCoroutine(setScoreJob);
        }

        /// <summary>
        /// Adds to the current score for the player in this leaderboard.
        /// </summary>
        /// <param name="scoreToAdd">Score value to add to the current score</param>
        /// <param name="onResponse">Callback action that is invoked with the updated player if successful</param>
        /// <param name="onError">Callback action that is invoked with an error message if the operation fails</param>
        public void AddScore(int scoreToAdd,
            Action<BoardPlayer> onResponse,
            Action<string> onError)
        {
            if (boardPlayer == null)
            {
                onError.Invoke("Board player not initialized!");
                ElephantLog.LogError("Leaderboard", "Board player not initialized!");
                return;
            }

            boardPlayer.score += scoreToAdd;
            
            

            var addScoreJob = _leaderboardOps.UpdateScore(boardPlayer, leaderboardId,"incr", scoreToAdd,
                response => HandleResponse(response, x =>
                {
                    onResponse?.Invoke(boardPlayer.Clone());
                    boardPlayer.score = x.score;
                }, onError),
                onError);

            ElephantCore.Instance.StartCoroutine(addScoreJob);
        }

        /// <summary>
        /// Retrieves the current leaderboard data.
        /// </summary>
        /// <param name="onResponse">Callback action that is invoked with the leaderboard data if successful</param>
        /// <param name="onError">Callback action that is invoked with an error message if the operation fails</param>
        public void GetLeaderboard(Action<LeaderboardContainer> onResponse,
            Action<string> onError)
        {
            if (_leaderboardContainerCache == null)
            {
                var errorMessage = "Leaderboard caching system is not initialized.";
                onError?.Invoke(errorMessage);
                ElephantLog.LogError("Leaderboard", errorMessage);
                return;
            }

            _leaderboardContainerCache.GetData(
                leaderboardContainer =>
                {
                    nextUnixSeconds = leaderboardContainer.global.next;
                    SortLeaderboardContainer(leaderboardContainer, onResponse);
                },
                x => { onError?.Invoke(x); });
        }

        /// <summary>
        /// Gets the end date for this leaderboard.
        /// </summary>
        /// <returns>The end date of the leaderboard</returns>
        public long GetNextUnixSeconds()
        {
            return nextUnixSeconds;
        }

        /// <summary>
        /// Returns the current Board Player Data.
        /// </summary>
        /// <returns>BoardPlayer or null if not initialized</returns>
        public BoardPlayer GetBoardPlayer()
        {
            if (!_isInitializeRequested)
            {
                var errorMessage = "Leaderboard not initialized!";
                ElephantLog.LogError("Leaderboard", errorMessage);
                return null;
            }

            RenewPlayerData();
            return boardPlayer.Clone();
        }

        private void RenewPlayerData()
        {
            if (!_isInitializeRequested)
            {
                var errorMessage = "Leaderboard not initialized!";
                ElephantLog.LogError("Leaderboard", errorMessage);
                return;
            }

            var player = Social.Instance.GetPlayer();
            boardPlayer.FillBaseData(player);
        }

        /// <summary>
        /// Sorts the board player list by score in descending order if a player exists with the same social ID as the given board player.
        /// If the player's score is lower than the score of the last player in the list, the sorting is skipped, and the response is invoked directly.
        /// </summary>
        /// <param name="leaderboardContainer">The list of board players to be checked and sorted.</param>
        /// <param name="onResponse">An action that is invoked with the sorted list or the original list if sorting is skipped.</param>
        private void SortLeaderboardContainer(LeaderboardContainer leaderboardContainer,
            Action<LeaderboardContainer> onResponse)
        {
            if (boardPlayer == null)
            {
                var errorMessage = "Board player is not initialized before sorting the leaderboard.";
                ElephantLog.LogError("Leaderboard", errorMessage);
                return;
            }

            RenewPlayerData();
            leaderboardContainer.global.records = UpdatePlayer(boardPlayer, leaderboardContainer.global.GetRecords());
            leaderboardContainer.global.records = SortLeaderboard(leaderboardContainer.global.GetRecords());
            leaderboardContainer.local.records = UpdatePlayer(boardPlayer, leaderboardContainer.local.GetRecords());
            leaderboardContainer.local.records = SortLeaderboard(leaderboardContainer.local.GetRecords());
            onResponse?.Invoke(leaderboardContainer);
        }

        private static List<BoardPlayer> SortLeaderboard(List<BoardPlayer> list)
        {
            return list?.OrderByDescending(player => player.score).ToList();
        }

        private List<BoardPlayer> UpdatePlayer(BoardPlayer targetBoardPlayer, List<BoardPlayer> boardPlayerList)
        {
            if (boardPlayerList == null)
            {
                var errorMessage = "Update Player operation failed. Target list is null";
                ElephantLog.LogError("Leaderboard", errorMessage);
                return null;
            }

            if (boardPlayerList.Count == 0)
            {
                boardPlayerList.Add(targetBoardPlayer);
                return boardPlayerList;
            }

            bool isScoreLowerThanLastPlayer =
                boardPlayerList[boardPlayerList.Count - 1].score > targetBoardPlayer.score;
            if (isScoreLowerThanLastPlayer)
            {
                return boardPlayerList;
            }

            for (int i = 0; i < boardPlayerList.Count; i++)
            {
                if (boardPlayerList[i].socialId != targetBoardPlayer.socialId)
                    continue;

                boardPlayerList[i] = targetBoardPlayer;
                break;
            }

            return boardPlayerList.OrderByDescending(player => player.score).ToList();
        }

        /// <summary>
        /// Requests the leaderboard data based on the specified leaderboard type and event. 
        /// </summary>
        /// <param name="onResponse">Callback action that is invoked with the server response if the operation is successful.</param>
        /// <param name="onError">Callback action that is invoked with an error message if the operation fails.</param>
        private void RequestLeaderboardContainer(Action<LeaderboardContainer> onResponse,
            Action<string> onError)
        {
            ElephantCore.Instance.StartCoroutine(
                _leaderboardOps.GetLeaderboard(leaderboardId,
                    response => HandleResponse(response, lbContainer => { onResponse?.Invoke(lbContainer); },
                        x => { onError?.Invoke(x); }),
                    x => { onError?.Invoke(x); }));
        }

        /// <summary>
        /// Initializes Board Player 
        /// </summary>
        /// <param name="onResponse">Callback action that is invoked with the server response if the operation is successful.</param>
        /// <param name="onError">Callback action that is invoked with an error message if the operation fails.</param>
        private void InitPlayer(Action<BoardPlayer> onResponse,
            Action<string> onError)
        {
            var player = Social.Instance.GetPlayer();
            var getScoreJob = _leaderboardOps.GetScore(leaderboardId,
                response => HandleResponse(response, x => onResponse?.Invoke(x.Clone()), onError),
                onError);

            ElephantCore.Instance.StartCoroutine(getScoreJob);
        }

        /// <summary>
        /// Initializes Leaderboard caching system.
        /// </summary>
        private void InitLeaderboardCache(Action<Action<LeaderboardContainer>,
            Action<string>> requestLeaderboardAction, Action onInitialized, Action<string> onError)
        {
            int cachingIntervalTime = RemoteConfig.GetInstance().GetInt("social_lb_caching_interval_seconds", 30);
            _leaderboardContainerCache = LeaderboardContainerCache.CreateInstance(
                (action, error) =>
                {
                    requestLeaderboardAction?.Invoke(response => { action?.Invoke(response); },
                        x => { error?.Invoke(x); });
                },
                cachingIntervalTime);

            _leaderboardContainerCache.GetData(x =>
                {
                    ElephantLog.Log("Leaderboard", "Global Leaderboard cached");
                    nextUnixSeconds = x.global.next;
                    onInitialized?.Invoke();
                },
                error =>
                {
                    ElephantLog.LogError("Leaderboard", error);
                    onError?.Invoke(error);
                });
        }
    }
}
using System;
using System.Collections.Generic;
using ElephantSDK;
using UnityEngine;

namespace ElephantSocial.CachingSystem
{
    public class GenericCachingSystem<T>
    {
        private T cachedData;
        private readonly int cachingIntervalSeconds;
        private DateTime lastCachingDateTime;
        private readonly Action<Action<T>, Action<string>> dataRequestAction;
        private readonly List<Action<T>> waitingResponses = new List<Action<T>>();
        private bool requestInProgress = false;

        protected GenericCachingSystem(Action<Action<T>, Action<string>> dataRequestAction, int cachingIntervalSeconds)
        {
            this.cachingIntervalSeconds = cachingIntervalSeconds;
            this.dataRequestAction = dataRequestAction;
        }
        
        protected GenericCachingSystem(Action<Action<T>, Action<string>> dataRequestAction, int cachingIntervalSeconds, T initValues)
        {
            this.cachingIntervalSeconds = cachingIntervalSeconds;
            this.dataRequestAction = dataRequestAction;
            cachedData = initValues;
            lastCachingDateTime = DateTime.Now;
        }

        public void GetData(Action<T> response, Action<string> onError)
        {
            var seconds = (DateTime.Now - lastCachingDateTime).TotalSeconds;
            if (cachedData == null || cachingIntervalSeconds < seconds)
            {
                waitingResponses.Add(response);
                if (requestInProgress)
                {
                    return;
                }
                
                requestInProgress = true;
                dataRequestAction?.Invoke(x =>
                {
                    lastCachingDateTime = DateTime.Now;
                    cachedData = x;
                    foreach (var waitingResponse in waitingResponses)
                    {
                        waitingResponse?.Invoke(cachedData);
                    }
                    
                    waitingResponses.Clear();
                    requestInProgress = false;
                }, x =>
                {
                    onError?.Invoke(x);
                    requestInProgress = false;
                });
            }
            else
            {
                response(cachedData);
            }
        }
        
        /// <summary>
        /// Clears the cache data and resets the caching timer.
        /// </summary>
        public void ClearCache()
        {
            cachedData = default(T);
            lastCachingDateTime = DateTime.MinValue;
            requestInProgress = false;
            waitingResponses.Clear();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ElephantSDK;

namespace RollicGames.Utils
{
    [Serializable]
    public class PostbidResolvedChain
    {
        public bool Enabled;
        public bool DisableUnitAfterConsecutiveFailCycles;
        public List<PostbidResolvedUnit> Units;

        public static bool TryFromAdChain(
            AdConfig.PostbidAdunitsConfig section,
            string idA,
            string idB,
            string idC,
            string postbidChainKey,
            out PostbidResolvedChain result)
        {
            result = null;
            if (section == null || !section.enabled)
            {
                return false;
            }

            var units = new List<PostbidResolvedUnit>();
            TryAddResolvedUnit(units, section.adunit_a, idA, $"{postbidChainKey}_adunit_a");
            TryAddResolvedUnit(units, section.adunit_b, idB, $"{postbidChainKey}_adunit_b");
            TryAddResolvedUnit(units, section.adunit_c, idC, $"{postbidChainKey}_adunit_c");
            if (units.Count == 0)
            {
                return false;
            }

            result = new PostbidResolvedChain
            {
                Enabled = true,
                DisableUnitAfterConsecutiveFailCycles = section.disable_unit_after_consecutive_fail_cycles,
                Units = units
            };
            return true;
        }

        private static void TryAddResolvedUnit(
            List<PostbidResolvedUnit> units,
            AdConfig.PostbidAdunitConfig slot,
            string adUnitId,
            string postbidAdUnitName)
        {
            if (slot == null || !slot.enabled || string.IsNullOrEmpty(adUnitId))
            {
                return;
            }

            units.Add(new PostbidResolvedUnit
            {
                Enabled = true,
                Order = slot.order,
                Multiplier = slot.multiplier,
                MaxRetry = slot.max_retry,
                MaxConsecutiveFailCycles = slot.max_consecutive_fail_cycles,
                AdUnitId = adUnitId,
                PostbidAdUnitName = postbidAdUnitName
            });
        }
    }

    [Serializable]
    public class PostbidResolvedUnit
    {
        public bool Enabled = true;
        public int Order;
        public double Multiplier = 1.0;
        public int MaxRetry = 1;
        public int MaxConsecutiveFailCycles;
        public string AdUnitId;
        public string PostbidAdUnitName;
    }

    [Serializable]
    public class PostBidAdUnitState
    {
        public int Index;
        public string Id;
        public bool IsActive;
        public double LastECPM;
        public int RetryAttempts;
        public int MaxRetryCount;
        public int ConsecutiveFailCycles;
        public int MaxConsecutiveFailCycles;
        public double FloorMultiplier;
        public string PostbidAdUnitName;
    }

    public class PostBidAdUnitManager
    {
        public PostBidConfig Config { get; }
        public Dictionary<string, bool> AdUnitStatus { get; }
        public Action<string, string, string> OnApplyFloorPrice;
		private int _cycleStartIndex;
        private IList<PostBidAdUnitState> Units => Config?.Units;
        
        public const int UnlimitedRetries = -1; // -1 = unlimited retry. First ad unit should always be -1
        public const int UnlimitedConsecutiveFailCycles = -1; // -1 = unlimited. First ad unit should always be -1

        private const string Tag = "POSTBID";

        public PostBidAdUnitManager(PostBidConfig config, Dictionary<string, bool> status)
        {
            Config = config;
            AdUnitStatus = status;
            _cycleStartIndex = 0;
        }

        public PostBidAdUnitState GetUnit(string adUnitId)
        {
            return Units?.FirstOrDefault(u => u.Id == adUnitId);
        }

        public PostBidAdUnitState GetNextUnit(PostBidAdUnitState unit)
        {
            if (unit == null || Units == null)
            {
                return null;
            }
            int nextIndex = unit.Index + 1;
            return nextIndex < Units.Count ? Units[nextIndex] : null;
        }

        public PostBidAdUnitState GetPreviousUnit(PostBidAdUnitState unit)
        {
            if (unit == null || Units == null || unit.Index <= 0)
            {
                return null;
            }
            return Units[unit.Index - 1];
        }

		public PostBidAdUnitState GetFirstUnit()
		{
    		if (Units == null || Units.Count <= 0)
        	{
				return null;
			}

    		return Units[_cycleStartIndex];
		}

		public PostBidAdUnitState GetChainFirstUnit()
		{
			if (Units == null || Units.Count <= 0)
			{
				return null;
			}
			return Units[0];
		}

        public bool IsFirstUnit(string adUnitId)
        {
            return Units != null && Units.Count > 0 && string.Equals(Units[0].Id, adUnitId);
        }

        public PostBidAdUnitState GetHighestPriorityReadyUnit(Func<string, bool> isReady)
        {
            if (!Config.IsFlowActive || isReady == null)
            {
                return null;
            }

		    for (int i = Units.Count - 1; i >= _cycleStartIndex; i--)
            {
                var u = Units[i];
                if (isReady(u.Id))
                {
                    return u;
                }
            }

            return null;
        }

        public PostBidAdUnitState GetAnyReadyUnit(Func<string, bool> isReady)
        {
            if (!Config.IsFlowActive || isReady == null)
            {
                return null;
            }

            for (int i = Units.Count - 1; i >= 0; i--)
            {
                var u = Units[i];
                if (isReady(u.Id))
                {
                    return u;
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates floor price for a unit based on the previous unit's eCPM and this unit's multiplier.
        /// floor = prev.eCPM * unit.FloorMultiplier
        /// </summary>
        public bool TryGetFloorPrice(PostBidAdUnitState unit, out string floorPrice)
        {
            floorPrice = null;

            if (unit == null || !Config.PostBidEnabled || Units == null || unit.Index == 0)
            {
                return false;
            }

            var prevUnit = Units[unit.Index - 1];
            if (prevUnit == null || prevUnit.LastECPM <= 0)
            {
                return false;
            }

            var mult = unit.FloorMultiplier > 0 ? unit.FloorMultiplier : 1.0;
            var floor = prevUnit.LastECPM * mult;
            if (floor <= 0)
            {
                return false;
            }

            floorPrice = floor.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
        
        /// <summary>
        /// Loads an ad with eCPM floor applied.
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <param name="loadAdFunc"></param>
        /// <returns></returns>
        public void LoadWithEcpmFloor(string adUnitId, Action<string> loadAdFunc)
        {
            if (Config.PostBidEnabled)
            {
                var floorKey = RemoteConfig.GetInstance().Get("max_price_floor_extra_param_key", "jC7Fp");
                var unit = GetUnit(adUnitId);
                if (unit != null)
                {
                    if (TryGetFloorPrice(unit, out var floorStr))
                    {
                        OnApplyFloorPrice?.Invoke(unit.Id, floorKey, floorStr);
                    }
                }
            }

            loadAdFunc?.Invoke(adUnitId);
        }

        /// <summary>
        /// Validates if the given eCPM is higher than previous unit's eCPM
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="ecpm"></param>
        /// <returns></returns>
        public bool IsEcpmValid(PostBidAdUnitState unit, double ecpm)
        {
            if (unit.Index == 0)
            {
                return true;
            }
            
            var prev = Units[unit.Index - 1];
            return prev.LastECPM > 0 && ecpm > prev.LastECPM;
        }

        public void UpdateLastEcpm(PostBidAdUnitState unit, double ecpm)
        {
            if (unit == null || ecpm <= 0 || double.IsNaN(ecpm) || double.IsInfinity(ecpm))
            {
                return;
            }

            unit.LastECPM = ecpm;
            unit.RetryAttempts = 0;
            unit.ConsecutiveFailCycles = 0;

            ElephantLog.Log(Tag, $"Updated eCPM: unit[{unit.Index}] {unit.Id}: {ecpm:F4}");
        }

        public bool IsUnitDisabledDueToConsecutiveFails(PostBidAdUnitState unit)
        {
            if (unit == null || !Config.DisableUnitAfterConsecutiveFailCycles || unit.Index == 0)
            {
                return false;
            }
            if (unit.MaxConsecutiveFailCycles == UnlimitedConsecutiveFailCycles || unit.MaxConsecutiveFailCycles < 0)
            {
                return false;
            }
            return unit.ConsecutiveFailCycles >= unit.MaxConsecutiveFailCycles;
        }

        public void AddConsecutiveFailCycle(PostBidAdUnitState unit)
        {
            if (unit == null || unit.Index == 0 || !Config.DisableUnitAfterConsecutiveFailCycles)
            {
                return;
            }
            if (unit.MaxConsecutiveFailCycles == UnlimitedConsecutiveFailCycles || unit.MaxConsecutiveFailCycles < 0)
            {
                return;
            }
            unit.ConsecutiveFailCycles++;
            ElephantLog.Log(Tag, $"Unit[{unit.Index}] {unit.Id} consecutive fail cycles: {unit.ConsecutiveFailCycles}/{unit.MaxConsecutiveFailCycles}");
        }

        public void ResetEcpm(PostBidAdUnitState unit)
        {
            if (unit == null)
            {
                return;
            }
            unit.RetryAttempts = 0;
        }

        public bool HasRetryAttemptsLeft(PostBidAdUnitState unit)
        {
            if (unit == null)
            {
                return false;
            }
            if (unit.Index == 0 || unit.MaxRetryCount == UnlimitedRetries)
            {
                unit.RetryAttempts++;
                return true;
            }
            if (unit.RetryAttempts < unit.MaxRetryCount)
            {
                unit.RetryAttempts++;
                return true;
            }
            unit.RetryAttempts = 0;
            return false;
        }

		public void ResetCycle()
		{
   			if (Units == null) 
			{
				return;
			}

    		foreach (var unit in Units)
    		{
        		unit.RetryAttempts = 0;
    		}

    		ElephantLog.Log(Tag, $"Cycle reset. Start index = {_cycleStartIndex}");
		}

        /// <summary>
        /// Sets which unit index will be used as the starting point for the next cycle
        /// </summary>
        /// <param name="adUnitId"></param>
        /// <returns></returns>
		public void SetCycleStartUnit(string adUnitId)
		{
    		var unit = GetUnit(adUnitId);
    		if (unit != null)
    		{
        		_cycleStartIndex = unit.Index;
        		ElephantLog.Log(Tag, $"Next cycle will start from unit[{unit.Index}] {unit.Id}");
    		}
		}
    }

    public class PostBidConfig
    {
        public bool PostBidEnabled { get; }
        public bool DisableUnitAfterConsecutiveFailCycles { get; }
        public List<PostBidAdUnitState> Units { get; }

        public bool IsFlowActive => PostBidEnabled && Units != null && Units.Count > 0;

    	public PostBidConfig(bool enabled, List<PostBidAdUnitState> units, bool disableUnitAfterConsecutiveFailCycles = false)
    	{
            PostBidEnabled = enabled;
        	Units = units ?? new List<PostBidAdUnitState>();
        	DisableUnitAfterConsecutiveFailCycles = disableUnitAfterConsecutiveFailCycles;
    	}
    }
}

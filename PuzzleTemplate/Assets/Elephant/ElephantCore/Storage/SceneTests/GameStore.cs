using System;
using System.Collections.Generic;

namespace ElephantSDK
{
    public enum PlayerStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public class Coin
    {
        public decimal Amount { get; set; } = 1000; // Default amount of coins
    }

    public class Achievement
    {
        public string AchievementId { get; set; } = "achievement01";
        public bool IsUnlocked { get; set; } = false;
        public DateTime UnlockTime { get; set; } = DateTime.Now;
    }

    public class PlayerStats
    {
        public int Experience { get; set; } = 0;
        public int Health { get; set; } = 100;
        public int Mana { get; set; } = 50;
    }

    public class PlayerSaveData : IElephantStorage
    {
        public string Name { get; set; } = "PlayerOne";
        public int Level { get; set; } = 1;
        public PlayerStatus Status { get; set; } = PlayerStatus.Active;
        public Coin Coins { get; set; } = new Coin(); // Direct property for coins
        public Dictionary<string, Achievement> Achievements { get; set; } = new Dictionary<string, Achievement>
        {
            { "FirstLogin", new Achievement() } // Default achievement
        };
        public PlayerStats Stats { get; set; } = new PlayerStats(); // Player statistics
    }
}
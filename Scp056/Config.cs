using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Interfaces;

namespace Scp056
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = false;
        public bool Debug { get; set; } = false;

        public string CustomInfo { get; set; } = "CustomInfo";

        public float Health { get; set; } = 100;

        public float ChanceToSpawn { get; set; } = 1;

        public float CoolDownAbilitySeconds { get; set; } = 30;

        public float AbilityTimeSeconds { get; set; } = 30;

        public string BroadcastFor056Spawn { get; set; } = "You are spawned";

        public List<ItemType> InventoryOf056 { get; set; } = new List<ItemType>
        {
            ItemType.KeycardGuard,
            ItemType.GunCOM15
        };

        public List<Dictionary<AmmoType, ushort>> Ammo056 { get; set; } = new List<Dictionary<AmmoType, ushort>>
        {
            (new Dictionary<AmmoType, ushort>
            {
                {AmmoType.Nato9, 24}
            })
        };
    }
}
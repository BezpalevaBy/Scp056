using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MapEditorReborn.Commands.UtilityCommands;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;
using UnityEngine;

namespace Scp056
{
    public class Handlers
    {
        public static bool IsAbilityActivated = false;
        public static bool Is056InRoundOrTried;
        public static bool Is056InRoundDebug;
        public static CoroutineHandle CoroutineHandle;
        public static List<Player> Player056 = new List<Player>();

        private static readonly List<RoleTypeId> ListScp = new List<RoleTypeId>
        {
            RoleTypeId.Scp049,
            RoleTypeId.Scp079,
            RoleTypeId.Scp096,
            RoleTypeId.Scp106,
            RoleTypeId.Scp173,
            RoleTypeId.Scp939
        };
        public static void OnPlayerSpawned(ChangingRoleEventArgs ev)
        {
            Timing.CallDelayed(1f, () =>
            {
                if(Player056.Count < 1) return;

                List<Player> players = (List<Player>)Player.List;

                players.Remove(Player056.FirstOrDefault());
                
                if (players.All(x => x.Role.Side is Side.Scp or Side.None or Side.Tutorial))
                {
                    Round.EndRound(true);
                }
            });
            
            if (ev.Player.IsConnected && Player056.Contains(ev.Player) && !ev.Player.IsAlive)
            {
                Round.IsLocked = false;
                Player056.Remove(ev.Player);;
                MEC.Timing.KillCoroutines(CoroutineHandle);
                
                Log.Debug("Kekw 056 is died or something");
            }
            
            if (Is056InRoundOrTried)
            {
                Log.Debug("There are 056 in round or it was tried to spawn");
                return;
            }

            if (!GetScpNewRole(ev.NewRole))
            {
                Log.Debug("There is no scp");
                return;
            }

            if (PluginMain.Instance.Config.ChanceToSpawn < Random.Range(0.0f, 1.0f))
            {
                Log.Debug("There is no 056 in round");
                Is056InRoundOrTried = true;
                return;
            }
            
            Set056PlayerRole(ev.Player, false);
            Round.IsLocked = true;

            Is056InRoundOrTried = true;
        }
        public static void Set056PlayerRole(Player player, bool isAbilityActivatedWell)
        {
            if (Is056InRoundDebug)
            {
                Log.Debug("There are 056 in round or it was tried to spawn(DEBUG)");
                return;
            }

            Is056InRoundDebug = true;
            Player056.Add(player);
            player.Role.Set(RoleTypeId.FacilityGuard);
            player.ResetInventory(PluginMain.Instance.Config.InventoryOf056);

            foreach (var dictionary in PluginMain.Instance.Config.Ammo056)
            {
                foreach (var keyValuePair in dictionary)
                {
                    player.SetAmmo(keyValuePair.Key, keyValuePair.Value);
                }
            }
            
            player.Position = Room.Get(RoomType.LczGlassBox).Position;
            player.Health = PluginMain.Instance.Config.Health;
            player.CustomInfo += "\n" + PluginMain.Instance.Config.CustomInfo;
            player.Broadcast(10, PluginMain.Instance.Config.BroadcastFor056Spawn);

            Set056PassiveAbilityOrActive(player, false);
        }
        public static void Set056PassiveAbilityOrActive(Player player, bool isAbilityActivatedWell)
        {
            if (CoroutineHandle.IsRunning)
            {
                Timing.KillCoroutines(CoroutineHandle);
                Log.Debug("Coroutine already running, killing coroutine");
            }
            CoroutineHandle = Timing.RunCoroutine(ItShouldBeEnemySkinAlways(player, isAbilityActivatedWell));
        }
        private static IEnumerator<float> ItShouldBeEnemySkinAlways(Player player, bool isAbilityActivatedWell)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(2.5f);

                if (!player.IsConnected || !player.IsAlive) yield break;

                foreach (var pl in Player.List)
                {
                    if (pl == player) continue;

                    NetworkWriterPooled writer = NetworkWriterPool.Get();
                    writer.WriteUShort(38952);
                    writer.WriteUInt(player.NetId);
                    writer.WriteRoleType(GetRoleType(pl, isAbilityActivatedWell));

                    byte unitId = 0;

                    if (GetRoleType(pl, isAbilityActivatedWell).GetSide() == (Side) Team.FoundationForces)
                    {
                        writer.WriteByte(unitId);
                    }
                    
                    Log.Debug($"Is ability activated? - {IsAbilityActivated}");
                    Log.Debug($"Player victim - {pl.Nickname}, player role - {pl.Role.Type}, player role side - {pl.Role.Side}, showed role {GetRoleType(pl, isAbilityActivatedWell)}");
                    
                    if (player.Role.Base is FpcStandardRoleBase standardRoleBase)
                    {
                        ushort syncH;
                        standardRoleBase.FpcModule.MouseLook.GetSyncValues(0, out syncH, out ushort _);
                        writer.WriteRelativePosition(new RelativePosition(player.ReferenceHub.transform.position));
                        writer.WriteUShort(syncH);
                    }

                    pl.Connection.Send(writer.ToArraySegment());
                }
            }
        }
        private static bool GetScpNewRole(RoleTypeId typeId)
        {
            if (ListScp.Contains(typeId)) return true;

            return false;
        }
        private static RoleTypeId GetRoleType(Player pl, bool isAbilityActivatedWell)
        {
            if (isAbilityActivatedWell)
            {
                switch (pl.Role.Side)
                {
                    case Side.Mtf:
                        return RoleTypeId.NtfSergeant;
                    case Side.ChaosInsurgency:
                        return RoleTypeId.ChaosRifleman;
                }
            }
            
            switch (pl.Role.Side)
            {
                case Side.Mtf:
                    return RoleTypeId.ChaosRifleman;
                case Side.ChaosInsurgency:
                    return RoleTypeId.NtfSergeant;
                default:
                    return RoleTypeId.FacilityGuard;
            }
        }

        public static void OnRestartingRound()
        {
            Player056 = null;
            IsAbilityActivated = false;
            Is056InRoundOrTried = false;
            Is056InRoundDebug = false;
            MEC.Timing.KillCoroutines(CoroutineHandle);
        }

        public static void OnHurting(HurtingEventArgs ev)
        { 
            if (!Player056.Contains(ev.Player)) return;
            if (ev.DamageHandler.Type is not DamageType.Scp049 or DamageType.Scp096 or DamageType.Scp106
                or DamageType.Scp173 or DamageType.Scp0492 or DamageType.Scp939) return;
            ev.IsAllowed = false;
        }
    }
}
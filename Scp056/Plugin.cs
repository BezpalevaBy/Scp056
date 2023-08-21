using Exiled.API.Features;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;

namespace Scp056
{
    public class PluginMain : Plugin<Config>
    {
        public override string Name => "Scp056";
        public override string Prefix => "Scp056";
        public override string Author => "Starlight/bezpa";

        public static PluginMain Instance;
        public override void OnEnabled()
        {
            Player.Hurting += Handlers.OnHurting;
            Server.RestartingRound += Handlers.OnRestartingRound;
            Player.ChangingRole += Handlers.OnPlayerSpawned;
            Instance = this;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Hurting -= Handlers.OnHurting;
            Server.RestartingRound -= Handlers.OnRestartingRound;
            Player.ChangingRole -= Handlers.OnPlayerSpawned;
            Instance = null;
            base.OnDisabled();
        }
    }
}
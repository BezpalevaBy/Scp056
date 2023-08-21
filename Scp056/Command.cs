using System;
using CommandSystem;
using Exiled.API.Features;
using MEC;

namespace Scp056;

[CommandHandler(typeof(ClientCommandHandler))]

public class Command056 : ICommand
{
    private DateTime _time = DateTime.MinValue;
    
    public string Command => "Ability";
    public string[] Aliases { get; } = {"Ab, 056, 056ab, ab056"};
    public string Description { get; } = "Ability of 056";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);

        if (!Handlers.Player056.Contains(player))
        {
            response = "You are not 056!";
            return false;
        }

        if (Handlers.IsAbilityActivated)
        {
            response = "There are already activated ability!";
            return false;
        }

        if (PluginMain.Instance.Config.AbilityTimeSeconds > 0 && _time + TimeSpan.FromSeconds(PluginMain.Instance.Config.CoolDownAbilitySeconds) > DateTime.Now)
        {
            Log.Debug($"Abilcoold + timenow - {_time + TimeSpan.FromSeconds(PluginMain.Instance.Config.CoolDownAbilitySeconds)}");
            Log.Debug($"timenow - {_time}");
            
            response = "There are cooldown ability";
            return false;
        }

        _time = DateTime.Now;

        Handlers.Set056PassiveAbilityOrActive(player, true);
        Handlers.IsAbilityActivated = true;

        if (PluginMain.Instance.Config.AbilityTimeSeconds < 1)
        {
            response = "Ability was activated, but there is no cooldown, so yeah";
            return true;
        }

        Timing.CallDelayed(PluginMain.Instance.Config.AbilityTimeSeconds, () =>
        {
            Handlers.Set056PassiveAbilityOrActive(player, false);
            Handlers.IsAbilityActivated = false;
            
            player.ShowHint("Ability ended");
        });
        
        response = "Ability activated";
        return true;
    }
}

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class SetCommand056 : ICommand
{
    public string Command => "Set056";
    public string[] Aliases => new[]{"Set056"}; 
    public string Description { get; } = "Debug command that set 056";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);

        if (Handlers.Is056InRoundDebug)
        {
            response = "056 already here";
            return false;
        }
        
        Handlers.Set056PlayerRole(player, false);
        
        response = "056 has been setted";
        return true;
    }
}
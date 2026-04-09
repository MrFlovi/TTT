using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Player;

namespace TTT.Roles.Shop;

public class CreditManager(IRoleService roleService) : IPluginBehavior
{
    private BasePlugin _plugin;
    
    public void Start(BasePlugin plugin)
    {
        _plugin = plugin;
        //plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        Console.WriteLine("CreditsManager Player Death Event triggered");
        CCSPlayerController? killer = @event.Attacker;
        CCSPlayerController? victim = @event.Userid;
        
        if (killer == null || victim == null) return HookResult.Continue;
        if (killer == victim) return HookResult.Continue;
        
        var attackerPlayer = roleService.GetPlayer(killer);
        var victimPlayer = roleService.GetPlayer(victim);
        
        if (attackerPlayer.PlayerRole() == Role.Traitor && victimPlayer.PlayerRole() != Role.Traitor)
        {
            if(victimPlayer.PlayerRole() == Role.Detective) attackerPlayer.AddCredits(2);
            else attackerPlayer.AddCredits(1);
            return HookResult.Continue;
        }
        
        if (attackerPlayer.PlayerRole() != Role.Traitor && victimPlayer.PlayerRole() == Role.Traitor)
        {
            attackerPlayer.AddCredits(2);
            return HookResult.Continue;
        }
        
        attackerPlayer.RemoveCredits(100);
        
        return HookResult.Continue;
    }

    
}
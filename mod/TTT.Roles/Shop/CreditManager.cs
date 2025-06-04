using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Player;

namespace TTT.Roles.Shop;

public class CreditManager : IPluginBehavior
{
    private readonly IPlayerService _playerService;

    private CreditManager(BasePlugin plugin, IPlayerService playerService)
    {
        _playerService = playerService;
    }
    
    public void Start(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    public static void Register(BasePlugin parent, IPlayerService service)
    {
        new CreditManager(parent, service);
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? killer = @event.Attacker;
        CCSPlayerController? victim = @event.Userid;
        
        if (killer == null || victim == null) return HookResult.Continue;
        if (killer == victim) return HookResult.Continue;
        
        var attackerPlayer = _playerService.GetPlayer(killer);
        var victimPlayer = _playerService.GetPlayer(victim);
        
        if (attackerPlayer.PlayerRole() == Role.Traitor && victimPlayer.PlayerRole() != Role.Traitor)
        {
            if(victimPlayer.PlayerRole() == Role.Detective) attackerPlayer.AddCredits(500);
            else attackerPlayer.AddCredits(250);
            return HookResult.Continue;
        }
        
        if (attackerPlayer.PlayerRole() != Role.Traitor && victimPlayer.PlayerRole() == Role.Traitor)
        {
            attackerPlayer.AddCredits(500);
            return HookResult.Continue;
        }
        
        attackerPlayer.RemoveCredits(500);
        
        return HookResult.Continue;
    }

    
}
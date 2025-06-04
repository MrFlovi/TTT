using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Player;
using TTT.Public.Behaviors;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Round;

namespace TTT.Roles;

public class RDMListener(IRoleService roleService) : IPluginBehavior
{
    private BasePlugin _plugin;
    public void Start(BasePlugin plugin)
    {
        _plugin = plugin;
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var killedPlayer = @event.Userid;

        if (killedPlayer == null || attacker == null) return HookResult.Continue;

        var attackerRole = roleService.GetRole(attacker);
        var killedRole = roleService.GetRole(killedPlayer);
        
        if (attackerRole == Role.Traitor && killedRole != Role.Traitor) return HookResult.Continue;
        if (killedRole == Role.Traitor) return HookResult.Continue;

        GamePlayer attackerPlayer = roleService.GetPlayer(attacker);
        attackerPlayer.RemoveKarma();
        
        attacker.CommitSuicide(true, true);
        attackerPlayer.SetKiller(attacker);
        
        return HookResult.Continue;
    }
}

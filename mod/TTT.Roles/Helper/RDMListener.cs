using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using McMaster.NETCore.Plugins;
using TTT.Player;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Round;
using PluginConfig = TTT.Public.Configuration.PluginConfig;

namespace TTT.Roles;

public class RDMListener(IRoleService roleService) : IPluginBehavior
{
    private BasePlugin _plugin;
    public void Start(BasePlugin plugin)
    {
        _plugin = plugin;
        //plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var killedPlayer = @event.Userid;

        if (!killedPlayer.IsReal() || !attacker.IsReal()) return HookResult.Continue;

        var attackerRole = roleService.GetRole(attacker);
        var killedRole = roleService.GetRole(killedPlayer);
        
        if (attackerRole == Role.Traitor || killedRole == Role.Traitor) return HookResult.Continue;

        GamePlayer attackerPlayer = roleService.GetPlayer(attacker);
        attackerPlayer.RemoveKarma();
        attackerPlayer.HasRDMed = true;

        if (PluginConfig.TttConfig.SuicideOnRDM)
        {
            attacker.CommitSuicide(true, true);
            attackerPlayer.SetKiller(attacker);
        }
        
        return HookResult.Continue;
    }
}

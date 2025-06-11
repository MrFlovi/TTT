using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Player;

namespace TTT.Roles;

public class ChatManager(IRoleService roleService) : IPluginBehavior
{
    public void Start(BasePlugin plugin)
    {
        plugin.AddCommandListener("say_team", OnSayTeam);
    }

    private HookResult OnSayTeam(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller == null || !caller.IsReal()) return HookResult.Continue;
        Role role = roleService.GetRole(caller);
        switch (role)
        {
            case Role.Innocent:
                return HookResult.Stop;
            case Role.Detective:
            {
                string message = $" {ChatColors.DarkBlue} DETECTIVE {caller.PlayerName} {info.GetArg(1)}";
                foreach (CCSPlayerController? player in roleService.GetDetectives())
                {
                    player?.PrintToChat(message);
                }
                break;
            }
            case Role.Traitor:
            {
                string message = $" {ChatColors.DarkRed} TRAITOR {caller.PlayerName} {info.GetArg(1)}";
                foreach (CCSPlayerController? player in roleService.GetTraitors())
                {
                    player?.PrintToChat(message);
                }
                break;
            }
            case Role.Unassigned:
                return HookResult.Continue;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return HookResult.Handled;
    }
}
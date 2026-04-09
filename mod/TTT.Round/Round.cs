using System.Drawing;
using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Public.Formatting;

namespace TTT.Round;

public class Round
{
    private readonly IRoleService _roleService;
    private float _graceTime;

    public Round(IRoleService roleService)
    {
        _roleService = roleService;
        _graceTime = PluginConfig.TttConfig.GraceTime * 64;
    }

    public void Tick()
    {
        _graceTime--;
        
        var players = Utilities.GetPlayers()
            .Where(player => player.IsReal())
            .ToList();
        
        var formattedColor = $"<font color=\"#{Color.Yellow.R:X2}{Color.Yellow.G:X2}{Color.Yellow.B:X2}\">";
        
        foreach (var player in players)
        {
            Server.NextFrame(() =>
            {
                player?.PrintToCenterHtml(
                    $"{formattedColor}<b>[TTT] Game is starting in {Math.Floor(_graceTime / 64)} seconds</b></font>");
            });
        }
    }

    public float GraceTime()
    {
        return _graceTime;
    }

    public void Start()
    { 
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()).Where(player => !player.PawnIsAlive).Where(player => player.Team is CsTeam.Terrorist or CsTeam.CounterTerrorist))
        {
           player.Respawn();
        }
        
        _roleService.AddRoles();
        Server.NextFrame(() => Server.PrintToChatAll(StringUtils.FormatTTT($"A new round has started! {_roleService.GetTraitors().Count} traitors.")));
        SendTraitorMessage();
        SendDetectiveMessage();
        SendInnocentMessage();
    }

    private void SendInnocentMessage()
    {
        foreach (var player in _roleService.GetInnocents())
        {
            if (!player.IsReal()) continue;
            Server.NextFrame(() => player.PrintToChat(StringUtils.FormatTTT("You are now an innocent")));
        }
    }
    
    private void SendTraitorMessage()
    {
        var traitors = _roleService.GetTraitors();
        
        foreach (var traitor in traitors)
        {
            if (!traitor.IsReal()) continue;
            Server.NextFrame(() =>
            {
                if (traitor.IsReal())
                {
                    traitor.PrintToChat(StringUtils.FormatTTT("You are a Traitor"));
                    traitor.PrintToChat(StringUtils.FormatTTT("Other Traitors:"));
                    foreach (var player in traitors)
                    {
                        if (player.IsReal())
                        {
                            var message = StringUtils.FormatTTT(Role.Traitor.FormatStringFullAfter(player.PlayerName));
                            traitor.PrintToChat(message);
                        }
                    }
                }
            });
        }
    }

    private void SendDetectiveMessage()
    {
        var detectives = _roleService.GetDetectives();
        
        foreach (var detective in detectives)
        {
            if (!detective.IsReal()) continue;
            Server.NextFrame(() =>
            {
                detective.PrintToChat(StringUtils.FormatTTT("You are a Detective"));
                detective.PrintToChat(StringUtils.FormatTTT("Detective:"));
                foreach (var player in detectives)
                {
                    if (player.IsReal())
                    {
                        var message = StringUtils.FormatTTT(Role.Detective.FormatStringFullAfter(" " + player.PlayerName));
                        detective.PrintToChat(message);
                    }
                }
            });
        }
    }
}
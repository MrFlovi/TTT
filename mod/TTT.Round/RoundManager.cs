using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using TTT.Public.Action;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Shop.Items.Traitor;

namespace TTT.Round;

public class RoundManager : IRoundService
{
    private readonly IRoleService _roleService;
    private readonly LogsListener _logs;
    private Round? _round;
    private RoundStatus _roundStatus = RoundStatus.Paused;

    private int _roundTimeElapsedSeconds = 0;

    public RoundManager(IRoleService roleService, BasePlugin plugin)
    {
        _roleService = roleService;
        _logs = new LogsListener(roleService, plugin);
        plugin.RegisterListener<Listeners.OnTick>(TickWaiting);
        
        plugin.AddCommandListener("jointeam", (player, info) =>
        {
            if (_roundStatus != RoundStatus.Started || player == null || !player.IsReal() || _roleService.GetRole(player) != Role.Unassigned) return HookResult.Continue;
            Server.NextFrame(() => player?.CommitSuicide(false, true));

            return HookResult.Continue;
        }, HookMode.Pre);
        
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(hook =>
        {
            var ent = hook.GetParam<CEntityInstance>(0);
            
            if (!ent.IsValid || ent.DesignerName is not "player") return HookResult.Continue;
            return _roundStatus != RoundStatus.Waiting ? HookResult.Continue : HookResult.Stop;
        }, HookMode.Pre);
        
        plugin.AddTimer(1, () =>
        {
            _roundTimeElapsedSeconds++;

            if (_roundTimeElapsedSeconds == PluginConfig.TttConfig.GiveWallhackTimeSeconds)
            {
                foreach (CCSPlayerController? controller in _roleService.GetTraitors())
                {
                    if (controller == null || controller.IsReal()) continue;
                    
                    _roleService.GetPlayer(controller).AddItem(new WallHackItem());
                }
            }
            
            if (_roundStatus == RoundStatus.Started && Utilities.GetPlayers().Count(player => player.PawnIsAlive) == 1)
            {
                ForceEnd();
            }

            var traitorCount = _roleService.GetTraitors().Count(player => player != null && player.PawnIsAlive);
            var innocentCount = _roleService.GetInnocents().Count(player => player != null && player.PawnIsAlive);
            var detectiveCount = _roleService.GetDetectives().Count(player => player != null && player.PawnIsAlive);

            if (_roundStatus == RoundStatus.Started && (traitorCount == 0 || innocentCount + detectiveCount == 0))
            {
                ForceEnd();
            }
        }, TimerFlags.REPEAT);
    }


    public RoundStatus GetRoundStatus()
    {
        return _roundStatus;
    }

    public void SetRoundStatus(RoundStatus roundStatus)
    {
        _roundStatus = roundStatus;
        
        switch (roundStatus)
        {
            case RoundStatus.Ended:
                ForceEnd();
                break;
            case RoundStatus.Waiting:
                _round = new Round(_roleService);
                break;
            case RoundStatus.Started:
                ForceStart();
                break;
            case RoundStatus.Paused:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(roundStatus), roundStatus, "Invalid round status.");
        }
    }

    public void TickWaiting()
    {
        if (_round == null)
        {
            _round = new Round(_roleService);
            return;
        }

        if (_roundStatus != RoundStatus.Waiting) return;

        _round.Tick();

        if (_round.GraceTime() != 0) return;
        
        
        if (Utilities.GetPlayers().Where(player => player is { IsValid: true, PawnIsAlive: true } && player.IsReal()).ToList().Count <= 2)
        {
            Server.PrintToChatAll(StringUtils.FormatTTT("Not enough players to start the round. Round has been ended."));
            _roundStatus = RoundStatus.Paused;
            return; 
        }
        
        SetRoundStatus(RoundStatus.Started); 
        
    }

    public void ForceStart()
    {
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsReal()).Where(player => player.IsReal())
                     .ToList()) player.VoiceFlags = VoiceFlags.Normal;
        _roundTimeElapsedSeconds = 0;
        _round!.Start(); 
    }

    public void ForceEnd()
    {
        if (_roundStatus == RoundStatus.Ended) return;
        _roundStatus = RoundStatus.Ended;
        _logs.IncrementRound();
        Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules!.TerminateRound(10,
            RoundEndReason.RoundDraw);
    }
    
    public ILogsService GetLogsService()
    {
        return _logs;
    }
}

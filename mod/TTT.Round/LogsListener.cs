using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using TTT.Public.Action;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;

namespace TTT.Round;

public class LogsListener : ILogsService, IPluginBehavior
{
    private readonly HashSet<IAction> _actions = new();
    private readonly IRoleService _roleService;
    private int _roundId = 0;

    public LogsListener(IRoleService roleService, BasePlugin parent, int roundId = 0)
    {
        _roleService = roleService;
        _roundId = roundId;
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage);
    }
    
    public void Start(BasePlugin plugin)
    {
        plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
    }

    public void IncrementRound()
    {
        _roundId++;
    }

    public int GetCurrentRoundId()
    {
        return _roundId;
    }

    public void AddLog(IAction action)
    {
        _actions.Add(action);
    }
    
    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? killer = @event.Attacker;
        CCSPlayerController? victim = @event.Userid;
        
        if (killer == null || victim == null) return HookResult.Continue;
        if (!killer.IsReal() || !victim.IsReal()) return HookResult.Continue;
        
        _actions.Add(new KillAction(new Tuple<CCSPlayerController, Role>(killer, _roleService.GetRole(killer)),
            new Tuple<CCSPlayerController, Role>(victim, _roleService.GetRole(victim))
        ));
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info)
    {
        var killer = @event.Attacker;
        var deadPlayer = @event.Userid;
        var damage = @event.DmgHealth;

        if (killer == null || deadPlayer == null) return HookResult.Continue;

        if (!killer.IsReal() || !deadPlayer.IsReal()) return HookResult.Continue;
        //var hitbox = @event.Hitgroup; wip

        _actions.Add(new DamageAction(new Tuple<CCSPlayerController, Role>(killer, _roleService.GetRole(killer)),
            new Tuple<CCSPlayerController, Role>(deadPlayer, _roleService.GetRole(deadPlayer)),
            damage,
            0
        ));

        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (Utilities.GetPlayers().Count(player => player.PawnIsAlive) < 1) return HookResult.Continue;
        
        var chatMessage = CreateChatMessage();
        
        foreach (var player in Utilities.GetPlayers().Where(player => player.IsValid).Where(player => player.IsReal() && !player.IsBot)
                     .ToList()) Server.NextFrame(() => player?.PrintToConsole(chatMessage));
        
        Server.PrintToConsole(chatMessage);

        _actions.Clear();

        return HookResult.Continue;
    }

    private void Save()
    {
        
    }

    private string CreateChatMessage()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"[TTT] Logs round {_roundId}");

        foreach (var action in _actions) builder.AppendLine(action.ActionMessage());

        builder.AppendLine("[TTT] Logs ended!");

        return builder.ToString();
    }
}
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using TTT.Player;
using TTT.Public.Behaviors;
using TTT.Public.Configuration;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Role;
using TTT.Public.Mod.Round;
using TTT.Roles.Shop;
using TTT.Round;

namespace TTT.Roles;

public class RoleManager : PlayerHandler, IRoleService, IPluginBehavior
{
    private BasePlugin _plugin;
    
    private const int MaxDetectives = 3;

    private int _innocentsLeft;
    private IRoundService? _roundService;
    private int _traitorsLeft;
    private InfoManager? _infoManager;
    private MuteManager? _muteManager;
    private EntityGlowManager? _entityGlowManager;
    
    public void Start(BasePlugin parent)
    {
        _plugin = parent;
        
        _roundService = new RoundManager(this, parent);
        _infoManager = new InfoManager(this, _roundService, parent);
        _muteManager = new MuteManager(parent);
        _entityGlowManager = new EntityGlowManager(parent, this);
        ModelHandler.RegisterListener(parent);
        ShopManager.Register(parent, this); 
        CreditManager.Register(parent, this);
        
        parent.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
        parent.RegisterEventHandler<EventRoundPrestart>(OnRoundPrepare);
        parent.RegisterEventHandler<EventRoundFreezeEnd>(OnRoundStart);
        parent.RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        parent.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        parent.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Pre);
        //parent.RegisterEventHandler<EventPlayerDeath>(OnAfterPlayerDeath);
        parent.RegisterEventHandler<EventPlayerHurt>(OnPlayerDamage, HookMode.Pre);
    }
    
    public void SetMoveType(CCSPlayerController? player, MoveType_t moveType)
    {
        if (player == null || player.PlayerPawn.Value == null) return;
        player.PlayerPawn.Value.MoveType = moveType;
        player.PlayerPawn.Value.ActualMoveType = moveType;
        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_MoveType");
    }

    [GameEventHandler]
    private HookResult OnRoundPrepare(EventRoundPrestart @event, GameEventInfo info)
    {
        _plugin.Logger.Log(LogLevel.Debug, "PRE ROUND");
        
        foreach (CCSPlayerController controller in Utilities.GetPlayers())
        {
            controller.SwitchTeam(CsTeam.Terrorist);
            
            CCSPlayerPawn? pawn = controller.PlayerPawn.Value;
            if (pawn == null) continue;
            
            pawn.BotAllowActive = true;
            controller.RemoveWeapons();
            
            // controller.TakesDamage = true;
            // pawn.Render = Color.FromArgb(255, 255, 255, 255);
            // pawn.RenderMode = RenderMode_t.kRenderNormal;
            // Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");
            // SetMoveType(controller, MoveType_t.MOVETYPE_WALK);
            // Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_MoveType");
            // pawn.MaxHealth = 100;
            // pawn.Health = pawn.MaxHealth;
        }
        
        Server.NextFrame(() =>
        {
            foreach (CCSPlayerController controller in Utilities.GetPlayers())
            {
                CCSPlayerPawn? pawn = controller.PlayerPawn.Value;
                if (pawn == null) continue;
                controller.GiveNamedItem("weapon_knife");
                controller.GiveNamedItem("weapon_glock");
            }
        });

        return HookResult.Continue;
    }
    
    [GameEventHandler]
    private HookResult OnRoundStart(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        _roundService?.SetRoundStatus(RoundStatus.Waiting);
        foreach (CCSPlayerController controller in Utilities.GetPlayers().Where(player => player.IsReal() && player.Team != CsTeam.None || player.Team != CsTeam.Spectator))
        {
            // player.GiveNamedItem("weapon_knife");
            // player.GiveNamedItem("weapon_glock");
            CCSPlayerPawn? pawn = controller.PlayerPawn.Value;
            if (pawn?.WeaponServices?.MyWeapons == null) continue;
            foreach (CHandle<CBasePlayerWeapon> handle in pawn.WeaponServices.MyWeapons)
            {
                if (!handle.IsValid)
                    continue;

                var weapon = handle.Value;
                if (weapon == null || !weapon.IsValid)
                    continue;

                if (weapon.DesignerName == "weapon_c4")
                {
                    weapon.Remove();
                }
            }

        }
        
        foreach (var target in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("weapon_c4"))
        {
            target.Remove();
        }

        foreach (var target in Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_bomb_target"))
        {
            target.Remove();
        }
        
        
        
        ConVar.Find("mp_roundtime")!.SetValue((PluginConfig.TttConfig.RoundTimeSeconds + PluginConfig.TttConfig.GraceTime) / 60.0f);
        ConVar.Find("mp_solid_teammates")!.SetValue(0);
        
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (Utilities.GetPlayers().Count(player => player.IsReal() && !player.IsBot && 
                                                   (player.Team != CsTeam.None || player.Team == CsTeam.Spectator)) < 3)
        {
            _roundService?.ForceEnd();
        }

        if (@event.Userid == null) return HookResult.Stop;
        _plugin.Logger.Log(LogLevel.Debug, "OnPlayerConnect - Creating a player");
        CreatePlayer(@event.Userid);
        
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    private HookResult OnPlayerDamage(EventPlayerHurt @event, GameEventInfo info)
    {
        var killer = @event.Attacker;
        var victim = @event.Userid;
        // var damage = @event.DmgHealth;
    
        // _plugin.Logger.Log(LogLevel.Debug, $"SOMEONE RECEIVED {damage} DAMAGE");
        
        if (killer == null || victim == null) return HookResult.Continue;
        
        if (!killer.IsReal() || !victim.IsReal()) return HookResult.Continue;
        
        // CCSPlayerPawn victimPawn = victim.PlayerPawn.Value;
        //
        // if (victimPawn.Health <= 0)
        // {
        //     // _plugin.Logger.Log(LogLevel.Debug, $"(Pre-)Health: {victimPawn.Health} and Damage: {@event.DmgHealth}");
        //     @event.DmgHealth = 0;
        //     
        //     PlayerDeath(killer, victim);
        // }
    
        return HookResult.Continue;
    }
    
    [GameEventHandler]
    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        info.DontBroadcast = true;

        CCSPlayerController? killer = @event.Attacker;
        CCSPlayerController? victim = @event.Userid;
        if (victim == null) return HookResult.Continue;
        
        // @event.Free();
        
        CCSPlayerPawn? victimPawn = victim.PlayerPawn.Value;
        if (victimPawn == null) return HookResult.Continue;
        Vector? pos = victimPawn.AbsOrigin;
        QAngle? angle = victimPawn.AbsRotation;
        if (pos == null || angle == null) return HookResult.Continue;
        
        Server.NextFrame(() =>
        {
            if (killer != null || victim != null || victimPawn != null)
            {
                CCSPlayerPawn? victimPawn = victim.PlayerPawn.Value;
                if (victimPawn == null) return;
        
                // victim.PlayerPawn.Value.ForceServerRagdoll = true;
                if (killer == null) victim.CommitSuicide(false, true);
        
                victim.RemoveWeapons();
                victim.ModifyScoreBoard();
                if (killer == null) GetPlayer(victim).SetKiller(killer);
                else GetPlayer(victim).SetKiller(victim);
                _muteManager?.Mute(victim);
        
                if (IsTraitor(victim)) _traitorsLeft--;
                if (IsDetective(victim) || IsInnocent(victim)) _innocentsLeft--;
                if (_traitorsLeft == 0 || _innocentsLeft == 0) Server.NextFrame(() => _roundService?.ForceEnd());
        
                Server.NextFrame(() =>
                {
                    if(victim != null && killer != null) SendDeathMessage(victim, killer);
                });
            }
        });
        
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (Utilities.GetPlayers().Count(player => player.PawnIsAlive) < 1) return HookResult.Continue;
        
        var players = Utilities.GetPlayers()
            .Where(player => player.IsValid).Where(player => player.IsReal() && !player.IsBot).ToList();

        Server.PrintToChatAll(StringUtils.FormatTTT(GetWinner().FormatStringFullAfter(" team has won!")));
        foreach (var player in players)
        {
            player.PrintToCenter(GetWinner().FormatStringFullAfter(" team has won!"));
        }

        Console.WriteLine("Clear and Dispose starting");
        Server.NextFrame(Clear);
        _muteManager?.UnMuteAll();
        _entityGlowManager?.Dispose();
        return HookResult.Continue;
    }

    [GameEventHandler]
    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null) return HookResult.Continue;
        Server.NextFrame(() =>
        {
            if(player != null) RemovePlayer(player);
            if (GetPlayers().Count == 0) _roundService?.SetRoundStatus(RoundStatus.Paused);
        });
        
        return HookResult.Continue;
    }
    
    public void AddRoles()
    {
        var eligible = Utilities.GetPlayers()
            .Where(player => player.IsReal())
            .Where(player => player.Team is not (CsTeam.Spectator or CsTeam.None))
            .ToList();

        var traitorCount = (int)Math.Floor(Convert.ToDouble(eligible.Count / PluginConfig.TttConfig.TraitorRatio));
        var detectiveCount = (int)Math.Floor(Convert.ToDouble(eligible.Count / PluginConfig.TttConfig.DetectiveRatio));

        _traitorsLeft = traitorCount;
        _innocentsLeft = eligible.Count - traitorCount;

        if (detectiveCount > MaxDetectives) detectiveCount = MaxDetectives;

        for (var i = 0; i < traitorCount; i++)
        {
            var chosen = eligible[Random.Shared.Next(eligible.Count)];
            eligible.Remove(chosen);
            AddTraitor(chosen);
        }
        
        for (var i = 0; i < detectiveCount; i++)
        {
            var chosen = eligible[Random.Shared.Next(eligible.Count)];//eligible.First(player => !player.IsBot);
            eligible.Remove(chosen);
            AddDetective(chosen);
        }

        AddInnocents(eligible);
        
        Server.RunOnTick(5, () =>
        {
            _entityGlowManager?.SetPlayersGlow(GetTraitors().ToList(), EntityGlowManager.TraitorGlowColor);
            _entityGlowManager?.SetPlayersGlow(GetInnocents().ToList(), EntityGlowManager.InnocentGlowColor);
            _entityGlowManager?.SetPlayersGlow(GetDetectives().ToList(), EntityGlowManager.DetectiveGlowColor);
        });
    }

    public HashSet<CCSPlayerController?> GetTraitors()
    {
        return Players().Where(player => player.PlayerRole() == Role.Traitor).Select(player => player.Player()).ToHashSet();
    }

    public HashSet<CCSPlayerController?> GetDetectives()
    {
        return Players().Where(player => player.PlayerRole() == Role.Detective).Select(player => player.Player()).ToHashSet();
    }

    public HashSet<CCSPlayerController?> GetInnocents()
    {
        return Players().Where(player => player.PlayerRole() == Role.Innocent).Select(player => player.Player()).ToHashSet();
    }
    

    public Role GetRole(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole();
    }

    public void AddTraitor(CCSPlayerController player)
    {
        GetPlayer(player).SetPlayerRole(Role.Traitor);
        player.SwitchTeam(CsTeam.Terrorist);
        player.PrintToCenter(Role.Traitor.FormatStringFullBefore("You are now a"));
        player.PrintToChat(Role.Traitor.FormatStringFullBefore("You are now a"));
        ModelHandler.SetModelNextServerFrame(player, ModelHandler.ModelPathTmPhoenix);
    }

    public void AddDetective(CCSPlayerController player)
    {
        GetPlayer(player).SetPlayerRole(Role.Detective);
        player.SwitchTeam(CsTeam.CounterTerrorist);
        player.PrintToCenter(Role.Detective.FormatStringFullBefore("You are now a"));
        player.GiveNamedItem(CsItem.Taser);
        ModelHandler.SetModelNextServerFrame(player, ModelHandler.ModelPathCtmSas);
    }

    public void AddInnocents(IEnumerable<CCSPlayerController> players)
    {
        foreach (var player in players)
        {
            GetPlayer(player).SetPlayerRole(Role.Innocent);
            player.PrintToCenter(Role.Innocent.FormatStringFullBefore("You are now an"));
            player.SwitchTeam(CsTeam.Terrorist);     
            ModelHandler.SetModelNextServerFrame(player, ModelHandler.ModelPathTmPhoenix);
        }
    }

    public bool IsDetective(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole() == Role.Detective;
    }

    public bool IsTraitor(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole() == Role.Traitor;
    }

    public void Clear()
    {
        Console.WriteLine("Start Clr");
        Clr();
        _infoManager?.Reset();
        foreach (var key in GetPlayers()) key.Value.SetPlayerRole(Role.Unassigned);
    }
    
    public bool IsInnocent(CCSPlayerController player)
    {
        return GetPlayer(player).PlayerRole() == Role.Innocent;
    }

    private Role GetWinner()
    {
        return _innocentsLeft == 0 ? Role.Traitor : Role.Innocent;
    }

    private void SendDeathMessage(CCSPlayerController playerWhoWasDamaged, CCSPlayerController attacker)
    {
        if(PluginConfig.TttConfig.AnnounceDeaths) Server.PrintToChatAll(StringUtils.FormatTTT($"{GetRole(playerWhoWasDamaged).FormatStringFullAfter(" has been found.")}"));
            
        if (attacker == playerWhoWasDamaged) return;
            
        attacker.ModifyScoreBoard();
            
        playerWhoWasDamaged.PrintToChat(StringUtils.FormatTTT(
            $"You were killed by {GetRole(attacker).FormatStringFullAfter(" " + attacker.PlayerName)}."));
        
        attacker.PrintToChat(PluginConfig.TttConfig.KnowRoleOfVictim
            ? StringUtils.FormatTTT(
                $"You killed {GetRole(playerWhoWasDamaged).FormatStringFullAfter(" " + playerWhoWasDamaged.PlayerName)}.")
            : StringUtils.FormatTTT($"You killed {playerWhoWasDamaged.PlayerName}."));
    }
}
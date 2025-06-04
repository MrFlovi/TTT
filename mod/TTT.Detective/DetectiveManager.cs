using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using TTT.Player;
using TTT.Public.Action;
using TTT.Public.Behaviors;
using TTT.Public.Extensions;
using TTT.Public.Formatting;
using TTT.Public.Mod.Detective;
using TTT.Public.Mod.Role;

namespace TTT.Detective;

public class DetectiveManager : IDetectiveService, IPluginBehavior
{
    private const int TaserAmmoType = 18;
    private readonly IRoleService _roleService;

    public DetectiveManager(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public void Start(BasePlugin parent)
    {
        parent.RegisterListener<Listeners.OnTick>(() =>
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers().Where(player => player.IsValid && player.IsReal())
                         .Where(player => (player.Buttons & PlayerButtons.Use) != 0)) OnPlayerUse(player);
        });

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnZeus, HookMode.Pre);

    }


    private HookResult OnZeus(DynamicHook hook)
    {
            CBaseEntity ent = hook.GetParam<CBaseEntity>(0);

            CCSPlayerController? playerWhoWasDamaged = player(ent);

            if (playerWhoWasDamaged == null) return HookResult.Continue;
                 
            CTakeDamageInfo info = hook.GetParam<CTakeDamageInfo>(1);
            
            CCSPlayerController? attacker = null;
            
            if (info.Attacker.Value != null)
            {
                CCSPlayerPawn playerWhoAttacked = info.Attacker.Value.As<CCSPlayerPawn>();

                attacker = playerWhoAttacked.Controller.Value?.As<CCSPlayerController>();   
                
            }

            if (info.BitsDamageType != DamageTypes_t.DMG_SHOCK) return HookResult.Continue;
            if (attacker == null) return HookResult.Continue;
                
            info.Damage = 0;
                
            GamePlayer targetRole = _roleService.GetPlayer(playerWhoWasDamaged);
            
            Server.NextFrame(() =>
            {
                if (attacker != null)
                {
                    if (_roleService.GetPlayer(attacker).PlayerRole() != Role.Detective)
                    {
                        attacker.PrintToChat(
                            StringUtils.FormatTTT(
                                $"Only a Detective can use this."));
                    } else {
                        attacker.PrintToChat(
                            StringUtils.FormatTTT(
                                $"You tased player {playerWhoWasDamaged.PlayerName} they are a {targetRole.PlayerRole().FormatRoleFull()}"));
                    
                    }
                }
            });
            
            //_roundService.GetLogsService().AddLog(new MiscAction("tased player " + targetRole.PlayerRole().FormatStringFullAfter(playerWhoWasDamaged.PlayerName), attacker));
                
            return HookResult.Stop;
    }
    
    private void OnPlayerUse(CCSPlayerController player)
    {
        IdentifyBody(player);
    }

    private void IdentifyBody(CCSPlayerController caller)
    {
        //add states

       if (_roleService.GetRole(caller) != Role.Detective) return;

       CCSPlayerController? entity = caller.GetClientRagdollAimTarget();

        if (entity == null) return;
        
        // if (entity.PawnIsAlive) return;
        
        GamePlayer player = _roleService.GetPlayer(entity);

        if (player.IsFound()) return;
        
        CCSPlayerController? killerEntity= player.Killer();
        
        string message;

        CCSPlayerController? plr = player.Player();
        if (plr == null) return;

        if (killerEntity == null || !killerEntity.IsReal())
            message = StringUtils.FormatTTT(player.PlayerRole()
                .FormatStringFullAfter($"{plr.PlayerName} was killed by world"));
        else
            message = StringUtils.FormatTTT(
                player.PlayerRole().FormatStringFullAfter($"{plr.PlayerName} was killed by ") +
                _roleService.GetRole(killerEntity).FormatStringFullAfter(killerEntity.PlayerName));


        player.SetFound(true);
        
        Server.NextFrame(() => { Server.PrintToChatAll(message); });
    }
    
    private static CCSPlayerController? player(CEntityInstance? instance)
    {
        if (instance == null)
        {
            return null;
        }

        if (instance.DesignerName != "player")
        {
            return null;
        }

        // grab the pawn index
        int playerIndex = (int)instance.Index;
        
        // grab player controller from pawn
        CCSPlayerPawn? playerPawn = Utilities.GetEntityFromIndex<CCSPlayerPawn>(playerIndex);

        // pawn and controller valid
        if (playerPawn == null || !playerPawn.IsValid || !playerPawn.OriginalController.IsValid)
        {
            return null;
        }

        // any further validity is up to the caller
        return playerPawn.OriginalController.Value;
    }
}
using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using TTT.Public.Extensions;

namespace TTT.Roles;

public class ModelHandler
{
    public static readonly string ModelPathCtmHeavy = "characters\\models\\ctm_heavy\\ctm_heavy.vmdl";
    public static readonly string ModelPathCtmSas = "characters\\models\\ctm_sas\\ctm_sas.vmdl";
    public static readonly string ModelPathTmHeavy = "characters\\models\\tm_phoenix_heavy\\tm_phoenix_heavy.vmdl";
    public static readonly string ModelPathTmPhoenix = "characters\\models\\tm_phoenix\\tm_phoenix.vmdl";

    public static void RegisterListener(BasePlugin plugin)
    {
        plugin.RegisterListener<Listeners.OnMapStart>(map =>
        {
            Server.PrecacheModel(ModelPathCtmHeavy);
            Server.PrecacheModel(ModelPathCtmSas);
            Server.PrecacheModel(ModelPathTmPhoenix);
            Server.PrecacheModel(ModelPathTmHeavy);
        });
    }
    
    public static void SetModel(CCSPlayerController player, string modelPath)
    {
        player.PlayerPawn.Value?.SetModel(modelPath);
    }
    
    public static void SetModelNextServerFrame(CCSPlayerController player, string model)
    {
        Server.NextFrame(() =>
        {
            SetModel(player, model);
        });
    }
    
    public static void HideWeapons(CCSPlayerController? player)
    {
        // only care if player is alive
        if (player == null || !player.IsReal())
            return;

        CCSPlayerPawn? pawn = player.PlayerPawn?.Value;
        if (pawn == null)
            return;

        var weapons = pawn.WeaponServices?.MyWeapons;
        if (weapons == null)
            return;
        
        //player.PrintToChat("Hiding weapons");

        foreach (var weaponOpt in weapons)
        {
            CBasePlayerWeapon? w = weaponOpt.Value;

            if (w == null)
                continue;
                
            player.PrintToChat($"Hiding {w.DesignerName}");
            Color newRenderW = Color.FromArgb(0, w.Render.R, w.Render.G, w.Render.B);

            w.RenderMode = RenderMode_t.kRenderTransAlpha;
            w.RenderFX = RenderFx_t.kRenderFxNone;
            w.Render = newRenderW;

            Utilities.SetStateChanged(w, "CBaseModelEntity", "m_nRenderMode");
            Utilities.SetStateChanged(w, "CBaseModelEntity", "m_nRenderFX");
            Utilities.SetStateChanged(w, "CBaseModelEntity", "m_clrRender");
        }
    }
}
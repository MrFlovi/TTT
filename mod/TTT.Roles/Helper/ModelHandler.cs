using System.Drawing;
using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using TTT.Public.Extensions;
using TTT.Shop.Items;

namespace TTT.Roles;

public class ModelHandler
{
    private BasePlugin _plugin;
    
    public static readonly string ModelPathCtmHeavy = "characters\\models\\ctm_heavy\\ctm_heavy.vmdl";
    public static readonly string ModelPathCtmSas = "characters\\models\\ctm_sas\\ctm_sas.vmdl";
    public static readonly string ModelPathTmHeavy = "characters\\models\\tm_phoenix_heavy\\tm_phoenix_heavy.vmdl";
    public static readonly string ModelPathTmPhoenix = "characters\\models\\tm_phoenix\\tm_phoenix.vmdl";
    
    public static readonly string GasGrenadeModel = "models\\weapon\\gasgrenade.vmdl";
    public static readonly string DartGunModel = "models\\weapon\\dartgun.vmdl";
    public static readonly string LaserBlasterModel = "models\\weapon\\laserblaster\\laserblaster.vmdl";

    private static Dictionary<uint, string> _lastWeaponHandles = new();

    public ModelHandler(BasePlugin plugin)
    {
        _plugin = plugin;
        
        plugin.RegisterListener<Listeners.OnMapStart>(map =>
        {
            Server.PrecacheModel(ModelPathCtmHeavy);
            Server.PrecacheModel(ModelPathCtmSas);
            Server.PrecacheModel(ModelPathTmPhoenix);
            Server.PrecacheModel(ModelPathTmHeavy);
            Server.PrecacheModel(GasGrenadeModel);
            Server.PrecacheModel(DartGunModel);
            Server.PrecacheModel(LaserBlasterModel);
        });
        plugin.RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
        {
            manifest.AddResource(GasGrenadeModel); 
            manifest.AddResource(DartGunModel); 
            manifest.AddResource(LaserBlasterModel); 
        });
        plugin.RegisterListener<Listeners.OnTick>(Tick);
        plugin.RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
        // CustomWeapon.OnMapStart();
    }
    
    public static void RegisterListener(BasePlugin plugin)
    {
        
    }

    public void Tick()
    {
        // foreach (var entity in Utilities.FindAllEntitiesByDesignerName<CDecoyProjectile>("decoy_projectile"))
        // {
        //     if (!entity.IsValid)
        //     {
        //         continue;
        //     }
        //
        //     if (entity.CBodyComponent?.SceneNode?.AbsRotation != new QAngle(0, 0, 0)) entity.Teleport(null, new QAngle(0, 0, 0), entity.Bounces == 1 ? new Vector(0, 0, 0) : null);
        //
        //     var nadeSceneNode = entity.CBodyComponent!.SceneNode!;
        //     entity.GravityScale = 2.0f;
        //     if (nadeSceneNode.GetSkeletonInstance().ModelState.ModelName != ModelHandler.GasGrenadeModel)
        //         entity.SetModel(ModelHandler.GasGrenadeModel);
        // }
        // foreach (var entity in Utilities.FindAllEntitiesByDesignerName<CDecoyGrenade>("decoy_grenade"))
        // {
        //     if (!entity.IsValid)
        //     {
        //         continue;
        //     }
        //
        //     if (entity.CBodyComponent?.SceneNode?.AbsRotation != new QAngle(0, 0, 0)) entity.Teleport(null, new QAngle(0, 0, 0), new Vector(0, 0, 0));
        //
        //     var nadeSceneNode = entity.CBodyComponent!.SceneNode!;
        //     
        //     if (nadeSceneNode.GetSkeletonInstance().ModelState.ModelName != ModelHandler.GasGrenadeModel)
        //         entity.SetModel(ModelHandler.GasGrenadeModel);
        // }
        //
        // foreach (var entity in Utilities.FindAllEntitiesByDesignerName<CWeaponSSG08>("weapon_ssg08"))
        // {
        //     if (!entity.IsValid)
        //     {
        //         continue;
        //     }
        //
        //     if (entity.CBodyComponent?.SceneNode?.AbsRotation != new QAngle(0, 0, 0)) entity.Teleport(null, new QAngle(0, 0, 0), new Vector(0, 0, 0));
        //
        //     var nadeSceneNode = entity.CBodyComponent!.SceneNode!;
        //     
        //     if (nadeSceneNode.GetSkeletonInstance().ModelState.ModelName != ModelHandler.DartGunModel)
        //         entity.SetModel(ModelHandler.DartGunModel);
        // }
        // foreach (var entity in Utilities.FindAllEntitiesByDesignerName<CAK47>("weapon_ak47"))
        // {
        //     if (!entity.IsValid)
        //     {
        //         continue;
        //     }
        //
        //     if (entity.CBodyComponent?.SceneNode?.AbsRotation != new QAngle(0, 0, 0)) entity.Teleport(null, new QAngle(0, 0, 0), new Vector(0, 0, 0));
        //
        //     var nadeSceneNode = entity.CBodyComponent!.SceneNode!;
        //     
        //     if (nadeSceneNode.GetSkeletonInstance().ModelState.ModelName != ModelHandler.LaserBlasterModel)
        //         entity.SetModel(ModelHandler.LaserBlasterModel);
        //}
        
        
        
        
        
        // foreach (var player in Utilities.GetPlayers())
        // {
        //     if (!player.IsReal()) continue;
        //
        //     string currentWeapon = player.GetActiveWeaponName();
        //     if (String.IsNullOrEmpty(currentWeapon)) continue;
        //
        //     uint playerId = player.Index;
        //
        //     // Only proceed if the weapon just changed
        //     if (!_lastWeaponHandles.ContainsKey(playerId) || _lastWeaponHandles[playerId] != currentWeapon)
        //     {
        //         _lastWeaponHandles[playerId] = currentWeapon;
        //
        //         // Only apply custom model if it's an AK-47
        //         switch (currentWeapon)
        //         {
        //             case "weapon_ak47":
        //                 player.
        //                 break;
        //         }
        //     }
        // }
    }
    
    public void OnEntityCreated(CEntityInstance entity)
    {
        // CustomWeapon.OnEntityCreated(entity);
        // if (entity == null || entity.Entity == null || !entity.IsValid || !entity.DesignerName.Contains("weapon_")) return;
        //
        // Server.NextFrame(() =>
        // {
        //     Console.WriteLine($"Weapon name: {entity.DesignerName}");
        //     var weapon = new CBasePlayerWeapon(entity.Handle);
        //
        //     if (!weapon.IsValid) return;
        //
        //     CCSWeaponBase _weapon = weapon.As<CCSWeaponBase>();
        //     if (_weapon == null) return;
        //
        //     if (_weapon.VData != null)
        //     {
        //         if (_weapon.VData?.WeaponType != CSWeaponType.WEAPONTYPE_PISTOL)
        //         {
        //             _weapon.VData!.PrimaryReserveAmmoMax = 1;
        //             _weapon.ReserveAmmo[0] = 1;
        //             Utilities.SetStateChanged(weapon.As<CCSWeaponBase>(), "CBasePlayerWeapon", "m_pReserveAmmo");
        //         }
        //         else
        //         {
        //             if (_weapon.DesignerName == "weapon_glock")
        //             {
        //                 _weapon.SetModel(DartGunModel);
        //                 if (_weapon.OwnerEntity.IsValid && _weapon.OwnerEntity.Value != null && _weapon.OwnerEntity.Value is CCSPlayerController)
        //                 {
        //                     CCSPlayerController player = _weapon.OwnerEntity.Value.As<CCSPlayerController>();
        //                     SetViewModel(player, DartGunModel);
        //                 }
        //                 
        //             }
        //             
        //             _weapon.VData.SecondaryReserveAmmoMax = 1;
        //             _weapon.ReserveAmmo[0] = 1;
        //             Utilities.SetStateChanged(weapon.As<CCSWeaponBase>(), "CBasePlayerWeapon", "m_pReserveAmmo");
        //         }
        //     }
        // });
    }
    
    /*private void SetViewModel(CCSPlayerController player, string model)
    {
        nint? handle = player.PlayerPawn.Value?.ViewModelServices?.Handle;
        if (handle == null || !handle.HasValue) return;

        CCSPlayer_ViewModelServices viewModelServices = new(handle.Value);
        nint ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        Span<nint> viewModels = MemoryMarshal.CreateSpan(ref ptr, 3);
        
        new CHandle<CBaseViewModel>(viewModels[0]).Value?.SetModel(model);
    }*/
    
    public void SetModel(CCSPlayerController player, string modelPath)
    {
        player.PlayerPawn.Value?.SetModel(modelPath);
    }
    
    public void SetModelNextServerFrame(CCSPlayerController player, string model)
    {
        Server.NextFrame(() =>
        {
            SetModel(player, model);
        });
    }
    
    public void HideWeapons(CCSPlayerController? player)
    {
        // only care if player is alive
        if (!player.IsReal())
            return;

        CCSPlayerPawn? pawn = player.PlayerPawn.Value;
        if (!pawn.IsReal())
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
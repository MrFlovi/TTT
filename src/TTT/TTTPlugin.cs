using System.Collections.Immutable;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTT.Public.Behaviors;
using TTT.Public.Configuration;
using TTT.Roles;

namespace TTT;

public class TTTPlugin : BasePlugin
{
    private readonly IServiceProvider _provider;
    private readonly WeaponEquipHandler _weaponEquipHandler;
    private IReadOnlyList<IPluginBehavior>? _extensions;
    private IServiceScope? _scope;

    /// <summary>
    ///     The TTT plugin.
    /// </summary>
    /// <param name="provider"></param>
    public TTTPlugin(IServiceProvider provider)
    {
        _provider = provider;
        _weaponEquipHandler = new WeaponEquipHandler(provider.GetRequiredService<ILogger<WeaponEquipHandler>>());
    }

    /// <inheritdoc />
    public override string ModuleName => "TTT";

    /// <inheritdoc />
    public override string ModuleVersion => "1.0.0";

    /// <inheritdoc />
    public override string ModuleAuthor => "NTM";


    /// <inheritdoc />
    public override void Load(bool hotReload)
    {
        Logger.LogInformation("[TTT] Loading...");
        
        PluginConfig.ReloadConfig();

        _scope = _provider.CreateScope();
        _extensions = _scope.ServiceProvider.GetServices<IPluginBehavior>()
            .ToImmutableList();

        Logger.LogInformation("[TTT] Found {@BehaviorCount} behaviors.", _extensions.Count);

        foreach (var extension in _extensions)
        {
            RegisterAllAttributes(extension);
            extension.Start(this);
            Logger.LogInformation("[TTT] Loaded behavior {@Behavior}", extension.GetType().FullName);
        }

        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);

        base.Load(hotReload);
    }

    public void OnTick()
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            CCSPlayerPawn? pawn = player.PlayerPawn.Value;
            if (pawn == null || !pawn.IsValid) continue;
            
            pawn.EntitySpottedState.Spotted = false;
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_entitySpottedState", Schema.GetSchemaOffset("EntitySpottedState_t", "m_bSpotted"));
             
            Span<uint> spottedByMask = pawn.EntitySpottedState.SpottedByMask;
            for (int i = 0; i < spottedByMask.Length; i++)
            { 
                spottedByMask[i] = 0;
            }
             
            Utilities.SetStateChanged(pawn, "CCSPlayerPawn", "m_entitySpottedState", Schema.GetSchemaOffset("EntitySpottedState_t", "m_bSpottedByMask"));
        }
    }

    /// <inheritdoc />
    public override void Unload(bool hotReload)
    {
        Logger.LogInformation("[TTT] Shutting down...");
        
        PluginConfig.ReloadConfig();

        if (_extensions != null)
            foreach (var extension in _extensions)
                extension.Dispose();

        _scope?.Dispose();
        WeaponEquipHandler.ClearStoredSubclasses();

        base.Unload(hotReload);
    }

    public void OnEntitySpawned(CEntityInstance entity)
    {
        if (!entity.IsValid) return;

        // Handle gas grenades (decoy projectiles)
        if (entity.DesignerName == "decoy_projectile")
        {
            var decoyProj = entity.As<CDecoyProjectile>();
            if (decoyProj == null) return;

            var nadeSceneNode = decoyProj.CBodyComponent?.SceneNode;
            if (nadeSceneNode == null) return;

            Server.NextFrame(() =>
            {
                if (nadeSceneNode.GetSkeletonInstance().ModelState.ModelName != ModelHandler.GasGrenadeModel)
                    decoyProj.SetModel(ModelHandler.GasGrenadeModel);
            });
            return;
        }

        // Handle decoy grenades
        if (entity.DesignerName == "weapon_decoy")
        {
            var decoyNade = entity.As<CDecoyGrenade>();
            if (decoyNade == null) return;

            var nadeSceneNode = decoyNade.CBodyComponent?.SceneNode;
            if (nadeSceneNode == null) return;

            if (nadeSceneNode.GetSkeletonInstance().ModelState.ModelName != ModelHandler.GasGrenadeModel)
                decoyNade.SetModel(ModelHandler.GasGrenadeModel);
            return;
        }

        // Handle weapon subclass equipping
        if (!entity.DesignerName.StartsWith("weapon_"))
            return;

        var weapon = entity.As<CBasePlayerWeapon>();
        if (weapon == null || !weapon.IsValid)
            return;

        Server.NextFrame(() => _weaponEquipHandler.ApplyOnWeaponSpawn(weapon, GetEquippedSubclass));
    }

    private string? GetEquippedSubclass(string weaponName)
    {
        // TODO: Implement retrieving the equipped subclass for the player from your storage system.
        // This should return the subclass name (e.g., "weapon_awp_dragon") or null if none is equipped.
        return null;
    }
}
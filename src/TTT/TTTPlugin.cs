using System.Collections.Immutable;
using System.ComponentModel.Design.Serialization;
using System.Data;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TTT.Public.Behaviors;
using TTT.Public.Configuration;

namespace TTT;

public class TTTPlugin : BasePlugin
{
    private readonly IServiceProvider _provider;
    private IReadOnlyList<IPluginBehavior>? _extensions;
    private IServiceScope? _scope;

    /// <summary>
    ///     The TTT plugin.
    /// </summary>
    /// <param name="provider"></param>
    public TTTPlugin(IServiceProvider provider)
    {
        _provider = provider;
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
            //	Register all event handlers on the extension object
            RegisterAllAttributes(extension);

            //	Tell the extension to start it's magic
            extension.Start(this);

            Logger.LogInformation("[TTT] Loaded behavior {@Behavior}", extension.GetType().FullName);
        }

        RegisterListener<Listeners.OnTick>(OnTick);

        base.Load(hotReload);
    }

    public void OnTick()
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            CCSPlayerPawn? pawn = player.PlayerPawn.Value;
            if (pawn == null) continue;
            
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

        //	Dispose of original extensions scope
        //	When loading again we will get a new scope to avoid leaking state.
        _scope?.Dispose();

        base.Unload(hotReload);
    }
}
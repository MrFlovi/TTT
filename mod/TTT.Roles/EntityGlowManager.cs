using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using TTT.Public.Mod.Role;

namespace TTT.Roles;

public class EntityGlowManager
{
    private IRoleService _roleService;
    
    public static Color TraitorGlowColor = Color.Red, DetectiveGlowColor = Color.Blue, InnocentGlowColor = Color.Lime;
    
    private List<CBaseModelEntity> _glowingEntities = new();

    public EntityGlowManager(BasePlugin plugin, IRoleService roleService)
    {
        _roleService = roleService;
        plugin.RegisterListener<Listeners.CheckTransmit>(RemoveGlow);
    }

    public void SetPlayersGlow(List<CCSPlayerController?> controllers, Color color)
    {
        foreach (var controller in controllers)
        {
            if (controller == null || controller.PlayerPawn.Value == null) continue;
            SetGlowing(controller.PlayerPawn.Value, color);
        }
    }

    private void RemoveGlow(CCheckTransmitInfoList infoList)
    {
        HashSet<CCSPlayerController?> traitors = _roleService.GetTraitors();
            
        foreach (var (info, player) in infoList)
        {
            if (player == null)
                continue;
            
            foreach (var model in _glowingEntities)
            {
                if (model.Handle == IntPtr.Zero) continue;
                if (model.Glow.Handle == IntPtr.Zero) continue;
                if (model?.Glow?.GlowColorOverride == null) continue;

                Color color = model.Glow.GlowColorOverride;
                
                // Dead players can see everyone's role glow
                if (!player.PawnIsAlive) continue;
                
                // All players see the Detectives glow
                if (color.ToArgb().Equals(DetectiveGlowColor.ToArgb())) continue;
                
                if (traitors.Contains(player))
                {
                    if (_roleService.GetPlayer(player).HasItem("wallhack") || color.ToArgb().Equals(TraitorGlowColor.ToArgb()))
                    {
                        continue;
                    }
                }
                
                info.TransmitEntities.Remove(model);
            }
        }
    }
    
    public void Dispose()
    {
        List<CBaseModelEntity> entities = _glowingEntities;
        
        Console.WriteLine("Remove entities now");
        foreach (var entity in _glowingEntities)
        {
            entity.Remove();
        }
        Console.WriteLine("Done removing entities now");
        
        _glowingEntities.Clear();
        
        entities.Clear();
    }

    private void SetGlowing(CCSPlayerPawn pawn, Color color)
    {
        CBaseModelEntity? modelGlow = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        CBaseModelEntity? modelRelay = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        if (modelGlow == null || modelRelay == null)
        {
            return;
        }

        string modelName = pawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;

        modelRelay.SetModel(modelName);
        modelRelay.Spawnflags = 256u;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.DispatchSpawn();

        modelGlow.SetModel(modelName);
        modelGlow.Spawnflags = 256u;
        modelGlow.DispatchSpawn();

        modelGlow.Glow.GlowColorOverride = color;
        modelGlow.Glow.GlowRange = 5000;
        modelGlow.Glow.GlowTeam = -1;
        modelGlow.Glow.GlowType = 3;
        modelGlow.Glow.GlowRangeMin = 100;

        modelRelay.AcceptInput("FollowEntity", pawn, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");

        _glowingEntities.Add(modelGlow);
        _glowingEntities.Add(modelRelay);
    }
}
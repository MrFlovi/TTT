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

    private Dictionary<CBaseModelEntity, Color> _glowingEntities = new();

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

            foreach (var entry in _glowingEntities)
            {
                var model = entry.Key;
                var color = entry.Value;
                
                if (model.Handle == IntPtr.Zero) continue;
                
                // Dead players can see everyone's role glow
                if (!player.PawnIsAlive) continue;
                // All players see the Detectives glow
                
                
                if (color.ToArgb() == DetectiveGlowColor.ToArgb())
                {
                    //Console.WriteLine("Continue Detective");
                    continue;
                }

                if (traitors.Contains(player))
                {
                    if (_roleService.GetPlayer(player).HasItem("Wall Hack") ||
                        color.ToArgb() == TraitorGlowColor.ToArgb())
                    {
                        //Console.WriteLine("Continue Traitor");
                        continue;
                    }
                }
                
                info.TransmitEntities.Remove(model);
            }
        }
    }

    public void Dispose()
    {
        Dictionary<CBaseModelEntity, Color> entities = _glowingEntities;

        Console.WriteLine("Remove entities now");
        foreach (var entity in _glowingEntities.Keys)
        {
            if(entity.IsValid) entity.Remove();
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
        modelRelay.Teleport(pawn.AbsOrigin, pawn.AbsRotation, pawn.AbsVelocity);
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");
        modelGlow.Teleport(pawn.AbsOrigin, pawn.AbsRotation, pawn.AbsVelocity);
        
        _glowingEntities.Add(modelGlow, color);
        _glowingEntities.Add(modelRelay, color);
    }
}
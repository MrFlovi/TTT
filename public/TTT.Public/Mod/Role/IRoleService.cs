using CounterStrikeSharp.API.Core;
using TTT.Public.Player;

namespace TTT.Public.Mod.Role;

public interface IRoleService : IPlayerService
{
    Role GetRole(CCSPlayerController player);
    void AddRoles();
    HashSet<CCSPlayerController?> GetTraitors();
    HashSet<CCSPlayerController?> GetDetectives();
    HashSet<CCSPlayerController?> GetInnocents();
    bool IsDetective(CCSPlayerController player);
    bool IsTraitor(CCSPlayerController player);
    void AddDetective(CCSPlayerController player);
    void AddTraitor(CCSPlayerController player);
    void AddInnocents(IEnumerable<CCSPlayerController> players);
    void Clear();
}

public enum Role
{
    Traitor = 0,
    Detective = 1,
    Innocent = 2,
    Unassigned = 3
}
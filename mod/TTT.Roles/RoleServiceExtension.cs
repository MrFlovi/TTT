using Microsoft.Extensions.DependencyInjection;
using TTT.Public.Extensions;
using TTT.Public.Mod.Role;
using TTT.Roles.Commands;
using TTT.Roles.Shop;

namespace TTT.Roles;

public static class RoleServiceExtension
{
    public static void AddTTTRoles(this IServiceCollection collection)
    {
        collection.AddPluginBehavior<IRoleService, RoleManager>();
        collection.AddPluginBehavior<RDMListener>();
        collection.AddPluginBehavior<CreditManager>();
        collection.AddPluginBehavior<ChatManager>();
        //collection.AddPluginBehavior<RolesCommand>();
    }
}
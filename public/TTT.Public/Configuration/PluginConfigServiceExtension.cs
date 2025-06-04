using Microsoft.Extensions.DependencyInjection;
using TTT.Public.Extensions;
using TTT.Public.Mod.Config;

namespace TTT.Public.Configuration;

public static class PluginConfigServiceExtension
{
    public static void AddConfigBehavior(this IServiceCollection collection)
    {
        collection.AddPluginBehavior<IConfigService, PluginConfig>();
    }
}
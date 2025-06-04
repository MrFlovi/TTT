using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.DependencyInjection;
using TTT.Detective;
using TTT.Public.Configuration;
using TTT.Roles;

namespace TTT;

public class TTTServiceCollection : IPluginServiceCollection<TTTPlugin>
{
    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTTTRoles();
        serviceCollection.AddDetectiveBehavior();
        serviceCollection.AddConfigBehavior();
    }
}
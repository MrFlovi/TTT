using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;

namespace TTT.Public.Configuration;

public class PluginConfig : BasePluginConfig, IPluginConfig<PluginConfig>
{
    public PluginConfig Config { get; set; }
    public static PluginConfig TTTConfig { get; set; } = new();

    [JsonPropertyName("debug_mode")]
    public bool DebugMode { get; set; } = false; // TODO: Switch to ConVar when supported

    [JsonPropertyName("grace_time")] public float GraceTime { get; set; } = 15;
    [JsonPropertyName("traitor_ratio")] public int TraitorRatio { get; set; } = 3;
    [JsonPropertyName("detective_ratio")] public int DetectiveRatio { get; set; } = 8;
    [JsonPropertyName("block_suicide")] public bool BlockSuicide { get; set; } = false;
    
    [JsonPropertyName("announce_deaths")] public bool AnnounceDeaths { get; set; } = false;
    [JsonPropertyName("know_role_of_victim")] public bool KnowRoleOfVictim { get; set; } = false;

    [JsonPropertyName("starting_secondary")]
    public string StartingSecondary { get; set; } = "weapon_glock";

    [JsonPropertyName("starting_primary")] public string StartingPrimary { get; set; } = ""; // weapon_nova
    
    public static void ReloadConfig()
    {
        string configPath = TTTConfig.GetConfigPath();
        
        // initialize config if file doesn't exist yet
        if (!System.IO.File.Exists(configPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            FileStream stream = System.IO.File.Create(configPath);
            stream.Close();
            TTTConfig.Update();
        }
        TTTConfig.Reload();
    }

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
        Console.WriteLine("OVERWRITTEN!"); 
    }
}
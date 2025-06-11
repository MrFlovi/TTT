using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Extensions;
using TTT.Public.Behaviors;
using TTT.Public.Mod.Config;

namespace TTT.Public.Configuration;

public class PluginConfig : BasePluginConfig, IPluginConfig<PluginConfig>, IPluginBehavior, IConfigService
{
    public PluginConfig Config { get; set; } = null!;
    public static PluginConfig TttConfig { get; set; } = new();

    [JsonPropertyName("debug_mode")]
    public bool DebugMode { get; set; } = false; // TODO: Switch to ConVar when supported

    [JsonPropertyName("grace_time")] public float GraceTime { get; set; } = 15;
    [JsonPropertyName("round_time_seconds")] public int RoundTimeSeconds { get; set; } = 180;
    [JsonPropertyName("give_wallhack_time_seconds")] public int GiveWallhackTimeSeconds { get; set; } = 120; // the time after which all traitors receive wallhack for free
    [JsonPropertyName("traitor_ratio")] public int TraitorRatio { get; set; } = 3;
    [JsonPropertyName("detective_ratio")] public int DetectiveRatio { get; set; } = 8;
    [JsonPropertyName("block_suicide")] public bool BlockSuicide { get; set; } = false;
    
    [JsonPropertyName("announce_deaths")] public bool AnnounceDeaths { get; set; } = false;
    [JsonPropertyName("know_role_of_victim")] public bool KnowRoleOfVictim { get; set; } = false; // this setting is only visual, you can always make role out, due to glow, or changes in money
    [JsonPropertyName("suicide_on_rdm")] public bool SuicideOnRDM { get; set; } = false;
    [JsonPropertyName("clear_money_on_rdm")] public bool ClearMoneyOnRDM { get; set; } = false;

    [JsonPropertyName("starting_secondary")]
    public string StartingSecondary { get; set; } = "weapon_glock";

    [JsonPropertyName("starting_primary")] public string StartingPrimary { get; set; } = ""; // weapon_nova

    public void Start(BasePlugin parent)
    {
        parent.AddCommand("ttt_reloadconfig", "", ReloadCommand);
    }

    public void ReloadCommand(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller != null) {
            info.ReplyToCommand("Command can only be used from Console");
            return; // console command only
        }
        
        info.ReplyToCommand("Reloaded Config");
        ReloadConfig();
    }
    
    public static void ReloadConfig()
    {
        string configPath = TttConfig.GetConfigPath();
        
        // initialize config if file doesn't exist yet
        if (!System.IO.File.Exists(configPath))
        {
            var dirPath = Path.GetDirectoryName(configPath);
            if (dirPath == null) return;
            Directory.CreateDirectory(dirPath);
            FileStream stream = System.IO.File.Create(configPath);
            stream.Close();
            TttConfig.Update();
        }
        TttConfig.Reload();
    }

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
        Console.WriteLine("OVERWRITTEN!"); 
    }
}
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Natives.Structs;
using TTT.Player;
using TTT.Public.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.InteropServices;

public static class CustomWeapon
{
    /*private static BasePlugin _plugin;
    private static IPlayerService _playerService;
    
    private static bool _customWeaponExists = false;
    private enum EntityType
    {
        None,
        Weapon,
        Projectile
    }

    public static void OnPluginStart(BasePlugin parent, IPlayerService manager)
    {
        _plugin = parent;
        _playerService = manager;
        parent.RegisterEventHandler<EventItemEquip>(OnItemEquip);
    }

    public static void OnMapStart() { }

    public static bool OnEquip(CCSPlayerController player, IShopItem item)
    {
        return Weapon.HandleEquip(player, item, true);
    }

    public static bool OnUnequip(CCSPlayerController player, IShopItem item, bool update)
    {
        return !update || Weapon.HandleEquip(player, item, false);
    }

    public static void OnEntityCreated(CEntityInstance entity)
    {
        if (!_customWeaponExists) return;

        if (!IsRelevantEntity(entity, out var entityType)) return;

        Server.NextWorldUpdate(() => ProcessEntity(entity, entityType));
    }

    private static bool IsRelevantEntity(CEntityInstance entity, out EntityType entityType)
    {
        entityType = EntityType.None;

        if (entity.DesignerName.StartsWith("weapon_"))
        {
            entityType = EntityType.Weapon;
            return true;
        }

        if (entity.DesignerName.EndsWith("_projectile"))
        {
            entityType = EntityType.Projectile;
            return true;
        }

        return false;
    }

    private static void ProcessEntity(CEntityInstance entity, EntityType entityType)
    {
        var player = GetPlayerFromEntity(entity, entityType);
        if (player == null) return;

        var shopItems = _playerService.GetPlayer(player).GetItems();

        var weaponDesignerName = GetWeaponDesignerName(entity, entityType);

        foreach (var item in shopItems)
        {
            TryApplyEquipmentModel(entity, item, weaponDesignerName, entityType, player);
        }
    }
    
    public static CCSPlayerController? FindTargetFromWeapon(CBasePlayerWeapon weapon)
    {
        SteamID steamId = new(weapon.OriginalOwnerXuidLow);

        CCSPlayerController? player = steamId.IsValid()
            ? Utilities.GetPlayers().FirstOrDefault(p => p.IsValid && p.SteamID == steamId.SteamId64) ?? Utilities.GetPlayerFromSteamId(weapon.OriginalOwnerXuidLow)
            : Utilities.GetPlayerFromIndex((int)weapon.OwnerEntity.EntityIndex) ?? Utilities.GetPlayerFromIndex((int)weapon.As<CCSWeaponBaseGun>().OwnerEntity.Value!.Index);

        return !string.IsNullOrEmpty(player?.PlayerName) ? player : null;
    }

    private static CCSPlayerController? GetPlayerFromEntity(CEntityInstance entity, EntityType entityType)
    {
        switch (entityType)
        {
            case EntityType.Weapon:
                var weapon = new CBasePlayerWeapon(entity.Handle);
                if (weapon?.IsValid == true && weapon.OriginalOwnerXuidLow > 0)
                {
                    return FindTargetFromWeapon(weapon);
                }
                break;

            case EntityType.Projectile:
                var projectile = entity.As<CBaseCSGrenadeProjectile>();
                return projectile.OriginalThrower.Value?.OriginalController.Value;
        }

        return null;
    }

    private static string GetWeaponDesignerName(CEntityInstance entity, EntityType entityType)
    {
        return entityType switch
        {
            EntityType.Weapon => Weapon.GetDesignerName(entity.As<CBasePlayerWeapon>()),
            EntityType.Projectile => "weapon_" + entity.DesignerName.Replace("_projectile", ""),
            _ => string.Empty
        };
    }

    private static void TryApplyEquipmentModel(CEntityInstance entity, IShopItem shopItem,
        string weaponDesignerName, EntityType entityType, CCSPlayerController player)
    {
        string? model = shopItem.Model();
        if (model == null) return;
        
        string worldModel = model;

        try
        {
            ApplyModelToEntity(entity, entityType, player, worldModel, worldModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to set model for {entity.DesignerName}: {ex.Message}");
        }
    }

    private static void ApplyModelToEntity(CEntityInstance entity, EntityType entityType,
        CCSPlayerController player, string viewModel, string worldModel)
    {
        switch (entityType)
        {
            case EntityType.Weapon:
                var weapon = entity.As<CBasePlayerWeapon>();
                if (weapon?.IsValid != true || weapon.OriginalOwnerXuidLow <= 0) return;

                var activeWeapon = player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;
                Weapon.UpdateModel(player, weapon, viewModel, worldModel, weapon == activeWeapon);
                break;

            case EntityType.Projectile:
                var projectile = entity.As<CBaseCSGrenadeProjectile>();
                if (projectile?.IsValid == true)
                {
                    projectile.SetModel(worldModel);
                }
                break;
        }
    }

    public static HookResult OnItemEquip(EventItemEquip @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        if (player == null) return HookResult.Continue;

        string? globalName = player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value?.Globalname;
        if (!string.IsNullOrEmpty(globalName))
        {
            string model = Weapon.GetFromGlobalName(globalName, Weapon.GlobalNameData.ViewModel);
            Weapon.SetViewModel(player, model);
        }

        return HookResult.Continue;
    }

    public static class Weapon
    {
        public enum GlobalNameData
        {
            ViewModelDefault,
            ViewModel,
            WorldModel
        }

        public static string GetDesignerName(CBasePlayerWeapon weapon)
        {
            string weaponDesignerName = weapon.DesignerName;
            ushort weaponIndex = weapon.AttributeManager.Item.ItemDefinitionIndex;

            return (weaponDesignerName, weaponIndex) switch
            {
                var (name, _) when name.Contains("bayonet") => "weapon_knife",
                ("weapon_m4a1", 60) => "weapon_m4a1_silencer",
                ("weapon_hkp2000", 61) => "weapon_usp_silencer",
                ("weapon_mp7", 23) => "weapon_mp5sd",
                _ => weaponDesignerName
            };
        }

        public static string GetFromGlobalName(string globalName, GlobalNameData data)
        {
            string[] globalNameSplit = globalName.Split(',');

            return data switch
            {
                GlobalNameData.ViewModelDefault => globalNameSplit[0],
                GlobalNameData.ViewModel => globalNameSplit[1],
                GlobalNameData.WorldModel => !string.IsNullOrEmpty(globalNameSplit[2]) ? globalNameSplit[2] : globalNameSplit[1],
                _ => throw new NotImplementedException()
            };
        }

        public static unsafe string GetViewModel(CCSPlayerController player)
        {
            return ViewModel(player)?.VMName ?? string.Empty;
        }

        public static unsafe void SetViewModel(CCSPlayerController player, string model)
        {
            ViewModel(player)?.SetModel(model);
        }

        public static void UpdateModel(CCSPlayerController player, CBasePlayerWeapon weapon, string model, string? worldModel, bool update)
        {
            weapon.Globalname = $"{GetViewModel(player)},{model},{worldModel}";
            weapon.SetModel(!string.IsNullOrEmpty(worldModel) ? worldModel : model);

            if (update)
            {
                SetViewModel(player, model);
            }
        }

        public static void ResetWeapon(CCSPlayerController player, CBasePlayerWeapon weapon, bool update)
        {
            string globalName = weapon.Globalname;
            if (string.IsNullOrEmpty(globalName)) return;

            string oldModel = GetFromGlobalName(globalName, GlobalNameData.ViewModelDefault);
            weapon.Globalname = string.Empty;
            weapon.SetModel(oldModel);

            if (update)
            {
                SetViewModel(player, oldModel);
            }
        }

        public static bool HandleEquip(CCSPlayerController player, IShopItem item, bool isEquip)
        {
            string? weaponName = item.WeaponName();
            if (player.PawnIsAlive && weaponName != null)
            {
                CBasePlayerWeapon? weapon = Get(player, weaponName);
                if (weapon != null)
                {
                    bool equip = weapon == player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;

                    if (isEquip)
                    {
                        string? model = item.Model();
                        if(model != null) UpdateModel(player, weapon, model, model, equip);
                    }
                    else
                    {
                        ResetWeapon(player, weapon, equip);
                    }
                }
            }

            return true;
        }

        private static CBasePlayerWeapon? Get(CCSPlayerController player, string weaponName)
        {
            CPlayer_WeaponServices? weaponServices = player.PlayerPawn.Value?.WeaponServices;
            if (weaponServices == null) return null;

            CBasePlayerWeapon? activeWeapon = weaponServices.ActiveWeapon.Value;
            return activeWeapon != null && GetDesignerName(activeWeapon) == weaponName
                ? activeWeapon
                : (weaponServices.MyWeapons.SingleOrDefault(p => p.Value != null && GetDesignerName(p.Value) == weaponName).Value);
        }

        private static unsafe CBaseViewModel? ViewModel(CCSPlayerController player)
        {
            nint? handle = player.PlayerPawn.Value?.ViewModelServices?.Handle;
            if (handle == null || !handle.HasValue) return null;
            
            CCSPlayer_ViewModelServices viewModelServices = new(handle.Value);
            nint ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
            Span<nint> viewModels = MemoryMarshal.CreateSpan(ref ptr, 3);

            return new CHandle<CBaseViewModel>(viewModels[0]).Value;
        }
    }

    public static void Inspect(CCSPlayerController player, string model, string weapon)
    {
        if (player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value is not CBasePlayerWeapon activeWeapon) return;

        if (Weapon.GetDesignerName(activeWeapon) != weapon)
        {
            player.PrintToChat("You need correct weapon" + weapon);
            return;
        }

        string globalName = activeWeapon.Globalname;
        string oldModel = !string.IsNullOrEmpty(globalName) ? Weapon.GetFromGlobalName(globalName, Weapon.GlobalNameData.ViewModel) : Weapon.GetViewModel(player);

        Weapon.SetViewModel(player, model);

        _plugin.AddTimer(3.0f, () =>
        {
            if (player.IsValid && player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value == activeWeapon)
            {
                Weapon.SetViewModel(player, oldModel);
            }
        });
    }*/
}
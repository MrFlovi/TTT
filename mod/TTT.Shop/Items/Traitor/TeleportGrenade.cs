using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class TeleportGrenade : IShopItem
{
    public string Name()
    {
        return "Teleport Grenade";
    }

    public string SimpleName()
    {
        return "teleportgrenade";
    }
    
    public string? WeaponName()
    {
        return "weapon_decoy";
    }
    
    public string? Model()
    {
        return "models\\weapon\\gasgrenade.vmdl";
    }

    public int Price()
    {
        return 1;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        player.Player()?.GiveNamedItem(CsItem.Decoy);
        return BuyResult.Successful;
    }
}
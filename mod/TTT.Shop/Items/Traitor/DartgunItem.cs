using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Traitor;

public class DartgunItem : IShopItem
{
    public string Name()
    {
        return "Dart Gun";
    }

    public string SimpleName()
    {
        return "dartgun";
    }
    
    public string? WeaponName()
    {
        return "weapon_ssg08";
    }
    
    public string? Model()
    {
        return "models\\weapon\\dartgun.vmdl";
    }

    public int Price()
    {
        return 2;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        player.Player()?.GiveNamedItem(CsItem.SSG08);
        return BuyResult.Successful;
    }
}
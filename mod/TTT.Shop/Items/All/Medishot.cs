using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Detective;

public class Medishot : IShopItem
{
    public string Name()
    {
        return "Medishot";
    }

    public string SimpleName()
    {
        return "medishot";
    }

    public string? WeaponName()
    {
        return "weapon_healthshot";
    }
    
    public string? Model()
    {
        return null;
    }
    
    public int Price()
    {
        return 1;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        player.RemoveCredits(Price());
        var playerObject = player.Player();
        playerObject?.GiveNamedItem(CsItem.Healthshot);
        return BuyResult.Successful;
    }
}
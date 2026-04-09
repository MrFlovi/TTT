using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Detective;

public class DNAScannerItem : IShopItem
{
    public string Name()
    {
        return "DNA Scanner";
    }

    public string SimpleName()
    {
        return "dnascanner";
    }
    
    public string? WeaponName()
    {
        return null;
    }
    
    public string? Model()
    {
        return null;
    }

    public int Price()
    {
        return 2;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        if (player.PlayerRole() != Role.Detective) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        return BuyResult.Successful;
    }
}
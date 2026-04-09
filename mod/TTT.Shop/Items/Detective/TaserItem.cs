using System;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using TTT.Player;
using TTT.Public.Mod.Role;
using TTT.Public.Shop;

namespace TTT.Shop.Items.Detective;

public class TaserItem : IShopItem
{
    public string Name()
    {
       return "Taser";
    }

    public string SimpleName()
    {
        return "taser";
    }
    
    public string? WeaponName()
    {
        return "weapon_taser";
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
        var playerObject = player.Player();
        playerObject?.GiveNamedItem(CsItem.Zeus);
        return BuyResult.Successful;
    }
}
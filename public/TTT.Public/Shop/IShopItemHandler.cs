using CounterStrikeSharp.API.Core;

namespace TTT.Public.Shop;

public interface IShopItemHandler
{
    ISet<IShopItem> GetShopItems();
    void AddShopItem(IShopItem item);

    void OnItemBuy(CCSPlayerController player, IShopItem item);
}
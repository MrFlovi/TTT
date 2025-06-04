using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Menu;
using TTT.Player;
using TTT.Public.Formatting;
using TTT.Public.Player;

namespace TTT.Public.Shop;

public class ShopMenu
{
    private readonly CenterHtmlMenu _menu;
    private readonly GamePlayer _playerService;
    private readonly IShopItemHandler _shopItemHandler;

    public ShopMenu(BasePlugin plugin, IShopItemHandler shopItemHandler, GamePlayer playerService)
    {
        _menu = new CenterHtmlMenu($"Shop - {playerService.Credits()} credits", plugin);
        _shopItemHandler = shopItemHandler;
        _playerService = playerService;
        Create();
    }

    public void BuyItem(GamePlayer player, IShopItem item)
    {
        BuyResult successful = item.OnBuy(player);
        CCSPlayerController? controller = player.Player();
        if (controller == null) return;
        
        switch (successful)
        {
            case BuyResult.NotEnoughCredits:
                controller
                    .PrintToChat(StringUtils.FormatTTT($"You don't have enough credits to buy {item.Name()}"));
                break;
            case BuyResult.Successful:
                controller.PrintToChat(StringUtils.FormatTTT($"You have bought {item.Name()}"));
                player.AddItem(item);
                break;
            case BuyResult.AlreadyOwned:
                controller.PrintToChat(StringUtils.FormatTTT($"You already own {item.Name()}"));
                break;
            case BuyResult.IncorrectRole:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void BuyItem(GamePlayer player, int index)
    {
        IShopItem item = _shopItemHandler.GetShopItems().ElementAt(index);
        BuyItem(player, item);
    }

    public void BuyItem(GamePlayer player, string name)
    {
        foreach (IShopItem item in _shopItemHandler.GetShopItems())
        {
            if (!item.SimpleName().Equals(name)) continue;
            BuyItem(player, item);
            return;
        }

        CCSPlayerController? controller = player.Player();
        if (controller == null) return;
        controller.PrintToChat(StringUtils.FormatTTT("Item not found!"));
    }

    public void Create()
    {
        foreach (ChatMenuOption option in _menu.MenuOptions.Where(option => option.Text.Equals("close")))
        {
            option.OnSelect += (player, _) =>
            {
                _playerService.SetShopOpen(false);
            };
        }

        for (int index = 0; index < _shopItemHandler.GetShopItems().Count; index++)
        {
            IShopItem item = _shopItemHandler.GetShopItems().ElementAt(index);
            _menu.AddMenuOption(item.Name() + $" - {item.Price()} credits",
                (player, _) =>
                {
                    BuyItem(_playerService, item);
                    _playerService.SetShopOpen(false);
                });
        }
    }
    
    public void Open(CCSPlayerController player)
    {
        _menu.Open(player);
        
    }
}
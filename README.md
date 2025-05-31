# cs2TTT
A gmod inspired Trouble in Terrorist Town plugin for CS2

## Noteworthy features
* The detective gets a taser (Zeus), which upon use on a player, reveals their role
* Traitors can use team chat to privately communicate with each other


[//]: # (## Items & Shop)

[//]: # (You can open the shop with the `css_shop` command.)

[//]: # ()
[//]: # (You can buy items during the game by using the `css_buy <itemName>` command, where you replace `<itemName>` with the _itemName_ of the item of your choice.)

[//]: # ()
[//]: # (Below is a list of currently added items, an explanation of **what they do** &#40;still to be added&#41;, which **team** can buy them, their **price** and their **itemName** )

[//]: # ()
[//]: # (| Item              | Use     | Team      | Price | itemName   |)

[//]: # (| :---------------- | :------ | :-------: | :---: | :--------: |)

[//]: # (| AK47              |    -    | Traitor   | 500   | ak47       |)

[//]: # (| AWP               |    -    | Traitor   | 2000  | awp        |)

[//]: # (| Wall Hack         |    -    | Traitor   | 1000  | wallhack   |)

[//]: # (| Taser             | Reveal someone's role | Detective | 1000  | taser      |)

[//]: # (| DNA Scanner       |    -    | Detective | 1000  | dnascanner |)

[//]: # ()
[//]: # (## Commands)

[//]: # ()
[//]: # (### In-Game)

[//]: # (* `css_shop` Opens the shop menu )

[//]: # (* `css_buy <itemName>` Buys the specified item if you have enough money &#40;e.g. `css_buy ak47` or `css_buy wallhack`&#41;)

[//]: # ()
[//]: # (### Console)

[//]: # (* `css_roles` = Get the roles of all players)

## Contributing
TTT is in heavy development and I want you to know that contributions are always welcome. Please follow Microsoft's dependency injection system.

> [!TIP]
> Microsoft has some good documentation on dependency injection here: 
> [Overview](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection),
> [Using Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage),
> [Dependency Injection Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines).

## Creating items

Creating new items or modifying existing ones is easy. Create a new class in the correct directory, mod/TTT.Shop/Items/{group}. Then create it to your liking. Afterwards, compile the plugin and it's all set. The plugin handles loading all the items.

> [!TIP]
> Available groups are [All, Detective, Traitor]. <br>
> SimpleName is used for /buy {name}

#### Example Item
```c#
namespace TTT.Shop.Items.Traitor;

public class AwpItem : IShopItem
{
    public string Name()
    {
        return "AWP";
    }

    public string SimpleName()
    {
        return "awp";
    }

    public int Price()
    {
        return 2000;
    }

    public BuyResult OnBuy(GamePlayer player)
    {
        if (player.Credits() < Price()) return BuyResult.NotEnoughCredits;
        if (player.PlayerRole() != Role.Traitor) return BuyResult.IncorrectRole;
        player.RemoveCredits(Price());
        player.Player().GiveNamedItem(CsItem.AWP);
        return BuyResult.Successful;
    }
}
```

## Road Map
- [x] Role assignment
- [x] DNA Scanner
- [x] Tazer
- [x] Configuration
- [ ] Karma system
- [ ] Shop
- [ ] RDM Manager
- [ ] Add database support for logs and stats

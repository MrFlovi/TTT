﻿using CounterStrikeSharp.API.Core;
using TTT.Public.Mod.Role;
using TTT.Public.Player;
using TTT.Round;

namespace TTT.Player;

public class PlayerHandler : IPlayerService
{
    private readonly Dictionary<CCSPlayerController, GamePlayer> _players = new ();
    
    public Dictionary<CCSPlayerController, GamePlayer> GetPlayers()
    {
        return _players;
    }
    
    public void CreatePlayer(CCSPlayerController player)
    {
        if (_players.ContainsKey(player) || player.UserId == null) return;
        _players.Add(player, new GamePlayer(Role.Unassigned, 1000, 80, player.UserId.Value));
    }

    public List<GamePlayer> Players()
    {
        return _players.Values.ToList();
    }

    public GamePlayer GetPlayer(CCSPlayerController player)
    {
        if (!_players.ContainsKey(player)) CreatePlayer(player);
        return _players[player];
    }

    public void RemovePlayer(CCSPlayerController player)
    {
        _players.Remove(player);
    }

    public void Clr()
    {
        foreach (GamePlayer player in Players())
        {
            player.SetKiller(null);
            player.SetPlayerRole(Role.Unassigned);
            player.SetRagdollProp(null);
            player.ResetCredits();
            player.ResetItems();
            player.ModifyKarma();
            player.SetFound(false);
            player.SetDead();
        }
    }
}
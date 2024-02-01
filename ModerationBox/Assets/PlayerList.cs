
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;



public class PlayerRole : UdonSharpBehaviour
{
    public enum Role
    {
        Owner,
        Admin,
        Moderator,
        Visitor,
        Blocked
    }
    public static DataDictionary Colors = new DataDictionary() {
        {
            (int)Role.Owner, "#FFFF00"
        },
        {
            (int)Role.Admin, "#FFA500"
        },
        {
            (int)Role.Moderator, "#FFFF00"
        },
        {
            (int)Role.Visitor, "#00FF00"
        },
        {
            (int)Role.Blocked, "#FF0000"
        }
    };
}

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerList : UdonSharpBehaviour
{
    [HideInInspector]
    public DataDictionary playerRoles;

    void handlePlayerJoined(VRCPlayerApi player)
    {
        if (player == null)
        {
            return;
        }
        Debug.Log("Player joined: " + player.displayName);
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject) || playerRoles.ContainsKey(player.playerId))
        {
            return;
        }
        PlayerRole.Role role = PlayerRole.Role.Blocked;
        if (player.isInstanceOwner)
        {
            role = PlayerRole.Role.Owner;
        }
        else if (player.isMaster)
        {
            role = PlayerRole.Role.Admin;
        }
        playerRoles.Add(player.playerId, new DataToken((int)role));
    }
    void Start()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            return;
        }
        VRCPlayerApi[] players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[80]);
        playerRoles = new DataDictionary();
        foreach (VRCPlayerApi player in players)
        {
            handlePlayerJoined(player);
        }
        RequestSerialization();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            return;
        }
        handlePlayerJoined(player);
        RequestSerialization();
    }

    public override bool OnOwnershipRequest(VRCPlayerApi requester, VRCPlayerApi newOwner)
    {
        if (HasPermission(requester.playerId, PlayerRole.Role.Moderator))
        {
            Networking.SetOwner(newOwner, gameObject);
            return true;
        }
        return false;
    }

    public string GetPlayerColor(int playerId)
    {
        return PlayerRole.Colors.TryGetValue((int)GetPlayerRole(playerId), out DataToken color) ? color.String : "#000000";
    }

    public int CompareRolesFromId(int a, int b)
    {
        return GetPlayerRole(a).CompareTo(GetPlayerRole(b));
    }
    public int CompareRoles(VRCPlayerApi a, VRCPlayerApi b)
    {
        return CompareRolesFromId(a.playerId, b.playerId);
    }

    public PlayerRole.Role GetPlayerRole(int playerId)
    {
        if (!playerRoles.ContainsKey(playerId))
        {
            playerRoles.Add(playerId, new DataToken((int)PlayerRole.Role.Blocked));
        }
        return playerRoles.TryGetValue(playerId, out DataToken role) ? (PlayerRole.Role)role.Int : PlayerRole.Role.Blocked;
    }

    public bool HasPermission(int playerId, PlayerRole.Role role)
    {
        return GetPlayerRole(playerId) >= role;
    }

    public void _SetPlayerRole(int playerId, PlayerRole.Role role)
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            return;
        }
        playerRoles.SetValue(playerId, new DataToken((int)role));
        RequestSerialization();
    }

    public void SetPlayerRole(int playerId, PlayerRole.Role role)
    {
        if (!HasPermission(Networking.LocalPlayer.playerId, PlayerRole.Role.Moderator))
        {
            return;
        }
        VRCPlayerApi origOwner = Networking.GetOwner(gameObject);
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            Debug.Log("Setting owner to local player");
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        Debug.Log("Setting role of " + playerId + " to " + role);
        _SetPlayerRole(playerId, role);
        if (origOwner != Networking.LocalPlayer)
        {
            Debug.Log("Setting owner back to original owner");
            Networking.SetOwner(origOwner, gameObject);
        }
    }
}

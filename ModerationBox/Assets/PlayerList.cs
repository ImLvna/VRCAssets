
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;




    public enum Role
    {
        Owner,
        Admin,
        Moderator,
        Visitor,
        Blocked
    }


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PlayerList : UdonSharpBehaviour
{
    [SerializeField, UdonSynced]
    private string _json;
    private DataDictionary playerRoles;

    public override void OnPreSerialization()
    {
        if (VRCJson.TrySerializeToJson(playerRoles, JsonExportType.Minify, out DataToken result))
        {
            _json = result.String;
        }
        else
        {
            Debug.LogError(result.ToString());
        }
    }

    public override void OnDeserialization()
    {
        if (VRCJson.TryDeserializeFromJson(_json, out DataToken result))
        {
            playerRoles = result.DataDictionary;
        }
        else
        {
            Debug.LogError(result.ToString());
        }
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
        Role role = Role.Blocked;
        if (player.isInstanceOwner)
        {
            role = Role.Owner;
        }
        else if (player.isMaster)
        {
            role = Role.Admin;
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
        if (HasPermission(requester.playerId, Role.Moderator))
        {
            Networking.SetOwner(newOwner, gameObject);
            return true;
        }
        return false;
    }

    public string GetPlayerColor(int playerId)
    {
        return Colors.TryGetValue((int)GetPlayerRole(playerId), out DataToken color) ? color.String : "#000000";
    }

    public int CompareRolesFromId(int a, int b)
    {
        return GetPlayerRole(a).CompareTo(GetPlayerRole(b));
    }
    public int CompareRoles(VRCPlayerApi a, VRCPlayerApi b)
    {
        return CompareRolesFromId(a.playerId, b.playerId);
    }

    public Role GetPlayerRole(int playerId)
    {
        if (!playerRoles.ContainsKey(playerId))
        {
            playerRoles.Add(playerId, new DataToken((int)Role.Blocked));
        }
        return playerRoles.TryGetValue(playerId, out DataToken role) ? (Role)role.Int : Role.Blocked;
    }

    public bool HasPermission(int playerId, Role role)
    {
        return GetPlayerRole(playerId) >= role;
    }

    public void _SetPlayerRole(int playerId, Role role)
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            return;
        }
        playerRoles.SetValue(playerId, new DataToken((int)role));
        RequestSerialization();
    }

    public void SetPlayerRole(int playerId, Role role)
    {
        if (!HasPermission(Networking.LocalPlayer.playerId, Role.Moderator))
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

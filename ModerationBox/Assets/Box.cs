
using System;
using System.Linq;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;


[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Box : UdonSharpBehaviour
{



    public PlayerList playerList;

    public GameObject wall;

    private GameObject menu;
    private GameObject playerListContent;
    private GameObject playerListTemplate;

    private GameObject actionsHeader;

    private int currentPlayer = 0;
    void Start()
    {

        menu = transform.Find("Menu").gameObject;

        playerListContent = menu.transform.Find("PlayerList").Find("Viewport").Find("Content").gameObject;

        playerListTemplate = menu.transform.Find("PlayerList").Find("Template").gameObject;

        actionsHeader = menu.transform.Find("Actions").Find("Header").gameObject;

        updateWall();

        refreshPlayerList();
    }

    public void updateWall()
    {
        if (playerList.HasPermission(Networking.LocalPlayer.playerId, PlayerRole.Role.Visitor))
        {
            wall.SetActive(false);
        }
        else
        {
            wall.SetActive(true);
        }
        if (!playerList.HasPermission(Networking.LocalPlayer.playerId, PlayerRole.Role.Moderator))
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
        }
    }

    public void setCurrentPlayer(int playerId)
    {
        VRCPlayerApi player = VRCPlayerApi.GetPlayerById(playerId);
        if (player == null)
        {
            return;
        }
        currentPlayer = playerId;

        actionsHeader.GetComponentInChildren<TextMeshProUGUI>().text = String.Format(player.displayName);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        PlayerRoleUpdated();
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        PlayerRoleUpdated();
    }

    public void PlayerRoleUpdated()
    {
        refreshPlayerList();
        updateWall();
    }

    void onSetPlayerRole()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "PlayerRoleUpdated");
    }

    public void setPlayerVisitor()
    {
        Debug.Log(Networking.LocalPlayer.playerId);
        playerList.SetPlayerRole(currentPlayer, PlayerRole.Role.Visitor);
        onSetPlayerRole();
    }
    public void setPlayerModerator()
    {
        playerList.SetPlayerRole(currentPlayer, PlayerRole.Role.Moderator);
        onSetPlayerRole();
    }
    public void setPlayerAdmin()
    {
        playerList.SetPlayerRole(currentPlayer, PlayerRole.Role.Admin);
        onSetPlayerRole();
    }
    public void setPlayerBlocked()
    {
        playerList.SetPlayerRole(currentPlayer, PlayerRole.Role.Blocked);
        onSetPlayerRole();
    }

    void refreshPlayerList()
    {
        Debug.Log("Refreshing player list");

        Transform[] children = playerListContent.GetComponentsInChildren<Transform>();
        for (int i = 1; i < children.Length; i++)
        {
            Destroy(children[i].gameObject);
        }

        VRCPlayerApi[] players = VRCPlayerApi.GetPlayers(new VRCPlayerApi[80]);

        float step = playerListTemplate.GetComponent<RectTransform>().rect.height;
        float height = 0;

        foreach (VRCPlayerApi player in players)
        {
            if (player == null)
            {
                continue;
            }
            GameObject playerListEntry = Instantiate(playerListTemplate);

            playerListEntry.name = player.displayName;

            BoxPlayerListEntry boxPlayerListEntry = playerListEntry.GetComponent<BoxPlayerListEntry>();
            boxPlayerListEntry.playerId = player.playerId;


            playerListEntry.transform.SetParent(playerListContent.transform, false);
            playerListEntry.SetActive(true);
            playerListEntry.GetComponentInChildren<TextMeshProUGUI>().text = ColoredText.Colored(player.displayName, playerList.GetPlayerColor(player.playerId));
            playerListEntry.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, height);
            height -= step;
        }
    }
}

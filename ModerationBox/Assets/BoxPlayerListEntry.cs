
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class BoxPlayerListEntry : UdonSharpBehaviour
{
    public int playerId;
    public Box modbox;

    public void OnClick()
    {
        modbox.setCurrentPlayer(playerId);
    }
}

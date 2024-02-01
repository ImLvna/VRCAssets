
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Colored : UdonSharpBehaviour
{
    public static string C(string text, string color)
    {
        return "<color=" + color + ">" + text + "</color>";
    }
}
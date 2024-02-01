
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ColoredText : UdonSharpBehaviour
{
    public static string Colored(string text, string color)
    {
        return "<color=" + color + ">" + text + "</color>";
    }
}
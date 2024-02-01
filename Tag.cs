
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

public class Tag : UdonSharpBehaviour
{
    public string[] tags;

    public bool Has(string tag)
    {
        foreach (string t in tags)
        {
            if (t == tag)
            {
                return true;
            }
        }
        return false;
    }

    public void Add(string tag)
    {
        if (Has(tag))
        {
            return;
        }
        string[] newTags = new string[tags.Length + 1];
        for (int i = 0; i < tags.Length; i++)
        {
            newTags[i] = tags[i];
        }
        newTags[tags.Length] = tag;
        tags = newTags;
    }

    public void Remove(string tag)
    {
        if (!Has(tag))
        {
            return;
        }
        string[] newTags = new string[tags.Length - 1];
        int j = 0;
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] != tag)
            {
                newTags[j] = tags[i];
                j++;
            }
        }
        tags = newTags;
    }
}

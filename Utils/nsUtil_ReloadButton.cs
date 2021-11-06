
using UdonSharp;
using UnityEngine;

public class nsUtil_ReloadButton : UdonSharpBehaviour
{
    // [HideInInspector]
    public Video2Str video2Str;
    public void onClick(){
        video2Str.reload();
    }
}

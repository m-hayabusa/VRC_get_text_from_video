
using UdonSharp;
using UnityEngine;
using UnityEngine.Video;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.Base;

public class VideoPlayerController : UdonSharpBehaviour
{
    public TexToStr receiver;
    public VRCUnityVideoPlayer unityVideoPlayer;
    private int frame = 0;

    public void setFrame(int f)
    { // 1 fps想定
        frame = f;

        // unityVideoPlayer.Pause();
        // unityVideoPlayer.SetTime(frame);

        this.OnVideoReady();
    }

    public int getFrame()
    { // 1 fps想定
        return frame;
    }

    public override void OnVideoReady()
    {
        unityVideoPlayer.Pause();
        unityVideoPlayer.SetTime(frame);

        receiver.OnVideoReady();
    }
}

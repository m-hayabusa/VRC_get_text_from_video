
using UdonSharp;
using UnityEngine;
using UnityEngine.Video;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.Base;

public class VideoPlayerController : UdonSharpBehaviour
{
    public Video2Str receiver;
    public VRCUnityVideoPlayer unityVideoPlayer;
    private int frame = 0;
    public VRCUrl url;

    public void setFrame(int f)
    { // 1 fps想定
        frame = f;
        this.OnVideoReady();
    }

    public int getFrame()
    { // 1 fps想定
        return frame;
    }

    public void reload()
    {
        unityVideoPlayer.Stop();
        unityVideoPlayer.PlayURL(url);
    }

    public override void OnVideoReady()
    {
        unityVideoPlayer.Pause();
        unityVideoPlayer.SetTime(frame);

        receiver.OnVideoReady();
    }
}


using UdonSharp;
using VRC.SDK3.Video.Components;

namespace nekomimiStudio.video2String
{
    public class VideoPlayerController : UdonSharpBehaviour
    {
        private Video2Str video2Str;
        private VRCUnityVideoPlayer unityVideoPlayer;
        private v2sConfig config;
        private int frame = 0;
        private bool inited = false;

        private void init()
        {
            video2Str = this.GetComponent<Video2Str>();
            config = this.GetComponent<v2sConfig>();
            unityVideoPlayer = (VRCUnityVideoPlayer)(this.GetComponent(typeof(VRCUnityVideoPlayer)));
        }

        public bool isReady(){
            return unityVideoPlayer.IsReady && unityVideoPlayer.GetTime() == frame;
        }

        public void setFrame(int f)
        { // 1 fps想定
            frame = f;
            this.OnVideoReady();
        }

        public int getFrame()
        { // 1 fps想定
            return frame;
        }

        public int getLength()
        {
            return UnityEngine.Mathf.FloorToInt(unityVideoPlayer.GetDuration());
        }

        public void reload()
        {
            if (!inited) init();
            unityVideoPlayer.Stop();
            unityVideoPlayer.PlayURL(config.url);
        }

        public override void OnVideoReady()
        {
            unityVideoPlayer.Pause();
            unityVideoPlayer.SetTime(frame);

            video2Str.OnVideoReady();
        }
    }
}
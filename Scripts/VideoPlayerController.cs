
using UdonSharp;
using VRC.SDKBase;
using VRC.SDK3.Video.Components;

namespace nekomimiStudio.video2String
{
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
}

using UdonSharp;

namespace nekomimiStudio.video2String
{
    public class ReloadButton : UdonSharpBehaviour
    {
        public Video2Str video2Str;
        public void onClick()
        {
            video2Str.reload();
        }
    }
}

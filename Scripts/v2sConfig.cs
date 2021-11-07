
using UdonSharp;
using VRC.SDKBase;

namespace nekomimiStudio.video2String
{
    public class v2sConfig : UdonSharpBehaviour
    {
        public VRCUrl url;
        public bool isAutoStart = false;
        public int decodeWait = 2;
    }
}
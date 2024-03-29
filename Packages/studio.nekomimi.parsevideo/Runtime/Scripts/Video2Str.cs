﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;

namespace nekomimiStudio.video2String
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Video2Str : UdonSharpBehaviour
    {
        private v2sConfig config;
        public Texture2D tmpTex;
        public Texture2D outputTex;
        private Parser parser;
        private VideoPlayerController video;

        private bool triggerCapture = false;
        private bool isTmpTexReady = false, isParsed = false, isVideoReady = false, isRemoteLoading = false;

        public override void OnVideoReady()
        {
            isVideoReady = true;
        }

        [SerializeField] private Shader LinearToSRGB;
        [SerializeField] private Shader unlit;

        public void OnPostRender()
        {
            if (triggerCapture && isVideoReady)
            {
                this.fromCamera();
                triggerCapture = false;
                isTmpTexReady = true;
                video.setFrame(decodeFrame++); //ここでシーク失敗してたときに同じフレーム読む気がする
            };
            if (triggeredGetTexture > 0 && video.isReady())
                triggeredGetTexture--;
            if (triggeredGetTexture == 0)
            {
                this.GetComponent<Camera>().targetTexture = defaultTex;
                this.transform.GetChild(0).GetComponent<MeshRenderer>().material.shader = LinearToSRGB;
            }
        }

        private int decodeIttr = 0;
        private bool isDecoding = false;
        private int decodeFrame = 0;

        [UdonSynced]
        private string decodeResult = "";
        private int decodeWaitCnt = 0;

        private bool triggerDeSerialize;

        [UdonSynced]
        public string lastReload;

        public void RemoteReloaded()
        {
            isRemoteLoading = false;
            triggerDeSerialize = true;
        }
        public void RemoteReloading()
        {
            isRemoteLoading = true;
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            if (Networking.GetOwner(this.gameObject) == player) isRemoteLoading = false;
        }

        public override void OnDeserialization()
        {
            if (triggerDeSerialize)
            {
                parser.reset();
                parser.parse(decodeResult);
                isParsed = true;
            }
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (config.isGlobal && parser.isDone() && Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
            {
                RequestSerialization();
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RemoteReloaded");
            }
        }

        public void Update()
        {
            _GetTexture();
            if (isDecoding && isTmpTexReady)
            {
                decodeWaitCnt++;
                if (decodeWaitCnt > config.decodeWait)
                {
                    if (decodeString(decodeIttr))
                    {
                        Debug.Log("end decode");
                        isDecoding = false;
                        if (decodeResult != "")
                        {
                            parser.parse(decodeResult);

                            if (config.isGlobal)
                            {
                                RequestSerialization();
                                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RemoteReloaded");
                            }

                            isParsed = true;
                        }
                        else
                            Debug.LogWarning("デコード失敗");
                    }
                    else
                    {
                        decodeIttr++;
                        if (decodeIttr >= 256)
                        {
                            capture();
                            decodeIttr = 0;
                        }
                    }
                    decodeWaitCnt = 0;
                }
            }

            if (isTmpTexReady && !isParsed && !isDecoding)
            {
                Debug.Log("start decode");
                isDecoding = true;
            }
        }

        private const int GetTexture_BUF = 16;
        private int triggeredGetTexture = 0;
        private RenderTexture[] GetTexture_Target = new RenderTexture[GetTexture_BUF];
        private string[] GetTexture_file = new string[GetTexture_BUF];
        private int[] GetTexture_id = new int[GetTexture_BUF];
        private int GetTexture_head = 0;
        private int GetTexture_tail = 0;

        public bool GetTexture(RenderTexture target, string file, int id)
        {
            Debug.Log($"en {id}, {GetTexture_head}, {GetTexture_tail}");
            if (GetTexture_head == (GetTexture_tail + 1) % GetTexture_BUF)
                return false;

            GetTexture_id[GetTexture_tail] = id;
            GetTexture_file[GetTexture_tail] = file;
            GetTexture_Target[GetTexture_tail] = target;

            GetTexture_tail++;
            if (GetTexture_tail == GetTexture_BUF) GetTexture_tail = 0;
            return true;
        }

        private bool _GetTexture()
        {
            if (GetTexture_head == GetTexture_tail || !parser.isDone() || isLoading() || triggeredGetTexture > 0) { return false; }

            RenderTexture target = GetTexture_Target[GetTexture_head];
            string file = GetTexture_file[GetTexture_head];
            int id = GetTexture_id[GetTexture_head];
            Debug.Log($"de {id}, {GetTexture_head}, {GetTexture_tail}");

            video.setFrame(video.getLength() + int.Parse(parser.getString(file, id, "frame")) - 1);

            this.transform.GetChild(0).GetComponent<MeshRenderer>().material.shader = unlit;
            this.GetComponent<Camera>().targetTexture = target;
            triggeredGetTexture = 2;

            GetTexture_head++;
            if (GetTexture_head == GetTexture_BUF) GetTexture_head = 0;

            return true;
        }

        public float getDecodeProgress()
        {
            if (decodeIttr > 0 && !isDecoding) return 1.0F;
            return decodeIttr / 256F;
        }

        public bool isLoading()
        {
            return (triggerCapture || isVideoReady || isRemoteLoading) && !isParsed;
        }

        public Parser getParser()
        {
            if (parser == null) parser = this.GetComponent<Parser>();
            return parser;
        }
        public v2sConfig getConfig()
        {
            if (config == null) config = this.GetComponent<v2sConfig>();
            return config;
        }

        private RenderTexture defaultTex;
        void Start()
        {
            getParser();
            getConfig();
            video = this.GetComponent<VideoPlayerController>();
            defaultTex = this.GetComponent<Camera>().targetTexture;

            if (config.isAutoStart && (config.isGlobal ? Networking.IsOwner(Networking.LocalPlayer, this.gameObject) : true))
                reload();
        }

        private void capture()
        {
            isTmpTexReady = false;
            triggerCapture = true;
        }

        public void reload()
        {
#if UNITY_ANDROID
            // Oculus Questでは読み込めない: https://github.com/m-hayabusa/VRC_get_text_from_video/issues/1
#else
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "RemoteReloading");

            lastReload = DateTime.Now.ToLocalTime().ToString("yyyy/MM/dd HH:mm");

            Debug.Log("Reloading..");
            capture();
            isVideoReady = false;
            isParsed = false;
            parser.reset();
            decodeResult = "";
            decodeIttr = 0;
            decodeFrame = 1;

            video.reload();
#endif
        }

        public Texture2D encodeString(string input)
        {
            int tmp = 0;

            for (int i = 0; i < input.Length; i++)
            {
                Color col = new Color(0, 0, 0, 1);

                for (int j = 0; j < 2; j++)
                {
                    if (j % 2 == 1)
                        tmp = (char)input[i];
                    else
                        tmp = (char)input[i] >> 8;

                    if (((tmp >> 0) & 0b111) == 0b000)
                        col.r = 0.125F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b001)
                        col.r = 0.250F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b010)
                        col.r = 0.375F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b011)
                        col.r = 0.500F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b100)
                        col.r = 0.625F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b101)
                        col.r = 0.750F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b110)
                        col.r = 0.875F - 0.0625F;
                    else if (((tmp >> 0) & 0b111) == 0b111)
                        col.r = 1.000F;

                    if (((tmp >> 3) & 0b111) == 0b000)
                        col.g = 0.125F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b001)
                        col.g = 0.250F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b010)
                        col.g = 0.375F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b011)
                        col.g = 0.500F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b100)
                        col.g = 0.625F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b101)
                        col.g = 0.750F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b110)
                        col.g = 0.875F - 0.0625F;
                    else if (((tmp >> 3) & 0b111) == 0b111)
                        col.g = 1.000F;

                    if (((tmp >> 6) & 0b11) == 0b00)
                        col.b = 0.125F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b01)
                        col.b = 0.250F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b10)
                        col.b = 0.375F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b11)
                        col.b = 0.500F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b00)
                        col.b = 0.625F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b01)
                        col.b = 0.750F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b10)
                        col.b = 0.875F - 0.0625F;
                    else if (((tmp >> 6) & 0b11) == 0b11)
                        col.b = 1.000F;

                    outputTex.SetPixel(0, i * 2 + j, col);
                }
            }
            outputTex.Apply(false);
            return outputTex;
        }

        public bool decodeString(int x)
        {
            int res = 0;

            for (int y = 0; y < 256; y += 2)
            {
                res = 0;
                for (int i = y; i <= y + 1; i++)
                {
                    Color col = tmpTex.GetPixel(x, i);

                    int tmp = 0;

                    if (col.r < 0.125)
                        tmp += 0b00000000;
                    else if (col.r < 0.250)
                        tmp += 0b00000001;
                    else if (col.r < 0.375)
                        tmp += 0b00000010;
                    else if (col.r < 0.500)
                        tmp += 0b00000011;
                    else if (col.r < 0.625)
                        tmp += 0b00000100;
                    else if (col.r < 0.750)
                        tmp += 0b00000101;
                    else if (col.r < 0.875)
                        tmp += 0b00000110;
                    else
                        tmp += 0b00000111;

                    if (col.g < 0.125)
                        tmp += 0b00000000;
                    else if (col.g < 0.250)
                        tmp += 0b00001000;
                    else if (col.g < 0.375)
                        tmp += 0b00010000;
                    else if (col.g < 0.500)
                        tmp += 0b00011000;
                    else if (col.g < 0.625)
                        tmp += 0b00100000;
                    else if (col.g < 0.750)
                        tmp += 0b00101000;
                    else if (col.g < 0.875)
                        tmp += 0b00110000;
                    else
                        tmp += 0b00111000;

                    if (col.b < 0.125)
                        tmp += 0b00000000;
                    else if (col.b < 0.250)
                        tmp += 0b01000000;
                    else if (col.b < 0.375)
                        tmp += 0b10000000;
                    else if (col.b < 0.500)
                        tmp += 0b11000000;
                    else if (col.b < 0.625)
                        tmp += 0b00000000;
                    else if (col.b < 0.750)
                        tmp += 0b01000000;
                    else if (col.b < 0.875)
                        tmp += 0b10000000;
                    else
                        tmp += 0b11000000;

                    if (i % 2 == 0)
                        res += tmp << 8;
                    else
                        res += tmp << 0;
                }

                if (res == 0xFFFF || res == 0x0000) //U+FFFFは「存在しない」ことが保証されている 白埋めがFFFFになるので外に飛ぶ 黒(NUL→おしり)がある時もそうする
                    return true;
                decodeResult += (char)res;
            }

            return false;
        }

        private Texture2D fromCamera()
        {
            tmpTex.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            tmpTex.Apply(false); //RenderTextureじゃなくて普通のテクスチャに書き込む
            return tmpTex;
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TexToStr : UdonSharpBehaviour
{
    public Texture2D tmpTex;
    public Texture2D outputTex;
    public Parser parser;
    public VideoPlayerController video;

    private bool triggerCapture = false;
    private bool isTmpTexReady = false, isParsed = false, isVideoReady = false;

    public override void OnVideoReady()
    {
        isVideoReady = true;
    }

    public void OnPostRender()
    {
        if (triggerCapture && isVideoReady)
        {
            this.fromCamera();
            triggerCapture = false;
            isTmpTexReady = true;
            video.setFrame(decodeFrame++); //ここでシーク失敗してたときに同じフレーム読む気がする
        };
    }

    private int retryCount = 0;
    private int decodeIttr = 0;
    private bool isDecoding = false;
    private int decodeFrame = 0;
    private string decodeResult = "";
    private int decodeWait = 0;

    public void Update()
    {
        if (isDecoding && isTmpTexReady)
        {
            decodeWait++;
            if (decodeWait > 2)
            {
                if (decodeString(decodeIttr))
                {
                    Debug.Log("end decode");
                    isDecoding = false;
                    if (decodeResult != "")
                    {
                        parser.parse(decodeResult);
                        isParsed = true;
                    }
                    else
                        retryCount++;
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
                decodeWait = 0;
            }
        }

        if (isTmpTexReady && !isParsed && retryCount < 10 && !isDecoding)
        {
            Debug.Log("start decode");
            isDecoding = true;
        }
    }

    public float getDecodeProgress()
    {
        if (decodeIttr > 0 && !isDecoding) return 1.0F;
        return decodeIttr / 256F;
    }

    void Start()
    {
        reload();
    }

    private void capture()
    {
        isTmpTexReady = false;
        triggerCapture = true;
    }

    public void reload()
    {
        Debug.Log("Reloading..");
        capture();
        isVideoReady = false;
        isParsed = false;
        decodeResult = "";
        decodeIttr = 0;
        decodeFrame = 1;

        video.reload();
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

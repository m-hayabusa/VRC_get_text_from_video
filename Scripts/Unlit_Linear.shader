Shader "nekomimiStudio/Unlit_Linear"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // https://github.com/Unity-Technologies/PostProcessing/blob/4e01e2e06bc2be4afeef24f5d9eb547c30391fcc/PostProcessing/Shaders/Colors.hlsl#L156
            #define FLT_EPSILON 1.192092896e-07

            float3 PositivePow(float3 base, float3 power)
            {
                return pow(max(abs(base), float3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
            }
            
            half3 LinearToSRGB(half3 c)
            {
                half3 sRGBLo = c * 12.92;
                half3 sRGBHi = (PositivePow(c, half3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
                half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
                return sRGB;
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col;
                col.rgb = LinearToSRGB(tex2D(_MainTex, i.uv));
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}

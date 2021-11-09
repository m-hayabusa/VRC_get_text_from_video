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
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
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
                fixed4 col;
                // sqrt(c): https://github.com/Unity-Technologies/PostProcessing/blob/4e01e2e06bc2be4afeef24f5d9eb547c30391fcc/PostProcessing/Shaders/Colors.hlsl#L156
                col.rgb = sqrt(tex2D(_MainTex, i.uv));
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}

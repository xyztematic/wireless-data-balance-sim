Shader "Unlit/RangeVisualizer"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _RangeColor ("Range Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Offset -1, -1
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
            float4 _RangeColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 midpoint = float2(0.5,0.5);
                float dist = distance(i.uv, midpoint);
                if (dist > 0.5) discard;
                float d = smoothstep(distance(i.uv, midpoint), 0.5, 0.49);
                return d * _RangeColor;
            }
            ENDCG
        }
    }
}

Shader "Custom/DepthBlend"
{
    Properties
    {
        _RGBTex   ("RGB Texture",   2D) = "black" {}
        _DepthTex ("Depth Texture", 2D) = "black" {}
        _BlendMode ("Blend Mode (0=RGB, 1=Depth, 2=Mix)", Range(0,2)) = 2
        _MixStrength ("Mix Strength", Range(0,1)) = 0.4
        _DepthColorNear ("Near Color", Color) = (1, 0.4, 0, 1)
        _DepthColorFar  ("Far Color",  Color) = (0, 0.2, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _RGBTex;
            sampler2D _DepthTex;
            float     _BlendMode;
            float     _MixStrength;
            float4    _DepthColorNear;
            float4    _DepthColorFar;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 rgb   = tex2D(_RGBTex,   i.uv);
                fixed4 depth = tex2D(_DepthTex, i.uv);

                // 뎁스 밝기값 (0=가까움 밝음, 1=멀리)
                float depthVal = dot(depth.rgb, float3(0.299, 0.587, 0.114));
                // COLORMAP_JET 기준 파랑=멀리, 빨강=가까움 → 반전
                depthVal = 1 - depthVal;

                // 뎁스 컬러맵 (Near=주황, Far=파랑)
                fixed4 depthColored = lerp(_DepthColorFar, _DepthColorNear, depthVal);

                // 블렌드 모드
                fixed4 result;
                if (_BlendMode < 0.5)
                    result = rgb;                                         // RGB만
                else if (_BlendMode < 1.5)
                    result = depthColored;                                // Depth만
                else
                    result = lerp(rgb, depthColored, _MixStrength);      // 혼합

                return result;
            }
            ENDCG
        }
    }
}

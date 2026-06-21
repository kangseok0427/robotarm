Shader "Custom/HoloSkybox"
{
    Properties
    {
        _Color ("Glow Color", Color) = (0.2, 0.8, 1, 1)
        _GroundColor ("Ground Tint", Color) = (0.03, 0.07, 0.1, 1)
        _Opacity ("Glow Opacity", Range(0, 1)) = 0.5
        _BreatheSpeed ("Breathe Speed", Float) = 0.3
        _HorizonHeight ("Horizon Height", Range(-1, 1)) = -0.2
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float4 _GroundColor;
            float _Opacity, _BreatheSpeed, _HorizonHeight;

            struct appdata { float4 vertex : POSITION; float3 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float3 dir : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                // 지평선 기준 위(검정)/아래(은은한 컬러) 그라데이션
                float groundMix = smoothstep(_HorizonHeight + 0.4, _HorizonHeight - 0.4, dir.y);
                fixed3 baseCol = lerp(fixed3(0,0,0), _GroundColor.rgb, groundMix);

                // 아주 느리고 잔잔한 밝기 호흡 (전체적으로 균일하게)
                float breathe = sin(_Time.y * _BreatheSpeed) * 0.5 + 0.5;
                float ambientGlow = (0.3 + breathe * 0.2) * groundMix;

                fixed3 finalCol = baseCol + _Color.rgb * ambientGlow * _Opacity;
                return fixed4(finalCol, 1);
            }
            ENDCG
        }
    }
}

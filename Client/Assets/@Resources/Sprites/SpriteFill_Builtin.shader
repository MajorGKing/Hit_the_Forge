Shader "Custom/SpriteFillFlashCrack_Builtin"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _FillAmount("Fill Amount", Range(0,1)) = 1
        _Color("Color", Color) = (1,1,1,1)

        _FlashColor("Flash Color", Color) = (1,1,1,1)
        _FlashIntensity("Flash Intensity", Range(0,2)) = 0

        _CrackIntensity("Crack Intensity", Range(0,1)) = 0
        _CrackScale("Crack Scale", Range(1, 50)) = 10
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _FillAmount;
            float4 _Color;

            float4 _FlashColor;
            float _FlashIntensity;

            float _CrackIntensity;
            float _CrackScale;

            // Simple hash noise
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 435.345));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float i = hash21(floor(p));
                float f = hash21(floor(p + 1.0));
                return lerp(i, f, frac(p.x + p.y));
            }

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (i.uv.x > _FillAmount)
                    discard;

                fixed4 c = tex2D(_MainTex, i.uv) * _Color;

                // Flash effect
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashIntensity);

                // Crack noise
                float2 crackUV = i.uv * _CrackScale;
                float baseNoise = noise(crackUV);

                // Stretched vertical lines
                float2 stretchUV = float2(i.uv.x * _CrackScale, i.uv.y * (_CrackScale * 0.2));
                float lineNoise = noise(stretchUV);

                // Combine: more like cracks
                float crackMask = saturate(baseNoise * 0.5 + lineNoise * 1.5);

                c.rgb = lerp(c.rgb, c.rgb * crackMask, _CrackIntensity);

                return c;
            }
            ENDCG
        }
    }
}

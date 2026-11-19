Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness ("Outline Thickness (Screen Pixels)", Range(0, 64)) = 8
        [Toggle] _OutlineEnabled ("Outline Enabled", Float) = 0
        [Toggle] _OutlineOnly ("Outline Only", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _OutlineColor;
                float _OutlineThickness;
                float _OutlineEnabled;
                float _OutlineOnly;
            CBUFFER_END

            #define DIRECTION_COUNT 16
            #define RING_COUNT 6

            static const float2 Directions[DIRECTION_COUNT] =
            {
                float2( 1.0000,  0.0000), float2( 0.9239,  0.3827), float2( 0.7071,  0.7071), float2( 0.3827,  0.9239),
                float2( 0.0000,  1.0000), float2(-0.3827,  0.9239), float2(-0.7071,  0.7071), float2(-0.9239,  0.3827),
                float2(-1.0000,  0.0000), float2(-0.9239, -0.3827), float2(-0.7071, -0.7071), float2(-0.3827, -0.9239),
                float2( 0.0000, -1.0000), float2( 0.3827, -0.9239), float2( 0.7071, -0.7071), float2( 0.9239, -0.3827)
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;

                half neighbourAlpha = 0;

                float2 screenPixel = float2(length(ddx(input.uv)), length(ddy(input.uv)));

                if (_OutlineEnabled > 0.5 && sprite.a < 0.99)
                {
                    float2 offset = screenPixel * _OutlineThickness;

                    [unroll]
                    for (int ring = 1; ring <= RING_COUNT; ring++)
                    {
                        float2 ringOffset = offset * (ring / (float)RING_COUNT);

                        [unroll]
                        for (int i = 0; i < DIRECTION_COUNT; i++)
                        {
                            neighbourAlpha = max(neighbourAlpha,
                                SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, input.uv + Directions[i] * ringOffset, 0).a);
                        }
                    }
                }

                half outlineAlpha = neighbourAlpha * (1 - sprite.a) * _OutlineColor.a * _OutlineEnabled;

                if (_OutlineOnly > 0.5) return half4(_OutlineColor.rgb, outlineAlpha);

                half alpha = sprite.a + outlineAlpha;
                half3 color = (sprite.rgb * sprite.a + _OutlineColor.rgb * outlineAlpha) / max(alpha, 0.0001);

                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}

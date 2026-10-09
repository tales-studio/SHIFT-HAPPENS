Shader "Custom/SpriteOutlineHLSL"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        [HideInInspector] _OutlineWidth ("Outline Width", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                if (_OutlineWidth <= 0.0)
                    return c;

                if (c.a > 0.1)
                    return c;

                float maxAlpha = 0.0;
                
                // Clamp a un máximo de 16px para evitar lecturas excesivas de textura
                int width = min((int)_OutlineWidth, 16);

                // [loop] fuerza a HLSL a mantener la estructura de control de flujo en lugar de desenrollar el bucle
                [loop]
                for (int i = 1; i <= width; i++)
                {
                    float2 offsetX = float2(_MainTex_TexelSize.x * i, 0);
                    float2 offsetY = float2(0, _MainTex_TexelSize.y * i);

                    float aUp        = tex2D(_MainTex, IN.texcoord + offsetY).a;
                    float aDown      = tex2D(_MainTex, IN.texcoord - offsetY).a;
                    float aRight     = tex2D(_MainTex, IN.texcoord + offsetX).a;
                    float aLeft      = tex2D(_MainTex, IN.texcoord - offsetX).a;
                    float aUpRight   = tex2D(_MainTex, IN.texcoord + offsetX + offsetY).a;
                    float aDownLeft  = tex2D(_MainTex, IN.texcoord - offsetX - offsetY).a;
                    float aDownRight = tex2D(_MainTex, IN.texcoord + offsetX - offsetY).a;
                    float aUpLeft    = tex2D(_MainTex, IN.texcoord - offsetX + offsetY).a;

                    maxAlpha = max(maxAlpha, max(max(max(aUp, aDown), max(aRight, aLeft)),
                                            max(max(aUpRight, aDownLeft), max(aDownRight, aUpLeft))));

                    if (maxAlpha > 0.1)
                        break;
                }

                if (maxAlpha > 0.1)
                {
                    return fixed4(_OutlineColor.rgb * _OutlineColor.a, _OutlineColor.a);
                }

                return c;
            }
            ENDCG
        }
    }
}
// Credit goes to: https://blog.febucci.com/2019/06/sprite-outline-shader/
Shader "Custom/SpriteOutline_SortedSafe"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _OutlineColor("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Float) = 1.0
        _AlphaThreshold("Alpha Threshold", Float) = 0.5

        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Sprite"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "Always" }

            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
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
                half2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            fixed4 _OutlineColor;
            float _OutlineThickness;
            float _AlphaThreshold;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;

                fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Kill RGB on invisible pixels
                col.rgb *= col.a;

                float centerAlpha = col.a;

                float leftAlpha   = tex2D(_MainTex, IN.texcoord + float2(-offset.x, 0)).a;
                float rightAlpha  = tex2D(_MainTex, IN.texcoord + float2(offset.x, 0)).a;
                float upAlpha     = tex2D(_MainTex, IN.texcoord + float2(0, offset.y)).a;
                float downAlpha   = tex2D(_MainTex, IN.texcoord + float2(0, -offset.y)).a;

                float minNeighborAlpha = min(min(leftAlpha, rightAlpha), min(upAlpha, downAlpha));

                float edge = centerAlpha > _AlphaThreshold && minNeighborAlpha < _AlphaThreshold ? 1.0 : 0.0;

                return lerp(col, _OutlineColor * col.a, edge);
            }
            ENDCG
        }
    }
}

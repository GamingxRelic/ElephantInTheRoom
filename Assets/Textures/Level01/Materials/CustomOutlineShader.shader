// Credit goes to: https://blog.febucci.com/2019/06/sprite-outline-shader/
Shader "Unlit/CustomOutlineThick"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

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
            float4 _MainTex_TexelSize;

            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy * _OutlineThickness;

                float centerAlpha = tex2D(_MainTex, i.uv).a;

                // Sample surrounding alpha values
                float leftAlpha   = tex2D(_MainTex, i.uv + float2(-offset.x, 0)).a;
                float rightAlpha  = tex2D(_MainTex, i.uv + float2(offset.x, 0)).a;
                float upAlpha     = tex2D(_MainTex, i.uv + float2(0, offset.y)).a;
                float downAlpha   = tex2D(_MainTex, i.uv + float2(0, -offset.y)).a;

                float minNeighborAlpha = min(min(leftAlpha, rightAlpha), min(upAlpha, downAlpha));

                // Outline when current pixel is opaque but has transparent neighbor
                float edge = centerAlpha > 0.01 && minNeighborAlpha < 0.01 ? 1.0 : 0.0;

                fixed4 col = tex2D(_MainTex, i.uv);
                return lerp(col, _OutlineColor, edge);
            }

            ENDCG
        }
    }
}

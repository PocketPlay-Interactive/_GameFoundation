Shader "Dann/UI/Smart Image Effect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShineEnabled ("Shine Enabled", Float) = 0
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineWidth ("Shine Width", Float) = 0.18
        _ShineAngle ("Shine Angle", Float) = -25
        _ShinePosition ("Shine Position", Float) = 0
        _OutlineEnabled ("Outline Enabled", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 1
        _TilingEnabled ("Tiling Enabled", Float) = 0
        _TilingOffset ("Tiling Offset", Vector) = (0,0,0,0)
        _UvRect ("UV Rect", Vector) = (0,0,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;
            float _ShineEnabled;
            fixed4 _ShineColor;
            float _ShineWidth;
            float _ShineAngle;
            float _ShinePosition;
            float _OutlineEnabled;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _TilingEnabled;
            float4 _TilingOffset;
            float4 _UvRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                if (_TilingEnabled > 0.5)
                {
                    float2 localUv = (uv - _UvRect.xy) / max(_UvRect.zw, float2(0.0001, 0.0001));
                    uv = _UvRect.xy + frac(localUv + _TilingOffset.xy) * _UvRect.zw;
                }

                fixed4 color = (tex2D(_MainTex, uv) + _TextureSampleAdd) * IN.color;

                if (_OutlineEnabled > 0.5 && _OutlineWidth > 0.01)
                {
                    float2 stepUv = _MainTex_TexelSize.xy * _OutlineWidth;
                    float outlineAlpha = 0;
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, uv + float2(stepUv.x, 0)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, uv - float2(stepUv.x, 0)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, uv + float2(0, stepUv.y)).a);
                    outlineAlpha = max(outlineAlpha, tex2D(_MainTex, uv - float2(0, stepUv.y)).a);
                    fixed4 outline = _OutlineColor;
                    outline.a *= saturate(outlineAlpha - color.a);
                    color.rgb = lerp(outline.rgb, color.rgb, color.a);
                    color.a = max(color.a, outline.a);
                }

                if (_ShineEnabled > 0.5)
                {
                    float rad = radians(_ShineAngle);
                    float2 dir = float2(cos(rad), sin(rad));
                    float p = dot(IN.texcoord - 0.5, dir) + 0.5;
                    float shine = 1 - smoothstep(0, _ShineWidth, abs(p - _ShinePosition));
                    color.rgb += _ShineColor.rgb * _ShineColor.a * shine;
                }

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}

Shader "WorldOfThePack/SpriteFeedback"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0,2)) = 1
        _Opacity ("Opacity", Range(0,1)) = 1
        _AlphaThreshold ("Faint edge rejection", Range(0,.1)) = .025
        _FlashColor ("Hit flash", Color) = (1,1,.8,1)
        _FlashAmount ("Hit flash amount", Range(0,1)) = 0
        _Dissolve ("Dissolve", Range(0,1)) = 0
        _EdgeColor ("Dissolve edge", Color) = (1,.75,.2,1)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination blend", Float) = 10
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend [_SrcBlend] [_DstBlend], One OneMinusSrcAlpha
        Cull Off ZWrite Off
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            struct Attributes { COMMON_2D_INPUTS half4 color : COLOR; };
            struct Varyings { COMMON_2D_OUTPUTS half4 color : COLOR; float2 local : TEXCOORD5; };
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _Color, _FlashColor, _EdgeColor;
            float _Brightness, _Opacity, _AlphaThreshold, _FlashAmount, _Dissolve;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);
                Varyings o = CommonUnlitVertex(input);
                o.local = input.positionOS.xy;
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                c.a *= smoothstep(_AlphaThreshold, _AlphaThreshold + .01, c.a) * _Opacity;
                clip(c.a - .001);
                c.rgb = lerp(c.rgb * _Brightness, _FlashColor.rgb, _FlashAmount);
                if (_Dissolve > 0)
                {
                    float n = frac(sin(dot(floor(input.local * 32), float2(12.9898,78.233))) * 43758.5453);
                    clip(n - _Dissolve);
                    c.rgb = lerp(c.rgb, _EdgeColor.rgb, 1 - smoothstep(0,.09,n - _Dissolve));
                }
                return c;
            }
            ENDHLSL
        }
    }
}

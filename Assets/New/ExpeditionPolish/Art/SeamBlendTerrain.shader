Shader "WorldOfThePack/SeamBlendTerrain"
{
    Properties
    {
        [PerRendererData] _MainTex ("Terrain", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _EdgeBlend ("Edge blend", Range(0.001,0.1)) = 0.035
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Color, _RendererColor;
            float _EdgeBlend;
            struct Attributes { float3 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.color = input.color * _Color * _RendererColor;
                return output;
            }
            half4 Frag(Varyings input):SV_Target
            {
                half4 normal = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 mirrored = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(input.uv.x, 1-input.uv.y));
                float edge = min(input.uv.y, 1-input.uv.y);
                float weight = .5 * (1-smoothstep(0, _EdgeBlend, edge));
                return lerp(normal, mirrored, weight) * input.color;
            }
            ENDHLSL
        }
    }
}

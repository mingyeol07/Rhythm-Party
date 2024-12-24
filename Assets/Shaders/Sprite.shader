Shader "Custom/URP_SpriteLit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Color;
            float _Cutoff;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;

                // World position and normal
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.worldPosition = worldPos;
                o.worldNormal = float3(0, 0, -1); // 스프라이트의 기본 노멀 방향 (Z축)

                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Sample texture
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Apply tint color
                texColor *= _Color;

                // Alpha cutoff
                if (texColor.a < _Cutoff)
                    discard;

                // Lighting calculation
                Light mainLight = GetMainLight(); // URP에서 메인 라이트 데이터 가져오기
                float3 lightDir = normalize(mainLight.direction); // 방향성 라이트의 방향
                float3 lightColor = mainLight.color.rgb;          // 방향성 라이트의 색상

                // Diffuse lighting
                float NdotL = max(dot(i.worldNormal, lightDir), 0.0);
                float3 diffuse = texColor.rgb * lightColor * NdotL;

                return float4(diffuse, texColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
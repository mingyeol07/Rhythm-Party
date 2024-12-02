Shader "Custom/DonutShaderUI"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1) // Canvas Group 알파 반영
        _Thickness ("Thickness", Range(0, 0.5)) = 0.1
        _Radius ("Radius", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // 투명도 블렌딩
        ZWrite Off                      // 깊이 버퍼 비활성화
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Thickness;
            float _Radius;
            float4 _Color; // Canvas Group의 알파 값 포함

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
{
    float2 uv = i.uv - 0.5; // Center UV coordinates
    float dist = length(uv); // Distance from center

    // Check if the pixel is within the donut's thickness
    float alpha = step(_Radius - _Thickness, dist) * step(dist, _Radius);

    // 최종 Alpha 값은 CanvasGroup의 Alpha와 결합
    return fixed4(_Color.rgb, alpha * _Color.a);
}
            ENDCG
        }
    }
}
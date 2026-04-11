Shader "Custom/TMP_AlwaysVisible"
{
    Properties
    {
        _FaceColor ("Face Color", Color) = (1,1,1,1)
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceDilate ("Face Dilate", Range(-1,1)) = 0
        _Softness ("Softness", Range(0,1)) = 0.08
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 200
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FaceColor;
            float _FaceDilate;
            float _Softness;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 
                o.position = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float distance = tex2D(_MainTex, i.uv).a;
                float width = fwidth(distance);
                float alpha = smoothstep(0.5 - _Softness - _FaceDilate, 0.5 + _Softness - _FaceDilate, distance);
                return float4(_FaceColor.rgb, alpha * _FaceColor.a);
            }
            ENDHLSL
        }
    }
}
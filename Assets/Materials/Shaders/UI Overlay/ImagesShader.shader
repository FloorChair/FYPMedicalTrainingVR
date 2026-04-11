Shader "Custom/AlwaysVisibleVR"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay"}

        LOD 100

        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDHLSL
        }
    }
}
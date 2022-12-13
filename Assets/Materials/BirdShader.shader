Shader "Custom/BirdShader"
{
    Properties
    {
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }
        SubShader
    {
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        struct Input
        {
            float3 worldPos;
        };
        half _Smoothness;
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        StructuredBuffer<float3> _Positions;
        #endif
        float2 _Scale;
        void ConfigureProcedural()
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            float3 position = _Positions[unity_InstanceID];
            unity_ObjectToWorld = 0.0f;
            unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0f);
            unity_ObjectToWorld._m00_m11_m22 = _Scale.x;
            unity_WorldToObject = 0.0f;
            unity_WorldToObject._m03_m13_m23_m33 = float4(-position, 1.0f);
            unity_WorldToObject._m00_m11_m22 = _Scale.y;
            #endif
        }
        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
            surface.Smoothness = _Smoothness;
        }
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.5
        ENDCG
    }
    FallBack "Diffuse"
}

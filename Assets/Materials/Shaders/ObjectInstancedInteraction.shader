Shader "ObjectInstancedInteraction"
{
    Properties
    {
        _InteractionRadius("Interaction Radius",float) = 30
        _Color("Inactive color", Color) = (0, 0.8, 1, 1)
        _InteractColor("Active color", Color) = (1, .7, .0, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalRenderPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _Color;
            float4 _InteractColor;
            float _InteractionRadius;

            struct mesh_data
            {
                float3 basePos;
                float offsetY;
                float4x4 mat;
                float amount;
            };

            struct attributes
            {
                float3 normal : NORMAL;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct varyings
            {
                float4 vertex : SV_POSITION;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
            };

            StructuredBuffer<mesh_data> data;

            varyings vert(const attributes v, const uint instance_id : SV_InstanceID)
            {
                varyings o;

                const float3 pos = mul(data[instance_id].mat, v.vertex);
                o.vertex = mul(UNITY_MATRIX_MVP, float4(pos, 1.0f));
                o.diffuse = saturate(dot(v.normal, _MainLightPosition.xyz));
                o.color = lerp(_Color, _InteractColor, data[instance_id].amount);

                return o;
            }

            half4 frag(varyings i) : SV_Target
            {
                const float3 lighting = i.diffuse * 1.7;
                return half4(i.color * lighting, 1);;
            }
            ENDHLSL
        }
    }
}

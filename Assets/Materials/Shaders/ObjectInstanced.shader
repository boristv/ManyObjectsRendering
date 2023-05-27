Shader "Object Instanced"
{
    Properties
    {
        _Color("Color", Color) = (0, 0.8, 1, 1)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
                "RenderPipeline" = "UniversalRenderPipeline"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _Color;
            float3 _lastPos = float3(0, 0, 0);

            StructuredBuffer<float4> position_buffer_1;
            StructuredBuffer<float4> position_buffer_2;
            float4 color_buffer[8];

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

            float hash(float n)
            {
                return frac(sin(n) * 43758.5453);
            }
            
            float CustomNoise(float2 x)
            {
                float2 p = floor(x);
                float2 f = frac(x);
            
                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0;
            
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                            lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                            lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                                lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), 0);
            }

            varyings vert(attributes v, const uint instance_id : SV_InstanceID)
            {
                const float BURST_HEIGHT_SCALE = 2.5;
                const float NOISE_SCALE = 0.2;
                const float DEPTH_OFFSET = 1;
    
                float3 start = position_buffer_1[instance_id];
                float yOffset = position_buffer_2[instance_id];
    
                const float time = _Time.y;
                const float3 pos = start +_lastPos;
                const float t = smoothstep(yOffset, BURST_HEIGHT_SCALE + yOffset, pos.y);
                const float y = BURST_HEIGHT_SCALE * CustomNoise(float2(pos.x * NOISE_SCALE + time, pos.z * NOISE_SCALE + time)) + yOffset * DEPTH_OFFSET;
                const float3 newPos = float3(pos.x, y, pos.z);
                _lastPos = newPos - start;

                varyings o;
                o.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz + newPos, 1.0f));
                o.diffuse = saturate(dot(v.normal, _MainLightPosition.xyz));
                o.color = _Color;
                
                return o;
            }

            half4 frag(const varyings i) : SV_Target
            {
                const float3 lighting = i.diffuse *  1.7;
                return half4(i.color * lighting, 1);;
            }
            ENDHLSL
        }
    }
}

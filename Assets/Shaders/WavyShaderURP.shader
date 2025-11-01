Shader "Universal Render Pipeline/WavyShaderURP"
{
    Properties
    {
        _Color ("Line Color", Color) = (1,1,1,1)
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.01
        _Density ("Line Density", Range(0, 20)) = 8
        _TimeSpeed ("Time Speed", Range(-5, 5)) = 1.0
        _Seed ("Random Seed", Float) = 0.0
        _Scale ("Pattern Scale", Range(0.1, 50)) = 1.0 // Новый параметр масштаба
        [Enum(Closed, 0, Wavy, 1, Straight, 2)] _LineType ("Line Type", Int) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "UnlitPass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _LineWidth;
                float _Density;
                float _TimeSpeed;
                float _Seed;
                float _Scale; // Добавляем в буфер
                int _LineType;
            CBUFFER_END

            float hash(float2 p, float seed)
            {
                return frac(sin(dot(p + seed, float2(12.9898, 78.233))) * 43758.5453);
            }

            float noise(float2 p, float seed)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i, seed);
                float b = hash(i + float2(1.0, 0.0), seed);
                float c = hash(i + float2(0.0, 1.0), seed);
                float d = hash(i + float2(1.0, 1.0), seed);

                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Используем _Scale вместо фиксированного значения 10.0
                float2 uv = input.uv * _Scale;
                float time = _Time.y * _TimeSpeed;
                float lineAlpha = 0.0;

                if (_LineType == 0) // Closed — топографические контуры
                {
                    // Увеличиваем масштаб шума для большего размера паттерна
                    float wave1 = sin((uv.x * _Density * 0.5 + time) + noise(uv * 1.5, _Seed) * 2.0);
                    float wave2 = sin((uv.y * _Density * 0.5 + time * 0.7) + noise(uv * 1.5 + 1.0, _Seed) * 2.0);
                    float combined = wave1 + wave2;
                    float val = abs(frac(combined * 1.5) - 0.5);
                    lineAlpha = 1.0 - smoothstep(_LineWidth * 0.5, _LineWidth * 0.5 + 0.01, val);
                }
                else if (_LineType == 1) // Wavy — волнистые линии
                {
                    for (int i = 0; i < (int)_Density; i++)
                    {
                        float seedOffset = (float)i * 13.57 + _Seed;
                        float angle = hash(float2(seedOffset, 0.0), _Seed) * 6.28318530718;
                        float2 dir = float2(cos(angle), sin(angle));

                        // Уменьшаем множители для большего масштаба
                        float offset = hash(float2(seedOffset, 1.0), _Seed) * 5.0 + time * hash(float2(seedOffset, 2.0), _Seed);
                        float proj = dot(uv, dir);
                        float bend = noise(uv * 0.8 + float2(seedOffset, seedOffset * 0.3), _Seed) * 0.8;
                        float dist = abs(proj + bend - offset);

                        float lineValue = 1.0 - smoothstep(_LineWidth * 0.5, _LineWidth * 0.5 + 0.01, dist);
                        lineAlpha += lineValue;
                    }
                    lineAlpha = saturate(lineAlpha);
                }
                else if (_LineType == 2) // Straight — прямые линии
                {
                    for (int i = 0; i < (int)_Density; i++)
                    {
                        float seedOffset = (float)i * 17.31 + _Seed;
                        float angle = hash(float2(seedOffset, 0.0), _Seed) * 6.28318530718;
                        float2 dir = float2(cos(angle), sin(angle));
                        float2 perp = float2(-dir.y, dir.x);

                        float speed = hash(float2(seedOffset, 1.0), _Seed) * 2.0 - 1.0;
                        float offset = time * speed * 0.5 + hash(float2(seedOffset, 2.0), _Seed) * 3.0;

                        // Увеличиваем масштаб смещения
                        float dist = abs(dot(uv - _Scale * 0.5, perp) - offset);
                        float lineValue = 1.0 - smoothstep(_LineWidth * 0.5, _LineWidth * 0.5 + 0.01, dist);
                        lineAlpha += lineValue;
                    }
                    lineAlpha = saturate(lineAlpha);
                }

                float3 color = lerp(float3(0, 0, 0), _Color.rgb, lineAlpha);
                return float4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
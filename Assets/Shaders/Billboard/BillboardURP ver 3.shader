Shader "Custom/BillboardURP ver3"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.5
        _Brightness ("Brightness", Range(0, 3)) = 1.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Tint;
                float _TintStrength;
                float _Brightness;
                float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // Получаем центр объекта в мировом пространстве
                float3 centerWS = TransformObjectToWorld(float3(0,0,0));

                // Получаем масштаб объекта
                float3 scale = float3(
                    length(float3(UNITY_MATRIX_M[0].x, UNITY_MATRIX_M[1].x, UNITY_MATRIX_M[2].x)),
                    length(float3(UNITY_MATRIX_M[0].y, UNITY_MATRIX_M[1].y, UNITY_MATRIX_M[2].y)),
                    length(float3(UNITY_MATRIX_M[0].z, UNITY_MATRIX_M[1].z, UNITY_MATRIX_M[2].z))
                );

                // Получаем направление на камеру
                float3 viewDirWS = GetWorldSpaceViewDir(centerWS);
                float3 forward = normalize(viewDirWS);
                float3 up = float3(0, 1, 0);
                float3 right = normalize(cross(up, forward));
                up = normalize(cross(forward, right));

                // Применяем billboard трансформацию с учетом масштаба
                float3 vertexWS = centerWS;
                vertexWS += right * input.positionOS.x * scale.x;
                vertexWS += up * input.positionOS.y * scale.y;

                output.positionCS = TransformWorldToHClip(vertexWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                output.normalWS = -forward; // Нормаль смотрит на камеру

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Смешиваем оригинальный цвет с тинтом
                half4 tintedColor = lerp(texColor, texColor * _Tint, _TintStrength);

                // Применяем яркость
                half4 finalColor = tintedColor * _Brightness;

                // Alpha cutoff test
                clip(finalColor.a - _Cutoff);

                // Simple lighting
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                half3 lighting = mainLight.color * (NdotL * 0.5 + 0.5);
                finalColor.rgb *= lighting;

                // Fog
                finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
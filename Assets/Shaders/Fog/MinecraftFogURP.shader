Shader "Custom/MinecraftFogURP" 
{
    Properties 
    {
        // Основные текстуры
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        
        // Настройки тумана
        [Header(Fog Settings)]
        _FogDensity ("Fog Density", Range(0, 1)) = 0.1
        _FogHeightY ("Fog Height", Float) = 50
        _FogHeightFalloff ("Height Falloff", Range(1, 10)) = 5
        
        [Header(Fog Colors)]
        _FogColorDay ("Fog Color Day", Color) = (0.7, 0.8, 0.9, 1)
        _FogColorNight ("Fog Color Night", Color) = (0.1, 0.1, 0.3, 1)
        _SunFogColor ("Sun Fog Color", Color) = (1, 0.9, 0.7, 1)
    }
    
    SubShader 
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }
        
        Pass 
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // URP 6.2 includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
            // Структуры
            struct Attributes 
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings 
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                float3 fogColor : TEXCOORD5;
            };
            
            // Свойства
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _FogDensity;
                float _FogHeightY;
                float _FogHeightFalloff;
                float4 _FogColorDay;
                float4 _FogColorNight;
                float4 _SunFogColor;
            CBUFFER_END
            
            // Глобальные переменные (устанавливаем из скрипта)
            float _GlobalTimeBrightness;
            float _GlobalRainStrength;
            float3 _GlobalSunDirection;
            
            // Minecraft fog функция
            float3 CalculateMinecraftFog(float3 worldPos, float3 viewDir, out float fogAmount) 
            {
                float viewLength = length(viewDir);
                float3 normalizedView = normalize(viewDir);
                
                // Расстояние для тумана
                float distanceFog = viewLength * _FogDensity * 0.01;
                
                // Высотный туман (как в Minecraft)
                float heightFactor = exp2(-max(worldPos.y - _FogHeightY, 0.0) / 
                                         exp2(_FogHeightFalloff));
                distanceFog *= heightFactor;
                
                // View dot Up (для градиента неба)
                float VoU = saturate(dot(normalizedView, float3(0, 1, 0)));
                
                // View dot Light (для солнечного свечения)
                float VoL = saturate(dot(normalizedView, _GlobalSunDirection));
                
                // Плотность тумана на основе времени суток
                float density = lerp(0.8, 0.4, _GlobalTimeBrightness);
                
                // Базовый градиент (как в Minecraft)
                float baseGradient = exp(-(VoU * 0.5 + 0.5) * 0.5 / density);
                
                // Цвет тумана день/ночь
                float3 fogColor = lerp(_FogColorNight.rgb, _FogColorDay.rgb, 
                                       _GlobalTimeBrightness);
                fogColor *= baseGradient;
                
                // Добавляем солнечное свечение на горизонте
                float sunInfluence = pow((VoL * 0.5 + 0.5) * saturate(1.0 - VoU), 
                                        2.0 - _GlobalTimeBrightness) * 
                                    _GlobalTimeBrightness;
                
                float3 sunFog = _SunFogColor.rgb * baseGradient;
                fogColor = lerp(fogColor, sunFog, sunInfluence * 0.5);
                
                // Погодный эффект
                float3 rainFog = _FogColorDay.rgb * 0.5;
                fogColor = lerp(fogColor, rainFog, _GlobalRainStrength);
                
                // Финальный fog factor
                fogAmount = 1.0 - exp(-2.0 * pow(distanceFog, 1.25));
                fogAmount = saturate(fogAmount);
                
                return fogColor;
            }
            
            Varyings vert(Attributes IN) 
            {
                Varyings OUT;
                
                // Базовые трансформации
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = posInputs.positionCS;
                OUT.positionWS = posInputs.positionWS;
                
                // UV
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                
                // Нормали
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);
                OUT.normalWS = normInputs.normalWS;
                
                // View direction
                OUT.viewDirWS = GetCameraPositionWS() - OUT.positionWS;
                
                // Рассчитываем туман в вертекс шейдере (оптимизация)
                float fogAmount;
                OUT.fogColor = CalculateMinecraftFog(OUT.positionWS, 
                                                      OUT.viewDirWS, 
                                                      fogAmount);
                OUT.fogFactor = fogAmount;
                
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target 
            {
                // Базовый цвет объекта
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                baseColor *= _BaseColor;
                
                // Простое освещение (для URP 6.2)
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                half3 lighting = mainLight.color * NdotL;
                
                // Применяем освещение
                half3 color = baseColor.rgb * lighting;
                
                // Применяем Minecraft fog
                color = lerp(color, IN.fogColor, IN.fogFactor);
                
                return half4(color, baseColor.a);
            }
            ENDHLSL
        }
        
        // Shadow pass для URP 6.2
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
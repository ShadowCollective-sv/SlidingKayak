Shader "Megxlord/GridShaderURP"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map (RGB)", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _TileScale("Tile Scale", Float) = 0.95
        _Tiling("Tiling", Float) = 1.0
        _TileColor("Tile Color", Color) = (0.5283019, 0.5283019, 0.5283019, 0)
        _2nd_Tiling("2nd Tiling", Float) = 5.0
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
        [KeywordEnum(Opaque, Transparent)] _SurfaceType("Surface Type", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1.0 // One
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0.0 // Zero
        [Toggle(_ALPHATEST_ON)] _AlphaClip("Alpha Clipping", Float) = 0.0
        [HideInInspector] _QueueOffset("Queue Offset", Float) = 0.0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }
        LOD 300
        // Forward Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite On
            ZTest LEqual
            Cull Back
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _ _REFLECTIONS_OFF
            #pragma shader_feature_local _ _DISABLE_DECALS
            #pragma shader_feature_local _ _DISABLE_SSR
            #pragma shader_feature_local _ _DISABLE_SSR_TRANSPARENT
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF
            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv0 : TEXCOORD2;
                half4 fogFactorAndVertexLight : TEXCOORD3;
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord : TEXCOORD4;
                #endif
                #ifdef LIGHTMAP_ON
                    float2 staticLightmapUV : TEXCOORD5;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    float2 dynamicLightmapUV : TEXCOORD6;
                #endif
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };
            // Material properties
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _TileColor;
            float _TileScale;
            float _Tiling;
            float _2nd_Tiling;
            half4 _EmissionColor;
            // Helper function to create a rectangle shape (like Unity's Rectangle node in SG)
            float Rectangle(float2 uv, float width, float height)
            {
                float2 d = abs(uv * 2 - 1) - float2(width, height);
                d = saturate(1 - d / fwidth(d));
                return min(d.x, d.y);
            }
            // Triplanar sampling for texture
            half4 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 worldPos, float3 blendWeights, float4 tilingOffset)
            {
                half4 texX = SAMPLE_TEXTURE2D(tex, samp, worldPos.zy * tilingOffset.xy + tilingOffset.zw);
                half4 texY = SAMPLE_TEXTURE2D(tex, samp, worldPos.xz * tilingOffset.xy + tilingOffset.zw);
                half4 texZ = SAMPLE_TEXTURE2D(tex, samp, worldPos.xy * tilingOffset.xy + tilingOffset.zw);
                return texX * blendWeights.x + texY * blendWeights.y + texZ * blendWeights.z;
            }
            // Triplanar grid mask
            float TriplanarGridMask(float3 worldPos, float3 blendWeights, float tiling, float tileScale)
            {
                float2 gridUVx = worldPos.zy * tiling;
                float2 gridFracX = frac(gridUVx);
                float gridMaskX = Rectangle(gridFracX, tileScale, tileScale);

                float2 gridUVy = worldPos.xz * tiling;
                float2 gridFracY = frac(gridUVy);
                float gridMaskY = Rectangle(gridFracY, tileScale, tileScale);

                float2 gridUVz = worldPos.xy * tiling;
                float2 gridFracZ = frac(gridUVz);
                float gridMaskZ = Rectangle(gridFracZ, tileScale, tileScale);

                return gridMaskX * blendWeights.x + gridMaskY * blendWeights.y + gridMaskZ * blendWeights.z;
            }
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);
                #endif
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
                half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv0 = TRANSFORM_TEX(input.uv0, _BaseMap);
                output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    output.shadowCoord = TransformWorldToShadowCoord(output.positionWS);
                #endif
                #ifdef LIGHTMAP_ON
                    output.staticLightmapUV = input.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    output.dynamicLightmapUV = input.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                #endif
                float3 worldPos = input.positionWS;
                half3 blendWeights = abs(input.normalWS);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001); // Avoid div by zero
                // --- Grid Calculation (Triplanar) ---
                float gridMask = TriplanarGridMask(worldPos, blendWeights, _Tiling, _TileScale);
                float secondGridMask = TriplanarGridMask(worldPos, blendWeights, _2nd_Tiling, _TileScale);
                float combinedGrid = gridMask * secondGridMask;
                // --- End Grid Calculation ---
                half4 baseMapColor = SampleTriplanar(TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), worldPos, blendWeights, _BaseMap_ST);
                half4 finalColor = lerp(_BaseColor, _TileColor, combinedGrid) * baseMapColor;
                // Surface properties for UniversalFragmentPBR
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalColor.rgb;
                surfaceData.alpha = finalColor.a;
                surfaceData.specular = half3(0.0, 0.0, 0.0);
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = 0.5;
                surfaceData.occlusion = 1.0;
                #ifdef _EMISSION
                    surfaceData.emission = _EmissionColor.rgb * surfaceData.albedo;
                #else
                    surfaceData.emission = half3(0, 0, 0);
                #endif
                surfaceData.normalTS = half3(0.0, 0.0, 1.0);
                // Input data for lighting calculations
                InputData inputData;
                inputData.positionWS = input.positionWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    inputData.shadowCoord = input.shadowCoord;
                #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
                    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
                #else
                    inputData.shadowCoord = float4(0, 0, 0, 0);
                #endif
                inputData.fogCoord = input.fogFactorAndVertexLight.x;
                inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
                #ifdef LIGHTMAP_ON
                    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, inputData.normalWS);
                    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);
                #else
                    inputData.bakedGI = SampleSH(inputData.normalWS);
                    inputData.shadowMask = half4(1, 1, 1, 1);
                #endif
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                #if defined(_ALPHATEST_ON)
                    clip(surfaceData.alpha - 0.5);
                #endif
                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }
        // DepthOnly Pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv0 : TEXCOORD2;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _TileColor;
            float _TileScale;
            float _Tiling;
            float _2nd_Tiling;
            // Helper function to create a rectangle shape
            float Rectangle(float2 uv, float width, float height)
            {
                float2 d = abs(uv * 2 - 1) - float2(width, height);
                d = saturate(1 - d / fwidth(d));
                return min(d.x, d.y);
            }
            // Triplanar grid mask (simplified for depth, using normal)
            float TriplanarGridMask(float3 worldPos, float3 blendWeights, float tiling, float tileScale)
            {
                float2 gridUVx = worldPos.zy * tiling;
                float2 gridFracX = frac(gridUVx);
                float gridMaskX = Rectangle(gridFracX, tileScale, tileScale);

                float2 gridUVy = worldPos.xz * tiling;
                float2 gridFracY = frac(gridUVy);
                float gridMaskY = Rectangle(gridFracY, tileScale, tileScale);

                float2 gridUVz = worldPos.xy * tiling;
                float2 gridFracZ = frac(gridUVz);
                float gridMaskZ = Rectangle(gridFracZ, tileScale, tileScale);

                return gridMaskX * blendWeights.x + gridMaskY * blendWeights.y + gridMaskZ * blendWeights.z;
            }
            // Triplanar texture sampling
            half4 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 worldPos, float3 blendWeights, float4 tilingOffset)
            {
                half4 texX = SAMPLE_TEXTURE2D(tex, samp, worldPos.zy * tilingOffset.xy + tilingOffset.zw);
                half4 texY = SAMPLE_TEXTURE2D(tex, samp, worldPos.xz * tilingOffset.xy + tilingOffset.zw);
                half4 texZ = SAMPLE_TEXTURE2D(tex, samp, worldPos.xy * tilingOffset.xy + tilingOffset.zw);
                return texX * blendWeights.x + texY * blendWeights.y + texZ * blendWeights.z;
            }
            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);
                #endif
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, float4(0,0,0,0)); // No tangent needed
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv0 = TRANSFORM_TEX(input.uv0, _BaseMap);
                return output;
            }
            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                #endif
                // Compute alpha WITHOUT grid for solid depth (consistent clipping without grid holes)
                float3 worldPos = input.positionWS;
                half3 blendWeights = abs(input.normalWS);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001); // Avoid div by zero
                half4 baseMapColor = SampleTriplanar(TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), worldPos, blendWeights, _BaseMap_ST);
                half finalAlpha = _BaseColor.a * baseMapColor.a; // Ignore grid for depth
                #if defined(_ALPHATEST_ON)
                    clip(finalAlpha - 0.5);
                #else
                    clip(finalAlpha - 0.01);
                #endif
                return 0;
            }
            ENDHLSL
        }
        // ShadowCaster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                float2 uv0 : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _TileColor;
            float _TileScale;
            float _Tiling;
            float _2nd_Tiling;
            // Helper function to create a rectangle shape
            float Rectangle(float2 uv, float width, float height)
            {
                float2 d = abs(uv * 2 - 1) - float2(width, height);
                d = saturate(1 - d / fwidth(d));
                return min(d.x, d.y);
            }
            // Triplanar grid mask (simplified for shadow, using normal)
            float TriplanarGridMask(float3 worldPos, float3 blendWeights, float tiling, float tileScale)
            {
                float2 gridUVx = worldPos.zy * tiling;
                float2 gridFracX = frac(gridUVx);
                float gridMaskX = Rectangle(gridFracX, tileScale, tileScale);

                float2 gridUVy = worldPos.xz * tiling;
                float2 gridFracY = frac(gridUVy);
                float gridMaskY = Rectangle(gridFracY, tileScale, tileScale);

                float2 gridUVz = worldPos.xy * tiling;
                float2 gridFracZ = frac(gridUVz);
                float gridMaskZ = Rectangle(gridFracZ, tileScale, tileScale);

                return gridMaskX * blendWeights.x + gridMaskY * blendWeights.y + gridMaskZ * blendWeights.z;
            }
            // Triplanar texture sampling
            half4 SampleTriplanar(TEXTURE2D_PARAM(tex, samp), float3 worldPos, float3 blendWeights, float4 tilingOffset)
            {
                half4 texX = SAMPLE_TEXTURE2D(tex, samp, worldPos.zy * tilingOffset.xy + tilingOffset.zw);
                half4 texY = SAMPLE_TEXTURE2D(tex, samp, worldPos.xz * tilingOffset.xy + tilingOffset.zw);
                half4 texZ = SAMPLE_TEXTURE2D(tex, samp, worldPos.xy * tilingOffset.xy + tilingOffset.zw);
                return texX * blendWeights.x + texY * blendWeights.y + texZ * blendWeights.z;
            }
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);
                #endif
                float3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz) + worldNormal * 0.001f;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = worldNormal;
                output.uv0 = TRANSFORM_TEX(input.uv0, _BaseMap);
                return output;
            }
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                #endif
                // Compute alpha WITHOUT grid for solid shadows (consistent casting without grid holes)
                float3 worldPos = input.positionWS;
                half3 blendWeights = abs(input.normalWS);
                blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z + 0.0001); // Avoid div by zero
                half4 baseMapColor = SampleTriplanar(TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap), worldPos, blendWeights, _BaseMap_ST);
                half finalAlpha = _BaseColor.a * baseMapColor.a; // Ignore grid for shadows
                #if defined(_ALPHATEST_ON)
                    clip(finalAlpha - 0.5);
                #else
                    clip(finalAlpha - 0.01);
                #endif
                return 0;
            }
            ENDHLSL
        }
        // Universal2D Pass (kept UV-based for 2D compatibility, with grid for color but solid for alpha if needed)
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite On
            ZTest LEqual
            Cull Back
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                #endif
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                #endif
            };
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _TileColor;
            float _TileScale;
            float _Tiling;
            float _2nd_Tiling;
            // Helper function to create a rectangle shape
            float Rectangle(float2 uv, float width, float height)
            {
                float2 d = abs(uv * 2 - 1) - float2(width, height);
                d = saturate(1 - d / fwidth(d));
                return min(d.x, d.y);
            }
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, output);
                #endif
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv0 = TRANSFORM_TEX(input.uv0, _BaseMap);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                #if UNITY_ANY_INSTANCING_ENABLED
                    UNITY_SETUP_INSTANCE_ID(input);
                #endif
                // --- Grid Calculation (UV-based for 2D) ---
                float2 gridUV = input.uv0 * _Tiling;
                float2 gridUVFrac = frac(gridUV);
                float gridMask = Rectangle(gridUVFrac, _TileScale, _TileScale);
                float2 secondTilingUV = gridUV * _2nd_Tiling;
                float2 secondTilingUVFrac = frac(secondTilingUV);
                float secondGridMask = Rectangle(secondTilingUVFrac, _TileScale, _TileScale);
                float combinedGrid = gridMask * secondGridMask;
                // --- End Grid Calculation ---
                half4 baseMapColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv0);
                half4 finalColor = lerp(_BaseColor, _TileColor, combinedGrid) * baseMapColor;
                return finalColor;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUIs.UniversalBaseShaderGUI"
}
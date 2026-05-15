Shader "Custom/HalfHalf_URP"
{
    Properties
    {
        _BaseColor ("Color Left", Color) = (1,1,1,1)
        _BaseColor2 ("Color Right", Color) = (1,1,1,1)
        _HColor ("Highlight Color", Color) = (1,1,1,1)
        _SColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
        _RampThreshold ("Threshold", Range(0.01,1)) = 0.75
        _RampSmoothing ("Smoothing", Range(0,1)) = 0.1
        _OutlineWidth ("Width", Range(0,10)) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        // Outline
        Pass
        {
            Name "Outline"
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
                float4 _BaseColor;
                float4 _BaseColor2;
                float4 _HColor;
                float4 _SColor;
                float _RampThreshold;
                float _RampSmoothing;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                posWS += normalWS * (_OutlineWidth * 0.01);
                OUT.positionCS = TransformWorldToHClip(posWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // Main
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseColor2;
                float4 _HColor;
                float4 _SColor;
                float _RampThreshold;
                float _RampSmoothing;
                float _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Object space X'e göre renk seç
                half4 baseColor = IN.positionOS.x < 0 ? _BaseColor : _BaseColor2;

                // Ramp shading
                Light mainLight = GetMainLight();
                float3 normal = normalize(IN.normalWS);
                float ndotl = dot(normal, mainLight.direction);
                float ramp = smoothstep(_RampThreshold - _RampSmoothing, _RampThreshold + _RampSmoothing, ndotl);

                float3 col = lerp(_SColor.rgb, _HColor.rgb, ramp);
                col *= baseColor.rgb * mainLight.color;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
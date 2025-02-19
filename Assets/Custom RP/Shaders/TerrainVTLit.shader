﻿Shader "Custom RP/TerrainVTLit"
{
    Properties
    {
        _Diffuse ("_Diffuse", 2D) = "white" {}
        _Normal ("Normal", 2D) = "grey" {}
    }
    SubShader
    {
        HLSLINCLUDE
        #include "../ShaderLibrary/Common.hlsl"
        #include "../ShaderLibrary/LitInput.hlsl"
        ENDHLSL
        

        Tags 
        { 
            "LightMode" = "CustomLit" 
            "PreviewType" = "Sphere"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _RECEIVE_SHADOWS
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma shader_feature _NORMAL_MAP
            #pragma shader_feature _MASK_MAP
            #pragma shader_feature _DETAIL_MAP
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _OTHER_PCF3 _OTHER_PCF5 _OTHER_PCF7
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
            #pragma multi_compile _ _SHADOW_MASK_ALWAYS _SHADOW_MASK_DISTANCE 
            #pragma multi_compile _ LOD_FADE_CROSSFADE LOD_FADE_PERCENTAGE 
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ _LIGHTS_PER_OBJECT
            #pragma multi_compile _ USE_CLUSTER_LIGHT
            // #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            // #pragma instancing_options assumeuniformscaling
            // make fog work
            #pragma multi_compile_fog
            
            #pragma enable_d3d11_debug_symbols

            #pragma vertex vert
            #pragma fragment frag

            #include "../ShaderLibrary/VTTerrainLitInput.hlsl"
            #include "../ShaderLibrary/TerrainVTLit.hlsl"
            ENDHLSL
        }

        Pass
        {
            Tags 
            {
                "LightMode" = "ShadowCaster"
            }
            
            ColorMask 0
            HLSLPROGRAM
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma shader_feature _ _SHADOWS_CLIP _SHADOWS_DITHER
            #pragma multi_compile _ LOD_FADE_CROSSFADE LOD_FADE_PERCENTAGE 
            #pragma vertex ShadowCasterVert
            #pragma fragment ShadowCasterFrag

            #include "../ShaderLibrary/ShadowCaster.hlsl"
            
            ENDHLSL
        }

        Pass
        {
            Tags
            {
                "LightMode" = "Meta"
            }
            
            Cull Off
            
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex MetaPassVert
            #pragma fragment MetaPassFrag
            
            #include "../ShaderLibrary/MetaPass.hlsl"
            ENDHLSL
        }
        
    }
}
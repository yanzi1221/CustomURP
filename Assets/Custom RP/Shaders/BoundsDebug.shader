﻿Shader "Custom RP/Terrain/BoundsDebug"
{
    Properties
    {
    }
    
    SubShader
    {
        HLSLINCLUDE
        #include "../ShaderLibrary/Common.hlsl"
        #include "../ShaderLibrary/LitInput.hlsl"
        #include "../ShaderLibrary/TerrainInput.hlsl"
        ENDHLSL

        Blend One OneMinusSrcAlpha

        Tags
        {
            "RenderType" = "Opaque"
            "LightMode" = "CustomLit"
        }
        
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature _ENABLE_MIP_DEBUG

            #include "../ShaderLibrary/BoundsDebug.hlsl"
            ENDHLSL
        }
    }
}
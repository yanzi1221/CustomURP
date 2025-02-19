#ifndef __SHADER_LIBRARY_BILATERAL_BLUR_HLSL__
#define __SHADER_LIBRARY_BILATERAL_BLUR_HLSL__



Varyins vert(Attributes input)
{
    Varyins o;
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    o.positionCS = TransformWorldToHClip(positionWS);
    o.uv = input.uv;
    return o;
}

float4 highHorizontalFrag(Varyins input) : SV_TARGET
{
    return BilateralBlur(input, int2(1, 0), _CameraDepthTexture,
                    sampler_point_clamp, FULL_RES_BLUR_KERNEL_SIZE, _CameraBufferSize.xy);
}

float4 highVerticalFrag(Varyins input) : SV_TARGET
{
    return BilateralBlur(input, int2(0, 1), _CameraDepthTexture,
        sampler_point_clamp, FULL_RES_BLUR_KERNEL_SIZE, _CameraBufferSize.xy);
}

float4 lowHorizontalFrag(Varyins input) : SV_TARGET
{
    return BilateralBlur(input, int2(1, 0), _HalfResDepthBuffer, sampler_HalfResDepthBuffer, 
                HALF_RES_BLUR_KERNEL_SIZE, _HalfResDepthBuffer_TexelSize.xy);

}

float4 lowVerticalFrag(Varyins input) : SV_TARGET
{
    return BilateralBlur(input, int2(0, 1), _HalfResDepthBuffer, sampler_HalfResDepthBuffer, HALF_RES_BLUR_KERNEL_SIZE, _HalfResDepthBuffer_TexelSize.xy);

}

DownSample vertHalfDepth(Attributes input)
{
    return vertDownSampleDepth(input, _CameraBufferSize.xy);
}


float fragHalfDepth(DownSample input) : SV_TARGET
{
   return DownSampleDepth(input, _CameraDepthTexture, sampler_point_clamp);
}

UpSample vertUpSampleToFull(Attributes input)
{
    return vertUpSample(input, _HalfResDepthBuffer_TexelSize);
}

float4 fragUpSampleToFull(UpSample input) : SV_TARGET
{
    return BilateralUpSample(input, _CameraDepthTexture, _HalfResDepthBuffer,
                        _HalfResColor, sampler_HalfResColor, sampler_HalfResDepthBuffer);
}

DownSample vertQuarterDepth(Attributes input)
{
    return vertDownSampleDepth(input, _HalfResDepthBuffer_TexelSize);
}

float fragQuarterDepth(DownSample input) : SV_TARGET
{
    return DownSampleDepth(input, _HalfResDepthBuffer, sampler_HalfResDepthBuffer);
}

UpSample vertQuarterUpSampleToFull(Attributes input)
{
    return vertUpSample(input, _QuarterResDepthBuffer_TexelSize);
}

float4 fragQuarterUpSampleToFull(UpSample input) : SV_TARGET
{
    return BilateralUpSample(input, _CameraDepthTexture, _QuarterResDepthBuffer,
                        _QuarterResColor, sampler_QuarterResColor, sampler_QuarterResDepthBuffer);
}

float4 quarterhorizontalFrag(Varyins input) : SV_TARGET
{
    return BilateralBlur(input, int2(1, 0), _QuarterResDepthBuffer,
                    sampler_QuarterResDepthBuffer, QUARTER_RES_BLUR_KERNEL_SIZE, _QuarterResDepthBuffer_TexelSize.xy);
}

float4 quarterVerticalFrag(Varyins input) : SV_TARGET
{
    return BilateralBlur(input, int2(0, 1), _QuarterResDepthBuffer,
                    sampler_QuarterResDepthBuffer, QUARTER_RES_BLUR_KERNEL_SIZE, _QuarterResDepthBuffer_TexelSize.xy);

}

#endif
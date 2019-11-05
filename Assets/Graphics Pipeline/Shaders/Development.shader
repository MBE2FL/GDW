Shader "MyPipeline/Development"
{
    Properties
    {
    }
    SubShader
    {
		Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
			#pragma multi_compile_local __ BOUND_DEBUG
            #pragma vertex vert
            #pragma fragment frag
			#include "UnityCG.cginc"
			#include "PrimitiveFunctions.hlsl"
			#include "Development.hlsl"

            ENDHLSL
        }
    }
}

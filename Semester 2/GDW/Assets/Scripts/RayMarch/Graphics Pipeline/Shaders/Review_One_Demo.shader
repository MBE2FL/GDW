Shader "MyPipeline/Review_One_Demo"
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
            #pragma vertex vert
            #pragma fragment frag
			#include "UnityCG.cginc"
			#include "Review_One_Demo.hlsl"

            ENDHLSL
        }
    }
}

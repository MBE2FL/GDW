﻿Shader "Unlit/PortalCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        Lighting Off
        Cull Back
        ZWrite On
        ZTest Less

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            VertexOutput vert (VertexInput input)
            {
                VertexOutput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.screenPos = ComputeScreenPos(output.vertex);
                output.uv = input.uv;
                return output;
            }

            sampler2D _MainTex;
			float _totalTime;

            float4 frag (VertexOutput input) : SV_Target
            {
                input.screenPos /= input.screenPos.w; 
				input.uv.x = input.uv.x * 2.0 - 1.0;
				input.uv.y = input.uv.y * 2.0 - 1.0;
				//float4 col = tex2D(_MainTex, float2(input.screenPos.x + 0.001 * sin(input.uv.x * _Time.y * -50.0),
									//					input.screenPos.y + 0.002 * sin(input.uv.y * _Time.y * 30.0)
									//));
				float displaceX = 0.001 * (sin(input.uv.x * (sin(_Time.y * 1.3) * 0.8 + 0.2) * 100.0) * 0.9 + 0.1);
				//displaceX = 0.0;
				float displaceY = 0.0008 * sin(input.uv.y * (sin(_Time.y * 2.0)) * 50.0);
				//displaceY = 0.0;

				float4 col = tex2D(_MainTex, float2(input.screenPos.x + displaceX,
									input.screenPos.y + displaceY
									));

				//col.rgb = abs(sin(input.uv.yyy * frac(_totalTime)));
				//col.rgb = sin(input.uv.x * (sin(_Time.y) * 0.8 + 0.2) * 100.0);
                return col;
            }
            ENDHLSL
        }
    }
}

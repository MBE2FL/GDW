Shader "Unlit/PortalCutout"
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
                //float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            VertexOutput vert (VertexInput input)
            {
                VertexOutput output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.screenPos = ComputeScreenPos(output.vertex);
                //output.uv = input.uv;
                return output;
            }

            sampler2D _MainTex;

            float4 frag (VertexOutput input) : SV_Target
            {
                input.screenPos /= input.screenPos.w; 
                float4 col = tex2D(_MainTex, float2(input.screenPos.x, input.screenPos.y));

                return col;
            }
            ENDHLSL
        }
    }
}

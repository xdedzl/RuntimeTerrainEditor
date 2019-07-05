
Shader "RunTimeHandles/Shape" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_ZWrite("ZWrite", Float) = 0.0
		_ZTest("ZTest", Float) = 0.0
		_Cull("Cull", Float) = 0.0
		_OFactors("OFactors", Float) = 0.0
		_OUnits("OUnits", Float) = 0.0
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent+10" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			ZTest[_ZTest]
			ZWrite[_ZWrite]
			Offset [_OFactors], [_OUnits]
			CGPROGRAM

			#include "UnityCG.cginc"
			#pragma vertex vert  
			#pragma fragment frag 

			struct vertexInput {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 color: COLOR;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 color: COLOR;
			};

			fixed4 _Color;


			inline float4 GammaToLinearSpace(float4 sRGB)
			{
				if (IsGammaSpace())
				{
					return sRGB;
				}
				return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
			}

			vertexOutput vert(vertexInput input)
			{
				float3 viewNorm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, input.normal));
				vertexOutput output;
				output.pos = UnityObjectToClipPos(input.vertex);
				output.color = input.color * 1.5 * dot(viewNorm, float3(0, 0, 1));
				output.color = GammaToLinearSpace(output.color);
				output.color.a = input.color.a;

				
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				return _Color * input.color;
			}	

			ENDCG
		}
	}
}
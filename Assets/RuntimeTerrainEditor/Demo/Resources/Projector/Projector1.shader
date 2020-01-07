Shader "Custom/Projector1" {
	Properties {
		//调色
		_Color ("Color", Color) = (1,1,1,1)
		//投影图片
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	    //根据投影仪视距渐变的图片
		_FalloffTex("Falloff",2D)="white"{}
	}
		SubShader{
			Pass{
				ZWrite Off
				//解决ZFighting现象
				Offset -1, -1
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float4 _Color;
	            //将投影仪剪辑空间的X和Y轴映射到U和V坐标，这些坐标通常用于对径向衰减纹理进行采样。
				float4x4 unity_Projector;
				//将投影仪视图空间的Z轴映射到U坐标（可能在V中复制它），该坐标可用于采样渐变纹理，该纹理定义投影仪随距离衰减。u值在投影仪近平面处为0，在投影仪远平面处为1。
	            float4x4 unity_ProjectorClip;
				sampler2D _MainTex;
				sampler2D _FalloffTex;

				struct v2f {
					float4 uvDecal:TEXCOORD0;
					float4 uvFalloff:TEXCOORD1;
					float4 pos:SV_POSITION;
				};

				v2f vert(appdata_img v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					//四元纹理坐标给UNITY_PROJ_COORD读取					
					o.uvDecal = mul(unity_Projector, v.vertex);					
					o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
					return o;
				}

				float4 frag(v2f i) :SV_Target{
					float4 decal;
				    //解决图片四周拖影
					if (i.uvDecal.x/i.uvDecal.w<0.0001|| i.uvDecal.x / i.uvDecal.w>0.9999|| i.uvDecal.y / i.uvDecal.w<0.0001 || i.uvDecal.y / i.uvDecal.w>0.9999)
					{
						decal = float4(0, 0, 0, 0);
					}
					else
					{
						//采样齐次uv，分量都除以了w
						decal = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uvDecal));
					}
					float falloff = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff)).r;
					return float4(decal.rgb*_Color.rgb,decal.a * falloff*_Color.a);
				}

		        ENDCG

            }
	}
}
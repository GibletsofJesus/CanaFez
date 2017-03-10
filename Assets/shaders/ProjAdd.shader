Shader "Projector/Additive" {
	Properties {
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
		_Power ("Power", Float) = 1
		_SliceGuide("_SliceGuide (RGB)", 2D) = "white" {}
		_SliceAmount("Slice Amount", Range(0.0, 1.0)) = 0.5
		}
		Subshader {
			//Tags {"Queue"="Transparent"}
			Pass {
				ZWrite Off
				ColorMask RGB
				Blend SrcAlpha OneMinusSrcAlpha
				//Blend One One
				Offset -1, -1
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				struct v2f {
					float4 uvShadow : TEXCOORD0;
					float4 uvFalloff : TEXCOORD1;
					float4 uvSlice : TEXCOORD2;
					float4 pos : SV_POSITION;
				};
				float4x4 unity_Projector;
				float4x4 unity_ProjectorClip;

			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				o.uvSlice = mul (unity_Projector,vertex);
				return o;
			}
		sampler2D _ShadowTex;
		sampler2D _FalloffTex;
		sampler2D _SliceGuide;
		float _SliceAmount;
 
		float _Power;
		fixed4 frag (v2f i) : COLOR//SV_Target
		{
			clip(tex2Dproj(_SliceGuide,UNITY_PROJ_COORD(i.uvSlice)).rgb - _SliceAmount);
			fixed4 texS = tex2Dproj (_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
			fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
			fixed4 res = lerp(fixed4(1,1,1,0), texS, texF.a);
			return res * _Power;
		}
		ENDCG
		}
	}
}
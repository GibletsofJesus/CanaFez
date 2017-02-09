Shader "Sprite/Crossfade Alpha Blended" {

Properties {
	_Fade ("Fade", Range(0, 1)) = 0
	_MainTex ("Texture A", 2D) = "white" {}
	_TexB ("Texture B", 2D) = "white" {}
	_Tint("Tint Colour", color) = (1,1,1,1)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	ZWrite Off

	SubShader {
		ColorMask RGB
		Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_fog_exp2
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"
				
				struct appdata_tiny {
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
				};
				
				struct v2f { 
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
				};
				
				uniform float4	_MainTex_ST,
								_TexB_ST;
				fixed4 _Tint;

				v2f vert (appdata_tiny v)
				{
					v2f o;
					o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
					o.uv2 = TRANSFORM_TEX(v.texcoord1,_TexB);
					return o;
				}
				
				uniform float _Fade;
				uniform sampler2D	_MainTex,
									_TexB;
				
				fixed4 frag (v2f i) : COLOR
				{
					half4	tA = tex2D(_MainTex, i.uv),
							tB = tex2D(_TexB, i.uv2);
					fixed3 sum = lerp(tA.rgb * tA.a*_Tint.a, tB.rgb * tB.a*_Tint.a, _Fade);
					fixed alpha = lerp(tA.a*_Tint.a, tB.a*_Tint.a, _Fade);
					return fixed4(sum, alpha);
				}
			ENDCG
		}
	}
	
	SubShader {Pass {
		Blend One OneMinusSrcAlpha
		BindChannels {
			Bind "vertex", vertex
			Bind "texcoord", texcoord0
			Bind "texcoord1", texcoord1
			Bind "texcoord1", texcoord2
			Bind "texcoord", texcoord3
		}
		SetTexture[_MainTex] {Combine texture * texture alpha}
		SetTexture[_TexB] {
			ConstantColor ([_Fade],[_Fade],[_Fade], [_Fade])
			Combine previous * one - constant, texture * constant
		}
		SetTexture[_TexB] {Combine texture * previous alpha + previous, previous}
		SetTexture[_MainTex] {
			ConstantColor (0,0,0, [_Fade])
			Combine previous, texture * one - constant + previous
		}
	}}
	
	SubShader {
		Pass {
			Blend One OneMinusSrcAlpha
			BindChannels {
				Bind "vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
			SetTexture[_MainTex] {Combine texture * texture alpha, texture}
			SetTexture[_TexB] {
				ConstantColor ([_Fade],[_Fade],[_Fade], [_Fade])
				Combine previous * one - constant, texture Lerp(constant) previous
			}
		}
		Pass {
			Blend SrcAlpha One
			BindChannels {
				Bind "vertex", vertex
				Bind "texcoord1", texcoord
			}
			SetTexture[_TexB] {
				ConstantColor ([_Fade], [_Fade], [_Fade])
				Combine texture * constant, texture
			}
		}
	}
	
	SubShader {	
		Pass {
			ColorMask A
			SetTexture[_MainTex] {
				ConstantColor (0,0,0, [_Fade])
				Combine texture * one - constant
			}
		}
		Pass {
			ColorMask A
			Blend One One
			BindChannels {
				Bind "vertex", vertex
				Bind "texcoord1", texcoord
			}
			SetTexture[_TexB] {
				ConstantColor (0,0,0, [_Fade])
				Combine texture * constant
			}
		}
		Pass {
			Blend SrcAlpha OneMinusDstAlpha
			SetTexture[_MainTex] {
				ConstantColor ([_Fade],[_Fade],[_Fade])
				Combine texture * one - constant, texture
			}
		}
		Pass {
			Blend SrcAlpha One
			BindChannels {
				Bind "vertex", vertex
				Bind "texcoord1", texcoord
			}
			SetTexture[_TexB] {
				ConstantColor ([_Fade],[_Fade],[_Fade])
				Combine texture * constant, texture
			}
		}
	}
}

}
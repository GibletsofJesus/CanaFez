Shader "Custom/wireframe" {

	Properties 
	{
		_SubBaseTex("Sub Base Texture", 2D) = "white" {}
		_SubTint("Sub Texture Tint Color", Color) = (1, 1, 1, 1)
		
		_BaseTex("Base Texture", 2D) = "white" {}
		_Tint("Base Texture Tint Color", Color) = (1, 1, 1, 1)
		
		_MainTex("WF Texture", 2D) = "white" {}
		_Color ("WF Color", Color) = (1,1,1,1)
		_Thickness ("WF Thickness", range(0,10)) = 1
		_ClippingOnOff("Clipping on/off", range(0,1)) = 0
		_Clipping ("Clipping", range(0,10)) = 1		
		_Clipping2 ("Clipping 2", range(0,10)) = 1
		_Clipping3 ("Clipping 3", range(0,100)) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma surface surf Lambert alpha

				sampler2D _SubBaseTex;
				fixed4 _SubTint;

				struct Input {
					float2 uv_SubBaseTex;
					float3 worldPos;
					fixed4 subTint;
				};

				void surf(Input IN, inout SurfaceOutput o)
				{
					fixed4 c = tex2D(_SubBaseTex, IN.uv_SubBaseTex);
					o.Albedo = c.rgb*_SubTint*c.a;
					o.Alpha = c.a *_SubTint.a;
				}
		
			ENDCG
			
			CGPROGRAM
				#pragma surface surf Lambert alpha

				sampler2D _BaseTex;
				fixed4 _Color, _Tint;
				float _Alpha;//, _SubClipping, _SubClipping2, _SubClipping3;
				float _ClippingOnOff;
				float _Clipping = 1;
				float _Clipping2 = 1;
				float _Clipping3 = 1;

				struct Input {
					float2 uv_BaseTex;
					float3 worldPos;
				};

				void surf(Input IN, inout SurfaceOutput o) 
				{
					if (_ClippingOnOff > 0.5f)
						clip(frac((IN.worldPos.y + IN.worldPos.z*_Clipping + _Clipping3) * _Clipping2) - 0.5);
					fixed4 c = tex2D(_BaseTex, IN.uv_BaseTex);
					o.Albedo = c.rgb *_Tint*c.a;
					//o.Alpha = _Alpha * c.a;
					o.Alpha = c.a* _Tint.a;
				}

		
			ENDCG

		Pass
		{
			Tags { "RenderType"="Transparent" "Queue"="Transparent" }
			
			Blend SrcAlpha OneMinusSrcAlpha //Alpha blending 
			LOD 200
			CGPROGRAM
				#pragma target 5.0
				#include "UnityCG.cginc"
				#include "wireframeFunctions.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom

				// Vertex Shader
				wf_v2g vert(appdata_base v)
				{
					//v.color.a = 0.2f;
					return wf_vert(v);
				}
				
				// Geometry Shader
				[maxvertexcount(3)]
				void geom(triangle wf_v2g p[3], inout TriangleStream<wf_g2f> triStream)
				{
					wf_geom(p, triStream);
				}
				
				// Fragment Shader
				float4 frag(wf_g2f input) : COLOR
				{	
					//return wf_frag(input);
					float4 col = wf_frag(input);				
					if( col.a < 0.5f ) discard;
					else col.a = 1.0f;
					
					return col;
				}
			
			ENDCG
		}
	}
} 

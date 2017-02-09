Shader "Custom Shaders/AlphaUnlitSlice" {
	Properties{
		_MainTex("SelfIllum Color (RGB) Alpha (A)", 2D) = "white"
		_BurnTex("Burning texture",2D) = "white"{}
		_SliceGuide("_SliceGuide (RGB)", 2D) = "white" {}
	_SliceAmount("Slice Amount", Range(0.0, 1.0)) = 0.5
		_BurnAmount("Burn Amount", Range(0.0, 1.0)) = 0.5
		_TintColour("Tint Colour",color) = (1,1,1,1)
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Lighting Off
		ZWrite On
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		//#pragma surface surf Lambert  //Instead of this line add the next 8
#pragma surface surf NoLighting 
		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
	{
		fixed4 c;
		c.rgb = s.Albedo;
		c.a = s.Alpha;
		return c;
	}
	struct Input {
		float2 uv_MainTex;
		float2 uv_BurnTex;
		float2 uv_SliceGuide;
		float _SliceAmount;
	};
	sampler2D _MainTex,_BurnTex;
	sampler2D _SliceGuide;
	float _SliceAmount,_BurnAmount;
	fixed4 _TintColour;
	void surf(Input IN, inout SurfaceOutput o) {
		clip(tex2D(_SliceGuide, IN.uv_SliceGuide).rgb - _SliceAmount);
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb*_TintColour.rgb - ((tex2D(_BurnTex, IN.uv_BurnTex)*_BurnAmount)*c.rgb);
		o.Alpha =_TintColour.a;
	}

	ENDCG
	}
		Fallback "Diffuse"
}
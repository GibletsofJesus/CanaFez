Shader "Unlit/ripplingWater"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint Color",Color)=(1,1,1,1)
		_WaveAmp("Wave amplitude",float)=1
		_Wave1("Wave speed",float)=1
		_Wave2("Wave frequency", Range(0,5)) = 1
		_Wave3("Wave rotation offset", Range(0,10)) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
	fixed4 _Tint;
			float _WaveAmp,_Wave1,_Wave2,_Wave3;
			v2f vert (appdata v)
			{
				v2f o;
   				float phase = _Time * _Wave1;
   				float offset = ((v.vertex.x +v.vertex.z)+ (v.vertex.z * _Wave3)) * (_Wave2/10);
  				v.vertex.y = sin(phase + offset) *_WaveAmp;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv)/ _Tint;

				return col;
			}
			ENDCG
		}
	}
}

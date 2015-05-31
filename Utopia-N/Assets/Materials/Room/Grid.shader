Shader "Custom/RoomGrid"
{
	Properties
	{
		_GridColor ("Color", Color) = (1,1,1,1)
		_GridTex ("Grid Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _GridTex;

		struct Input
		{
			float2 uv_GridTex;
		};
		
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			// Albedo comes from a texture tinted by color
//			fixed4 c = tex2D (_GridTex, IN.uv_GridTex) * _GridColor;
		//	o.Albedo = c.rgb;
		//	o.Alpha = c.a;
		}
		ENDCG
	}
}

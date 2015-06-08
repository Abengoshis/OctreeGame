Shader "Custom/Skybox"
{
	Properties
	{
		_SkyCube ("Environment Map", Cube) = "" {}
		_GridColor ("Grid Color", Color) = (1,1,1,1)
		_GridTex ("Grid (RGB)", 2D) = "white" {}
		_GridScale ("Grid Scale", Float) = 1
		_GridRevealDistance ("Grid Reveal Distance", Float) = 10
		_NoiseTex ("Noise (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Background" }
		
		Cull Off
		ZWrite Off
		//ZTest Always
		Lighting Off
		Fog{Mode Off}
		
		CGPROGRAM
		#pragma surface surf Unlit
		#pragma target 3.0
		
		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
		{
        	half4 c;
        	c.rgb = s.Albedo;
        	c.a = s.Alpha;
        	return c;
		}
		
		samplerCUBE _SkyCube;
		sampler2D _GridTex;
		fixed4 _GridColor;
		half _GridScale;
		half _GridRevealDistance;

		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float3 viewDir;
		};
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half3 plane = half3(IN.worldPos.x * (1 - abs(IN.worldNormal.x)),
								IN.worldPos.y * (1 - abs(IN.worldNormal.y)),
								IN.worldPos.z * (1 - abs(IN.worldNormal.z)));
			
			float2 uvFixed;
			uvFixed.x = plane.x != 0 ? plane.x : plane.z;	// Only one component is 0, meaning if x is 0 z and y must not be 0.
			uvFixed.y = plane.y != 0 ? plane.y : plane.z;	// If y is 0, x must not be 0 so x was assigned before.
			uvFixed /= _GridScale;
			
			fixed4 colSky = texCUBE(_SkyCube, IN.viewDir);
			fixed4 colGrid = tex2D(_GridTex, uvFixed);
			colGrid = colGrid * colGrid + pow(_GridColor * colGrid.a, 4);
			
			half3 vecToCam = _WorldSpaceCameraPos - IN.worldPos;
			half distToCam = vecToCam.x * vecToCam.x + vecToCam.y * vecToCam.y + vecToCam.z * vecToCam.z;
			distToCam = max(distToCam, 100.0);
			
			o.Albedo = colSky + colGrid / (distToCam / _GridRevealDistance);
			o.Alpha = 0;
		}
		ENDCG
	}
}

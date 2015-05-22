Shader "Custom/Tech"
{
	Properties
	{
		_DotColor ("Dot Colour", Color) = (1,1,1,1)
		_FirstColor ("First Colour", Color) = (1,1,1,1)
		_SecondColor ("Second Colour", Color) = (1,1,1,1)
		_DotSpeed ("Dot Speed", Float) = 0
		_FirstSpeed ("First Speed", Float) = 0
		_SecondSpeed ("Second Speed", Float) = 0
		
		_DotSize ("Dot Size", Range(0.0, 1.0)) = 0.1
		_DotCoverage ("Dot Coverage", int) = 128
		_LineSharpness ("Line Sharpness", Float) = 1024
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		
		

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			fixed4 _DotColor;
			fixed4 _FirstColor;
			fixed4 _SecondColor;
			half _DotSpeed;
			half _FirstSpeed;
			half _SecondSpeed;
			
			half _DotSize;
			int _DotCoverage;
			half _LineSharpness;
			sampler2D _NoiseTex;
			
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 worldPosition : SV_POSITION;
				float3 localPosition : float3;
			};
			
			v2f vert(appdata_base v)
			{
				v2f o;
				o.localPosition = v.vertex.xyz;
				o.worldPosition = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				return o;
			}

			struct Input
			{
				float2 uv_MainTex;
				float3 viewDir;
			};
		
			fixed4 getColour(float2 uvFirst, float2 uvSecond)
			{
				fixed4 first = tex2D(_NoiseTex, uvFirst) * _FirstColor;
				fixed4 second = tex2D(_NoiseTex, uvSecond) * _SecondColor;
				
				return first || second;
			}
			
			fixed4 frag (v2f f) : COLOR
			{
				float2 uvFirst = f.uv + float2(_Time.y + sin(_Time.x * 1.5 + 0.5) * 5, (_Time.y + sin(_Time.x * 5 + 0.5) * 7) * 0.5) * _FirstSpeed * 0.01;
				float2 uvSecond = f.uv - float2(_Time.y, (_Time.y + sin(_Time.x * 3) * 10) * 0.5) * _SecondSpeed * 0.01;
		
				fixed4 c = fixed4(0,0,0,1);
				
				// -------- Edge Detection. --------
				
				// If this pixel is not empty, it cannot be an edge.			// Instead of empty, could do colour threshold.
				fixed4 cCurrent = getColour(uvFirst, uvSecond);
				if (cCurrent.a == 1)
				{
					// If this pixel is empty and any of the surrounding pixels are not, it is an edge.
					float px = 1.0 / _LineSharpness;
					bool edge = false;
					for (int x = -1; x <= 1 && edge == false; ++x)
					{
						for (int y = -1; y <= 1 && edge == false; ++y)
						{
							if (x == 0 && y == 0) continue;	// Skip the middle pixel.
							
							// Check for non-red pixels.
							if (getColour(uvFirst + float2(x * px, y * px), uvSecond + float2(x * px, y * px)).a != 1)
							{
								// Edge has been found, loops will end early.
								edge = true;
								
								c += cCurrent;							
							}
						}
					}				
				}
				
				// Create dots outside the shapes.
				if (cCurrent.a != 1)
				{
					if (frac(f.uv.x * _DotCoverage - _Time.y * _DotSpeed) < _DotSize && frac(f.uv.y * _DotCoverage * 0.5) < _DotSize)
					{
						c += _DotColor;
					}
				}
				
				return c;
			}
			
			ENDCG
		}
	} 
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Simon/Test" 
{

	Properties
	{
		_Color("Color Tint", Color) = ( 1,1,1,1 )
		_FirstShadowMultColor("FirstShadowMultColor Tint", Color) = (0.5,0.5,0.5,1)
		_SecondShadowMultColor("SecondShadowMultColor Tint", Color) = (0.5,0.5,0.5,1)
		_Specular("Specular Tint", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_LightMap("Light Map Texture", 2D) = "gray" {}
		_Gloss("Gloss", Range(8.0, 256)) = 20

		_LightArea("LightArea", Range(0, 1)) = 0.5

		_SecondShadow("SecondShadow", Range(0, 1)) = 0.5

		_Shininess("Shininess", Range(0, 1)) = 0.5
	}
	SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM

			fixed4 _Color;
			fixed4 _FirstShadowMultColor;
			fixed4 _SecondShadowMultColor;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _LightMap;
			float4 _LightMap_ST;
			fixed4 _Specular;
			float _Gloss;
			float _LightArea;
			float _SecondShadow;
			float _Shininess;

			#pragma vertex vert  
			#pragma fragment frag  

# include "Lighting.cginc"

			struct v2f {
				float4 pos : POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
				float4 color : TEXCOORD3;
				float halfLambert : TEXCOORD4;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;

				o.halfLambert = 0.5 * dot(normalize(o.worldNormal), normalize(UnityWorldSpaceLightDir(o.worldPos))) + 0.5;

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				fixed3 mainCol;

				// 第一层阴影
				fixed3 lightMapColor = normalize(tex2D(_LightMap, i.uv));
				fixed mask = lightMapColor.y * i.color.y;
				mask += saturate(i.halfLambert);
				mask = mask * 0.5 + (-_LightArea) + 1;
				int lightStep = step(1, mask);
				mainCol.xyz = tex2D(_MainTex, i.uv).rgb;
				fixed3 firstShadow = mainCol.xyz * _FirstShadowMultColor.rgb;
				if (lightStep != 0)
					firstShadow = mainCol.xyz;
				else
					firstShadow = firstShadow;
				
				// 第二层阴影
				fixed3 secondShadow = mainCol.xyz * _SecondShadowMultColor.rgb;
				fixed secMask = i.color.y * lightMapColor.y + saturate(i.halfLambert);
				secMask = secMask * 0.5 + (-_SecondShadow) + 1;
				lightStep = step(1, secMask);
				
				if (lightStep != 0)
					secondShadow = mainCol.xyz;
				else
					secondShadow = secondShadow;

				fixed sep = i.color.y * lightMapColor.y + 0.9;
				int sepMask = step(1, sep);
				fixed3 finalColor;
				if (sepMask != 0)
					finalColor = firstShadow;
				else
					finalColor = secondShadow;

				// 高光
				float3 viewDir = -i.worldPos + _WorldSpaceCameraPos.xyz;
				viewDir = normalize(viewDir);
				float3 halfView = viewDir + normalize(_WorldSpaceLightPos0.xyz);
				halfView = normalize(halfView);
				float shinPow = pow(max(dot(normalize(i.worldNormal.xyz), halfView), 0), _Shininess);
				float oneMinusSpec = 1 - lightMapColor.z;
				oneMinusSpec = oneMinusSpec - shinPow;
				int specMaslk = step(0, oneMinusSpec);
				fixed3 specColor = 0.5 * _Specular.xyz;
				specColor = lightMapColor.x * specColor;
				if (specMaslk != 0)
					specColor = 0;
				else
					specColor = specColor;

				return fixed4(finalColor + specColor, 1.0);
			}
			ENDCG
		}

	}
	FallBack "Diffuse"
}


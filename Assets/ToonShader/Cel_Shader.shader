// Shader by VFX_LYP

Shader "LYP_FX/Cel_Shader" {
    Properties {
        _BaseTex ("BaseTex", 2D) = "white" {}
        [HDR]_BaseTexColor ("BaseTexColor", Color) = (1,1,1,1)
        _ILMTex ("ILMTex", 2D) = "white" {}
        _ShadowRange ("ShadowRange", Range(0, 1)) = 1
        _ShadowPower ("ShadowPower", Range(0, 1)) = 0.8
        _SpecularRange ("SpecularRange", Range(0.5, 1)) = 0.998
        _SpecularPower ("SpecularPower", Range(0, 1)) = 0.88
        [HDR]_OutlineColor ("OutlineColor", Color) = (0.5019608,0.5019608,0.5019608,1)
        _OutlineWidth ("OutlineWidth", Range(0, 0.05)) = 0.0001
        _EmissionTex ("EmissionTex", 2D) = "white" {}
        [HDR]_EmissionColor ("EmissionColor", Color) = (0,0,0,0)
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "RenderType"="Opaque"
        }
        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _BaseTex; uniform float4 _BaseTex_ST;
            uniform float _OutlineWidth;
            uniform float4 _OutlineColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( float4(v.vertex.xyz + v.normal*_OutlineWidth,1) );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 lightColor = _LightColor0.rgb;
                float4 _BaseTex_var = tex2D(_BaseTex,TRANSFORM_TEX(i.uv0, _BaseTex));
                return fixed4(((_BaseTex_var.rgb*_OutlineColor.rgb)*_LightColor0.rgb),0);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform float _ShadowRange;
            uniform float _SpecularRange;
            uniform sampler2D _BaseTex; uniform float4 _BaseTex_ST;
            uniform float _ShadowPower;
            uniform float _SpecularPower;
            uniform sampler2D _ILMTex; uniform float4 _ILMTex_ST;
            uniform float4 _BaseTexColor;
            uniform sampler2D _EmissionTex; uniform float4 _EmissionTex_ST;
            uniform float4 _EmissionColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
////// Lighting:
                float attenuation = 1;
////// Emissive:
                float4 _EmissionTex_var = tex2D(_EmissionTex,TRANSFORM_TEX(i.uv0, _EmissionTex));
                float3 emissive = (_EmissionColor.rgb*_EmissionColor.a*_EmissionTex_var.rgb*_EmissionTex_var.a);
                float4 _node_9643_copy_copy_copy = tex2D(_ILMTex,TRANSFORM_TEX(i.uv0, _ILMTex));
                float4 _BaseTex_var = tex2D(_BaseTex,TRANSFORM_TEX(i.uv0, _BaseTex));
                float3 node_1398 = saturate(( (_BaseTex_var.rgb*_BaseTexColor.rgb) > 0.5 ? (1.0-(1.0-2.0*((_BaseTex_var.rgb*_BaseTexColor.rgb)-0.5))*(1.0-_node_9643_copy_copy_copy.g)) : (2.0*(_BaseTex_var.rgb*_BaseTexColor.rgb)*_node_9643_copy_copy_copy.g) ));
                float4 _node_9643_copy_copy = tex2D(_ILMTex,TRANSFORM_TEX(i.uv0, _ILMTex));
                float node_3046_if_leA = step((max(0,dot(lightDirection,normalDirection))-(0.5-_node_9643_copy_copy.g)),_ShadowRange);
                float node_3046_if_leB = step(_ShadowRange,(max(0,dot(lightDirection,normalDirection))-(0.5-_node_9643_copy_copy.g)));
                float node_1346 = 0.0;
                float DiffuseIf = lerp((node_3046_if_leA*node_1346)+(node_3046_if_leB*1.0),node_1346,node_3046_if_leA*node_3046_if_leB);
                float3 Diffuse = (node_1398*DiffuseIf);
                float4 _node_9643_copy = tex2D(_ILMTex,TRANSFORM_TEX(i.uv0, _ILMTex));
                float node_526_if_leA = step(saturate(pow(max(0,dot(viewDirection,normalDirection)),_node_9643_copy.r)),_SpecularRange);
                float node_526_if_leB = step(_SpecularRange,saturate(pow(max(0,dot(viewDirection,normalDirection)),_node_9643_copy.r)));
                float node_4786 = 0.0;
                float node_8645_if_leA = step((lerp((node_526_if_leA*node_4786)+(node_526_if_leB*1.0),node_4786,node_526_if_leA*node_526_if_leB)*_node_9643_copy.b),0.1);
                float node_8645_if_leB = step(0.1,(lerp((node_526_if_leA*node_4786)+(node_526_if_leB*1.0),node_4786,node_526_if_leA*node_526_if_leB)*_node_9643_copy.b));
                float node_10 = 0.0;
                float3 finalColor = emissive + (((Diffuse+((node_1398*_ShadowPower)*(1.0 - DiffuseIf)))+(DiffuseIf*((Diffuse*_SpecularPower)*lerp((node_8645_if_leA*node_10)+(node_8645_if_leB*1.0),node_10,node_8645_if_leA*node_8645_if_leB))))*_LightColor0.rgb*attenuation);
                return fixed4(finalColor,1);
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _EmissionTex; uniform float4 _EmissionTex_ST;
            uniform float4 _EmissionColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i) : SV_Target {
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                float4 _EmissionTex_var = tex2D(_EmissionTex,TRANSFORM_TEX(i.uv0, _EmissionTex));
                o.Emission = (_EmissionColor.rgb*_EmissionColor.a*_EmissionTex_var.rgb*_EmissionTex_var.a);
                
                float3 diffColor = float3(0,0,0);
                o.Albedo = diffColor;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Legacy Shaders/Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}

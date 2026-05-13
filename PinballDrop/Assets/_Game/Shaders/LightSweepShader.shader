Shader "Vanta/UI/LightSweep"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
        _Value ("Value", Range(0.0, 1.0)) = 0.0
        _Brightness ("Brightness", Range(0.0, 1.0)) = 0.0
        [Enum(Line,0,Radial,1)] _Style ("Style", Int) = 0
        [Enum(Yes,1,No,0)] _Toon ("Toon", Int) = 0
        _ToonGlow ("Toon Glow", Range(0.0, 1.0)) = 0.0
		_Speed("Speed", Range(0.0, 1.0)) = 0.0
        
	}
	SubShader
	{
		Tags {
                "Queue"="Transparent" 
                "IgnoreProjector"="True" 
                "RenderType"="TransparentCutout"
        }
        LOD 100
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

		Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
        		float2 texcoord2 : TEXCOORD1; 
            };
 
            struct v2f {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            	float2 texcoord2 : TEXCOORD1;
            };
 
            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            
            uniform float4 _MainTex_ST;
            
            uniform float _Value;
            uniform float _Brightness;
            uniform int _Style;
            uniform int _Toon;
            uniform float _ToonGlow;
			uniform float _Speed;
            
            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
            	o.texcoord2 = TRANSFORM_TEX(v.texcoord2, _MainTex);
                
                return o;
            }
            
            fixed4 frag (v2f i) : COLOR {
                
				_Value = fmod(_Time.y * _Speed, 1.0);

                fixed2 r = i.texcoord;
                float amount = _Value * 6.3;
                amount -= 3.;
                
                float col = sin(r.y + r.x*3.1415 - amount);
                if (_Style == 1) {
                    col = sin(sin(r.y*3.1415) + cos(r.x*3.1415) + amount);
                }
                
                col *= col * col * 0.6;
                col = clamp(col, 0., 1.);
   
                if (_Toon == 1) {
                    col = col - fmod(col, 0.5) * (1.0 - _ToonGlow);
                    col *= _Brightness * 0.75;
                } else {
                    col -= 1.0 - _Brightness;
                    col = clamp(col, 0., 1.);
                }
                
                float4 tex = _MainTex.Sample(sampler_MainTex, r) * i.color;
				if (tex.a > 0.5) {
					tex = tex + fixed4(col, col, col, col);
				}
				return tex;;
    
    
    
    //fragColor = tex + vec4(col);
            
            
            
            }
        ENDCG
        }
	}
}

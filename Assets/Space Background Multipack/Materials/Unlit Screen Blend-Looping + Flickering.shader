// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ScreenBlend (Looping + Flickering)" {
Properties {
   _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
   _Color ("Color", Color) = (1,1,1,1)
   _FlickerTex ("Flicker Texture", 2D) = "white" {}
   [MaterialToggle] _isFlickering("Flickering", Float) = 0
}
 
SubShader {
   Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
   LOD 100
 
   BlendOp Add
   Blend OneMinusDstColor One, One Zero // screen
   //Blend SrcAlpha One, One Zero // linear dodge
   ZWrite Off
   AlphaTest Greater .01
     
   Pass {
     CGPROGRAM
       #pragma vertex vert
       #pragma fragment frag
     
       #include "UnityCG.cginc"
 
       struct appdata_t {
         float4 vertex : POSITION;
         float2 texcoord : TEXCOORD0;
         float2 texcoord2 : TEXCOORD1;
       };
 
       struct v2f {
         float4 vertex : SV_POSITION;
         half2 texcoord : TEXCOORD0;
         half2 texcoord2 : TEXCOORD1;
       };
 
       sampler2D _MainTex;
       sampler2D _FlickerTex;
	   float4 _MainTex_ST;
	   float4 _FlickerTex_ST;
       float4 _Color;
       float _isFlickering;
     
       v2f vert (appdata_t v)
       {
         v2f o;
         o.vertex = UnityObjectToClipPos(v.vertex);
         o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
         o.texcoord2 = TRANSFORM_TEX(v.texcoord, _FlickerTex);
         return o;
       }
     
       fixed4 frag (v2f i) : COLOR
       {
       		i.texcoord.x = fmod(i.texcoord.x, 1);
			i.texcoord.y = fmod(i.texcoord.y, 1);
       
         	fixed4 col = _Color*tex2D(_MainTex, i.texcoord);
         
         	if(_isFlickering > 0)
         	{
	        	i.texcoord2.x = fmod(i.texcoord.x + i.texcoord2.x, 1);
				i.texcoord2.y = fmod(i.texcoord.y + i.texcoord2.y, 1);
	         
	         	fixed4 flicker = _Color*tex2D(_FlickerTex, i.texcoord2);
	         	flicker.rgb*= flicker.a;
	         
	         
	         	if(col.r > 0.025 && flicker.r > 0.025)
					col = 1.0 - (1.0 - col) * (1.0 - flicker);
            }
                  
         	col.rgb *= col.a;
         	return col;
       }
     ENDCG
   }
}
}
//
// If your looking for example code to make your own shaders, I suggest looking at the included examples. 
// This shader uses lots of compiler/C Macro abuse which is going to make it much harder to understand
// than the included examples. 

Shader "VertexPainter/SplatBlend_1Layer" 
{
   Properties {
      _Tex1 ("Albedo + Height", 2D) = "white" {}
      _Tint1 ("Tint", Color) = (1, 1, 1, 1)
      [NoScaleOffset][Normal]_Normal1("Normal", 2D) = "bump" {}
      _Glossiness1 ("Smoothness", Range(0,1)) = 0.5
      [NoScaleOffset]_GlossinessTex1("Metallic(R)/Smoothness(A)", 2D) = "black" {}
      _Metallic1 ("Metallic", Range(0,1)) = 0.0
      _Emissive1  ("Emissive", 2D) = "black" {} 
      _EmissiveMult1("Emissive Multiplier", Float) = 1
      _Parallax1 ("Parallax Height", Range (0.005, 0.08)) = 0.02
      _TexScale1 ("Texture Scale", Float) = 1
      
      
      _FlowSpeed ("Flow Speed", Float) = 0
      _FlowIntensity ("Flow Intensity", Float) = 1
   }
   SubShader {
      Tags { "RenderType"="Opaque" }
      LOD 200
      
      CGPROGRAM
      
      // these are done with shader compile options - but honestly, on most modern hardware,
      // doing a branch would be fine since the branch would always go the same direction on
      // each pixel. If you are running low on keywords, that could be a viable option for you.
      #pragma surface surf Standard vertex:vert fullforwardshadows
      #pragma shader_feature __ _PARALLAXMAP
      #pragma shader_feature __ _NORMALMAP
      #pragma shader_feature __ _METALLICGLOSSMAP
      #pragma shader_feature __ _EMISSION
      // flow map keywords. 
      #pragma shader_feature __ _FLOW1
      #pragma shader_feature __ _FLOWDRIFT 

      #include "SplatBlend_Shared.cginc"
      
      void vert (inout appdata_full v, out Input o) 
      {
          SharedVert(v,o);
      }
      
      void surf (Input IN, inout SurfaceOutputStandard o) 
      {
         float2 uv1 = IN.uv_Tex1 * _TexScale1;
         INIT_FLOW
         #if (_FLOWDRIFT || !_PARALLAXMAP)
         fixed4 c1 = FETCH_TEX1(_Tex1, uv1);
         #else
         fixed4 c1 = tex2D(_Tex1, uv1);
         #endif
         
         #if _PARALLAXMAP
         float parallax = _Parallax1;
         float2 offset = ParallaxOffset (c1.a, parallax, IN.viewDir);
         uv1 += offset;
         #if (_FLOW1 || _FLOW2 || _FLOW3 || _FLOW4 || _FLOW5)
         fuv1 += offset;
         fuv2 += offset;
         #endif
         c1 = FETCH_TEX1(_Tex1, uv1);
         #endif

         c1 *= _Tint1;

         #if _METALLICGLOSSMAP
         fixed4 g1 = FETCH_TEX1(_GlossinessTex1, uv1);
         o.Smoothness = g1.a;
         o.Metallic = g1.r;
         #else
         o.Smoothness = _Glossiness1;
         o.Metallic = _Metallic1;
         #endif
         
         
         #if _EMISSION
         fixed4 e1 = FETCH_TEX1(_Emissive1, uv1);
         o.Emission = e1.rgb * _EmissiveMult1;
         #endif
         
         #if _NORMALMAP
         fixed4 n1 = FETCH_TEX1(_Normal1, uv1);
         o.Normal = UnpackNormal(n1);
         #endif
         
         o.Albedo = c1.rgb;
         
         
      }
      ENDCG
   } 
   CustomEditor "SplatMapShaderGUI"
   FallBack "Diffuse"
}
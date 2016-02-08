// Flow Map Example

Shader "Custom/FlowMap" 
{
   Properties 
   {
      _MainTex ("Albedo (RGB)", 2D) = "white" {}
      _NormalTex("Normal", 2D) = "bump" {}
      _Glossiness ("Smoothness", Range(0,1)) = 0.5
      _Metallic ("Metallic", Range(0,1)) = 0.0
      _FlowSpeed ("Flow Speed", Float) = 1
      _FlowIntensity ("Flow Intensity", Float) = 1
      _NoiseScale ("Noise Scale", Range(0, 0.2)) = 0.1
   }
   SubShader 
   {
      Tags { "RenderType"="Opaque" }
      LOD 200
		
      CGPROGRAM
      // Physically based Standard lighting model, and enable shadows on all light types
      #pragma surface surf Standard fullforwardshadows

      sampler2D _MainTex;
      sampler2D _NormalTex;

      struct Input {
         float2 uv_MainTex;
         fixed4 color : Color;
      };

      half _Glossiness;
      half _Metallic;
      float _FlowSpeed;
      float _FlowIntensity;
      float _NoiseScale;

      // The flow function takes a UV coordinate and produces two resulting UV coordinates and an interpolation value
      // between them. You pass in a 0-1 flow vector, stored in a texture or vertex, as well as speed and intensity factors. 
      // Finally, a noise value (usually computed per pixel) can be used to offset the time such that each pixel moves at
      // a slightly different rate. Note that if you don't need the per-pixel noise, you can compute this function entirely
      // in a vertex shader making it very cheap.
      void Flow(float2 uv, float2 flow, float speed, float intensity, float noise, out float2 uv1, out float2 uv2, out float interp)
      {
         float2 flowVector = (flow * 2.0 - 1.0) * intensity;
         
         float timeScale = _Time.y * speed + noise;
         float2 phase;
         
         phase.x = frac(timeScale);
         phase.y = frac(timeScale + 0.5);
  
         uv1 = (uv - flowVector * half2(phase.x, phase.x));
         uv2 = (uv - flowVector * half2(phase.y, phase.y));
         
         interp = abs(0.5 - phase.x) / 0.5;
      }

      void surf (Input IN, inout SurfaceOutputStandard o) 
      {
         float2 uv1;
         float2 uv2;
         float interp;
         // for this example, we're just using the normal map's r channel to provide noise. You can use a custom texture if you
         // prefer, or pack it into an unused channel.   
         float noise = tex2D(_NormalTex, IN.uv_MainTex).r * _NoiseScale;
         
         // generate our UV and interpolation values
         Flow(IN.uv_MainTex, IN.color.rg, _FlowSpeed, _FlowIntensity, noise, uv1, uv2, interp);
         
         // sample all the things..
			fixed4 c1 = tex2D (_MainTex, uv1);
         fixed4 c2 = tex2D (_MainTex, uv2);
         fixed4 n1 = tex2D (_NormalTex, uv1);
         fixed4 n2 = tex2D (_NormalTex, uv2);
         
         // blend the final color and normal values
         fixed4 cf = lerp(c1, c2, interp);
         half3 norm = UnpackNormal(lerp(n1, n2, interp));
         
	 o.Albedo = cf.rgb;
	 o.Metallic = _Metallic;
	 o.Smoothness = _Glossiness;
         o.Emission = cf.rgb;
         o.Normal = norm;
	 o.Alpha = cf.a;
      }
      ENDCG
   } 
   FallBack "Diffuse"
}

Shader "Hidden/VertexPainterLite_Preview"
{
   Properties
   {

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

         struct appdata
         {
            float4 vertex : POSITION;
            float4 color  : COLOR;
            float2 uv0    : TEXCOORD0;
            float2 uv1    : TEXCOORD1;
            float2 uv2    : TEXCOORD2;
            float2 uv3    : TEXCOORD3;
         };

         struct v2f
         {
            float4 vertex : SV_POSITION;
            float4 color  : COLOR0;
            float2 uv0    : TEXCOORD0;
            float2 uv1    : TEXCOORD1;
            float2 uv2    : TEXCOORD2;
            float2 uv3    : TEXCOORD3;
         };

         float  _channel;
         float2 _uvRange;

         v2f vert (appdata v)
         {
            v2f o;
            o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); 
            o.color = v.color;
            o.uv0 = v.uv0;
            o.uv1 = v.uv1;
            o.uv2 = v.uv2;
            o.uv3 = v.uv3;
            return o;
         }

         float Range(float f)
         {
            f -= _uvRange.x;
            f /= max(0.0001f, _uvRange.y);
            return f;
         }

         fixed4 frag (v2f i) : SV_Target
         {
            if (_channel < 1)
            {					
               return i.color;
            }
            if (_channel < 2)
            {
            	return fixed4(i.color.r, 0, 0, 1);
            }
            if (_channel < 3)
            {
            	return fixed4(0, i.color.g, 0, 1);
            }
            if (_channel < 4)
            {
            	return fixed4(0, 0, i.color.b, 1);
            }
            if (_channel < 5)
            {
            	return fixed4(i.color.a, i.color.a, i.color.a, 1);
            }
            if (_channel < 6)
            {
               float f = Range(i.uv0.x); return fixed4(f, f, f, 1);
            }
            if (_channel < 7)
            {
               float f = Range(i.uv0.y); return fixed4(f, f, f, 1);
            }
            if (_channel < 8)
            {
               float f = Range(i.uv1.x); return fixed4(f, f, f, 1);
            }
            if (_channel < 9)
            {
               float f = Range(i.uv1.y); return fixed4(f, f, f, 1);
            }
            if (_channel < 10)
            {
               float f = Range(i.uv2.x); return fixed4(f, f, f, 1);
            }
            if (_channel < 11)
            {
               float f = Range(i.uv2.y); return fixed4(f, f, f, 1);
            }
            if (_channel < 12)
            {
               float f = Range(i.uv3.x); return fixed4(f, f, f, 1);
            }
            if (_channel < 13)
            {
               float f = Range(i.uv3.y); return fixed4(f, f, f, 1);
            }

            return i.color;
         }
         ENDCG
      }
   }
}

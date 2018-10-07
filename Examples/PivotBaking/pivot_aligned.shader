// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Sub-Optimal shader example that rotates objects around the local space pivot of the original object. Pivot position and
// rotation stored in UV2/UV3 respectively.


Shader "Unlit/pivot_aligned"
{
	Properties
   {
      
   }
   SubShader
   {
      Tags { "RenderType"="Opaque" }
      LOD 100

      Pass
      {
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag

         
         #include "UnityCG.cginc"

         struct appdata
         {
            float4 vertex : POSITION;
            float4 pivot : TEXCOORD2;
            float3 rotation : TEXCOORD3;
         };

         struct v2f
         {
            float4 vertex : SV_POSITION;
            float4 color : TEXCOORD0;
         };
         

         float4 RotateXYZ(float4 p, float3 rotate)
         {
            
            float3 c = cos(rotate);
            float3 s = sin(rotate);

            // make a matrix for each axis
            float4x4 rotateXMtx  = float4x4( 1, 0, 0, 0,
                                       0, c.x, -s.x,   0,
                                       0, s.x, c.x, 0,
                                       0, 0, 0, 1);


            float4x4 rotateYMtx  = float4x4( c.y, 0, s.y, 0,
                                       0, 1, 0, 0,
                                       -s.y, 0, c.y, 0,
                                       0, 0, 0, 1);


            float4x4 rotateZMtx  = float4x4( c.z, -s.z,   0, 0,
                                       s.z, c.z, 0, 0,
                                       0, 0, 1, 0,
                                       0, 0, 0, 1);

            float4 rx = mul(p, rotateXMtx);
            float4 rxy = mul(rx, rotateYMtx);
            float4 rxyz = mul(rxy, rotateZMtx);
            return rxyz;
         }

         v2f vert (appdata v)
         {
            v2f o;

            // total rotation to apply in radians
            float3 rotate = float3(_Time.y, 0, 0);

            // compute the vertex location as an offset from the pivot point
            float4 pivot = v.pivot;
            float4 offset = v.vertex - pivot;
            // rotate around each axis
            offset = RotateXYZ(offset, radians(v.rotation));
            float4 rxyz = RotateXYZ(offset, rotate);
            rxyz = RotateXYZ(rxyz, -radians(v.rotation));
            // take the final rotation, and add the pivot back in to get our final position, then transform into MVP space
            o.vertex = UnityObjectToClipPos(rxyz + pivot);
            // the baker puts a random number into the W channel. This is VERY useful, for instance, if you want
            // each object to rotate at a random speed or at a random angle, or for use in a random function as
            // a seed.
            o.color = v.pivot.wwww;
            return o;
         }
         
         fixed4 frag (v2f i) : SV_Target
         {
            return i.color;
         }
         ENDCG
      }
   }
}

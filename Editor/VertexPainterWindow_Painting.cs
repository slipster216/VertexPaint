using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow
   {

      // C# doesn't have *& or **, so it's not easy to pass a reference to a value for changing.
      // instead, we wrap the setter into a templated lambda which allows us to pass a changable
      // reference around via a function which sets it. Pretty tricky sis, but I'd rather just
      // be able to pass the freaking reference already..
      // Note the ref object, which is there just to prevent boxing of Vector/Color structs. Also
      // note the complete lack of type safety, etc.. ugh..

      // whats worse- this could also be condensed down to a macro, which would actually be MORE
      // safe in terms of potential bugs than all this; and it would be like a dozen lines to boot.
      delegate void Setter(int idx, ref object x);

      Setter GetSetter(VertexInstanceStream s)
      {
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  return delegate(int idx, ref object v)
                  {
                     Vector2 vv = (Vector2)v;
                     s.colors[idx].r = vv.x;
                     s.colors[idx].g = vv.y;
                  }; 
               case FlowTarget.ColorBA:
                  return delegate(int idx, ref object v)
                  {
                     Vector2 vv = (Vector2)v;
                     s.colors[idx].b = vv.x;
                     s.colors[idx].a = vv.y;
                  }; 
               case FlowTarget.UV0_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv0[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x = iv.x;
                     vec.y = iv.y;
                     s.uv0[idx] = vec;
                  }; 
               case FlowTarget.UV0_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv0[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z = iv.x;
                     vec.w = iv.y;
                     s.uv0[idx] = vec;
                  }; 
               case FlowTarget.UV1_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv1[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x = iv.x;
                     vec.y = iv.y;
                     s.uv1[idx] = vec;
                  }; 
               case FlowTarget.UV1_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv1[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z = iv.x;
                     vec.w = iv.y;
                     s.uv1[idx] = vec;
                  }; 
               case FlowTarget.UV2_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv2[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x = iv.x;
                     vec.y = iv.y;
                     s.uv2[idx] = vec;
                  }; 
               case FlowTarget.UV2_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv2[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z = iv.x;
                     vec.w = iv.y;
                     s.uv2[idx] = vec;
                  }; 
               case FlowTarget.UV3_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv3[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x = iv.x;
                     vec.y = iv.y;
                     s.uv3[idx] = vec;
                  }; 
               case FlowTarget.UV3_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv3[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z = iv.x;
                     vec.w = iv.y;
                     s.uv3[idx] = vec;
                  }; 
            }
            return null;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx] = (Color)v;
               };     
            case BrushTarget.ValueR:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].r = (float)v;
               };
            case BrushTarget.ValueG:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].g = (float)v;
               };
            case BrushTarget.ValueB:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].b = (float)v;
               };
            case BrushTarget.ValueA:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].a = (float)v;
               }; 
            case BrushTarget.UV0_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.x = (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.y = (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.z = (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.w = (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV1_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.x = (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.y = (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.z = (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.w = (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV2_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.x = (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.y = (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.z = (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.w = (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV3_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.x = (float)v;
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.y = (float)v;
                  s.uv3[idx] = vec;
               };
            case BrushTarget.UV3_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.z = (float)v;
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.w = (float)v;
                  s.uv3[idx] = vec;
               }; 
               
         }
         return null;
      }

      delegate void Multiplier(int idx, ref object x);

      Multiplier GetMultiplier(VertexInstanceStream s)
      {
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  return delegate(int idx, ref object v)
                  {
                     Vector2 vv = (Vector2)v;
                     s.colors[idx].r *= vv.x;
                     s.colors[idx].g *= vv.y;
                  }; 
               case FlowTarget.ColorBA:
                  return delegate(int idx, ref object v)
                  {
                     Vector2 vv = (Vector2)v;
                     s.colors[idx].b *= vv.x;
                     s.colors[idx].a *= vv.y;
                  }; 
               case FlowTarget.UV0_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv0[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv0[idx] = vec;
                  }; 
               case FlowTarget.UV0_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv0[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv0[idx] = vec;
                  }; 
               case FlowTarget.UV1_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv1[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv1[idx] = vec;
                  }; 
               case FlowTarget.UV1_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv1[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv1[idx] = vec;
                  }; 
               case FlowTarget.UV2_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv2[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv2[idx] = vec;
                  }; 
               case FlowTarget.UV2_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv2[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv2[idx] = vec;
                  }; 
               case FlowTarget.UV3_XY:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv3[idx];
                     Vector2 iv = (Vector2)v;
                     vec.x *= iv.x;
                     vec.y *= iv.y;
                     s.uv3[idx] = vec;
                  }; 
               case FlowTarget.UV3_ZW:
                  return delegate(int idx, ref object v)
                  {
                     Vector4 vec = s.uv3[idx];
                     Vector2 iv = (Vector2)v;
                     vec.z *= iv.x;
                     vec.w *= iv.y;
                     s.uv3[idx] = vec;
                  }; 
            }
            return null;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx] *= (Color)v;
               };     
            case BrushTarget.ValueR:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].r *= (float)v;
               };
            case BrushTarget.ValueG:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].g *= (float)v;
               };
            case BrushTarget.ValueB:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].b *= (float)v;
               };
            case BrushTarget.ValueA:
               return delegate(int idx, ref object v)
               {
                  s.colors[idx].a *= (float)v;
               }; 
            case BrushTarget.UV0_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.x *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.y *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.z *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.w *= (float)v;
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV1_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.x *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.y *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.z *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.w *= (float)v;
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV2_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.x *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.y *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.z *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.w *= (float)v;
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV3_X:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.x *= (float)v;
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_Y:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.y *= (float)v;
                  s.uv3[idx] = vec;
               };
            case BrushTarget.UV3_Z:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.z *= (float)v;
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_W:
               return delegate(int idx, ref object v)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.w *= (float)v;
                  s.uv3[idx] = vec;
               }; 

         }
         return null;
      }

      delegate void Lerper(int idx,ref object x,float strength);
      
      Lerper GetLerper(VertexInstanceStream s)
      {
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  return delegate(int idx, ref object v, float r)
                  { 
                     Vector2 vv = (Vector2)v;
                     Color c = s.colors[idx];
                     s.colors[idx].r = Mathf.Lerp(c.r, vv.x, r);
                     s.colors[idx].g = Mathf.Lerp(c.g, vv.y, r); 
                  }; 
               case FlowTarget.ColorBA:
                  return delegate(int idx, ref object v, float r)
                  { 
                     Vector2 vv = (Vector2)v;
                     Color c = s.colors[idx];
                     s.colors[idx].b = Mathf.Lerp(c.r, vv.x, r);
                     s.colors[idx].a = Mathf.Lerp(c.g, vv.y, r); 
                  }; 
               case FlowTarget.UV0_XY:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv0[idx];
                     Vector2 t = (Vector2)v;
                     o.x = Mathf.Lerp(o.x, t.x, r);
                     o.y = Mathf.Lerp(o.y, t.y, r);
                     s.uv0[idx] = o;
                  };
               case FlowTarget.UV0_ZW:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv0[idx];
                     Vector2 t = (Vector2)v;
                     o.z = Mathf.Lerp(o.z, t.x, r);
                     o.w = Mathf.Lerp(o.w, t.y, r);
                     s.uv0[idx] = o;
                  }; 
               case FlowTarget.UV1_XY:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv1[idx];
                     Vector2 t = (Vector2)v;
                     o.x = Mathf.Lerp(o.x, t.x, r);
                     o.y = Mathf.Lerp(o.y, t.y, r);
                     s.uv1[idx] = o;
                  }; 
               case FlowTarget.UV1_ZW:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv1[idx];
                     Vector2 t = (Vector2)v;
                     o.z = Mathf.Lerp(o.z, t.x, r);
                     o.w = Mathf.Lerp(o.w, t.y, r);
                     s.uv1[idx] = o;
                  }; 
               case FlowTarget.UV2_XY:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv2[idx];
                     Vector2 t = (Vector2)v;
                     o.x = Mathf.Lerp(o.x, t.x, r);
                     o.y = Mathf.Lerp(o.y, t.y, r);
                     s.uv2[idx] = o;
                  }; 
               case FlowTarget.UV2_ZW:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv2[idx];
                     Vector2 t = (Vector2)v;
                     o.z = Mathf.Lerp(o.z, t.x, r);
                     o.w = Mathf.Lerp(o.w, t.y, r);
                     s.uv2[idx] = o;
                  }; 
               case FlowTarget.UV3_XY:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv3[idx];
                     Vector2 t = (Vector2)v;
                     o.x = Mathf.Lerp(o.x, t.x, r);
                     o.y = Mathf.Lerp(o.y, t.y, r);
                     s.uv3[idx] = o;
                  }; 
               case FlowTarget.UV3_ZW:
                  return delegate(int idx, ref object v, float r)
                  {
                     Vector4 o = s.uv3[idx];
                     Vector2 t = (Vector2)v;
                     o.z = Mathf.Lerp(o.z, t.x, r);
                     o.w = Mathf.Lerp(o.w, t.y, r);
                     s.uv3[idx] = o;
                  }; 
            }
            return null;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               return delegate(int idx, ref object v, float r)
               {
                  s.colors[idx] = Color.Lerp(s.colors[idx], (Color)v, r);
               };     
            case BrushTarget.ValueR:
               return delegate(int idx, ref object v, float r)
               {
                  s.colors[idx].r = Mathf.Lerp(s.colors[idx].r, (float)v, r);
               };
            case BrushTarget.ValueG:
               return delegate(int idx, ref object v, float r)
               {
                  s.colors[idx].g = Mathf.Lerp(s.colors[idx].g, (float)v, r);
               };
            case BrushTarget.ValueB:
               return delegate(int idx, ref object v, float r)
               {
                  s.colors[idx].b = Mathf.Lerp(s.colors[idx].b, (float)v, r);
               };
            case BrushTarget.ValueA:
               return delegate(int idx, ref object v, float r)
               {
                  s.colors[idx].a = Mathf.Lerp(s.colors[idx].a, (float)v, r);
               };
            case BrushTarget.UV0_X:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.x = Mathf.Lerp(vec.x, (float)v, r);
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Y:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.y = Mathf.Lerp(vec.y, (float)v, r);
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_Z:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.z = Mathf.Lerp(vec.z, (float)v, r);
                  s.uv0[idx] = vec;
               }; 
            case BrushTarget.UV0_W:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv0[idx];
                  vec.w = Mathf.Lerp(vec.w, (float)v, r);
                  s.uv0[idx] = vec;
               };   
            case BrushTarget.UV1_X:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.x = Mathf.Lerp(vec.x, (float)v, r);
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Y:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.y = Mathf.Lerp(vec.y, (float)v, r);
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_Z:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.z = Mathf.Lerp(vec.z, (float)v, r);
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV1_W:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv1[idx];
                  vec.w = Mathf.Lerp(vec.w, (float)v, r);
                  s.uv1[idx] = vec;
               }; 
            case BrushTarget.UV2_X:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.x = Mathf.Lerp(vec.x, (float)v, r);
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Y:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.y = Mathf.Lerp(vec.y, (float)v, r);
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_Z:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.z = Mathf.Lerp(vec.z, (float)v, r);
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV2_W:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv2[idx];
                  vec.w = Mathf.Lerp(vec.w, (float)v, r);
                  s.uv2[idx] = vec;
               }; 
            case BrushTarget.UV3_X:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.x = Mathf.Lerp(vec.x, (float)v, r);
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_Y:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.y = Mathf.Lerp(vec.y, (float)v, r);
                  s.uv3[idx] = vec;
               }; 
            case BrushTarget.UV3_Z:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.z = Mathf.Lerp(vec.z, (float)v, r);
                  s.uv3[idx] = vec;
               };
            case BrushTarget.UV3_W:
               return delegate(int idx, ref object v, float r)
               {
                  Vector4 vec = s.uv3[idx];
                  vec.w = Mathf.Lerp(vec.w, (float)v, r);
                  s.uv3[idx] = vec;
               };
               
         }
         return null;
      }

      object GetBrushValue()
      {
         if (tab == Tab.Flow)
         {
            return strokeDir;
         }
         switch (brushMode)
         {
            case BrushTarget.Color:
               return brushColor;   
            case BrushTarget.ValueR:
               return brushValue / 255.0f;
            case BrushTarget.ValueG:
               return brushValue / 255.0f;
            case BrushTarget.ValueB:
               return brushValue / 255.0f;
            case BrushTarget.ValueA:
               return brushValue / 255.0f;
            default:
               return floatBrushValue;
         }
      }


      public enum FlowTarget
      {
         ColorRG = 0,
         ColorBA,
         UV0_XY,
         UV0_ZW,
         UV1_XY,
         UV1_ZW,
         UV2_XY,
         UV2_ZW,
         UV3_XY,
         UV3_ZW
      }
      
      public enum FlowBrushType
      {
         Direction = 0,
         Soften
      }

      public enum FlowVisualization
      {
         Arrows = 0,
         Water,
      }
      
      public enum BrushTarget
      {
         Color = 0,
         ValueR,
         ValueG,
         ValueB,
         ValueA,
         UV0_X,
         UV0_Y,
         UV0_Z,
         UV0_W,
         UV1_X,
         UV1_Y,
         UV1_Z,
         UV1_W,
         UV2_X,
         UV2_Y,
         UV2_Z,
         UV2_W,
         UV3_X,
         UV3_Y,
         UV3_Z,
         UV3_W,
      }
      
      public enum VertexMode
      {
         Adjust,
         Smear,
         Smooth, 
         HistoryEraser,
      }
      
      public enum VertexContraint
      {
         Camera,
         Normal,
         X,
         Y,
         Z,
      }

      public bool            enabled;
      public Vector3         oldpos = Vector3.zero;
      public float           brushSize = 1;
      public float           brushFlow = 8;
      public float           brushFalloff = 1; // linear
      public Color           brushColor = Color.red;
      public int             brushValue = 255;
      public float           floatBrushValue = 1.0f;
      public Vector2         uvVisualizationRange = new Vector2(0, 1);
      public BrushTarget     brushMode = BrushTarget.Color;
      public VertexMode      vertexMode = VertexMode.Adjust;
      public FlowTarget      flowTarget = FlowTarget.ColorRG;
      public FlowBrushType   flowBrushType = FlowBrushType.Direction;
      public FlowVisualization flowVisualization = FlowVisualization.Water;
      public bool            flowRemap01 = true;
      public bool            pull = false;
      public VertexContraint vertexContraint = VertexContraint.Normal;
      public bool            showVertexShader = false;
      public PaintJob[]      jobs = new PaintJob[0];



      public class PaintJob
      {
         public MeshFilter meshFilter;
         public Renderer renderer;
         public VertexInstanceStream _stream;
         // cache of data we often need so we don't have to cross the c#->cpp bridge often 
         public Vector3[] verts;
         public Vector3[] normals;
         public Vector4[] tangents;


         public bool HasStream() { return _stream != null; }
         public VertexInstanceStream stream
         {
            get
            {
               if (_stream == null)
               {
                  if (meshFilter == null)
                  { // object has been deleted
                     return null;
                  }
                  _stream = meshFilter.gameObject.GetComponent<VertexInstanceStream>();
                  if (_stream == null)
                  {
                     _stream = meshFilter.gameObject.AddComponent<VertexInstanceStream>();
                  }
                  else
                  {
                     _stream.Apply();
                  }
               }
               return _stream;
            }

         }

         public void InitMeshConnections()
         {
            Profiler.BeginSample("Generate Mesh Connections");
            // a half edge representation would be nice, but really just care about adjacentcy for now.. 
            vertexConnections = new List<int>[meshFilter.sharedMesh.vertexCount];
            for (int i = 0; i < vertexConnections.Length; ++i)
            {
               vertexConnections[i] = new List<int>();
            }
            int[] tris = meshFilter.sharedMesh.triangles;
            for (int i = 0; i < tris.Length; i=i+3)
            {
               int c0 = tris[i];
               int c1 = tris[i + 1];
               int c2 = tris[i + 2];
               
               List<int> l = vertexConnections[c0];
               if (!l.Contains(c1))
               {
                  l.Add(c1);
               }
               if (!l.Contains(c2))
               {
                  l.Add(c2);
               }
               
               l = vertexConnections[c1];
               if (!l.Contains(c2))
               {
                  l.Add(c2);
               }
               if (!l.Contains(c0))
               {
                  l.Add(c0);
               }
               
               l = vertexConnections[c2];
               if (!l.Contains(c1))
               {
                  l.Add(c1);
               }
               if (!l.Contains(c0))
               {
                  l.Add(c0);
               }
            }
            Profiler.EndSample();
         }

         public List<int>[] vertexConnections;

         public PaintJob(MeshFilter mf, Renderer r)
         {
            meshFilter = mf;
            renderer = r;
            if (r.sharedMaterials != null && r.sharedMaterials.Length > 1)
            {
               stream.originalMaterial = new Material[r.sharedMaterials.Length];
               for (int i = 0; i < r.sharedMaterials.Length; ++i)
               {
                  stream.originalMaterial[i] = r.sharedMaterials[i];
               }
            }
            else
            {
               stream.originalMaterial = new Material[1];
               stream.originalMaterial[0] = r.sharedMaterial;
            }
            verts = mf.sharedMesh.vertices;
            normals = mf.sharedMesh.normals;
            tangents = mf.sharedMesh.tangents;
            // optionally defer this unless the brush is set to position..
            InitMeshConnections();
         }
      }
      
      public void RevertMat()
      {
         // revert old materials
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i].renderer != null)
            {
               var j = jobs[i];
               if (j.renderer.sharedMaterials != null && j.stream.originalMaterial != null &&
                   j.renderer.sharedMaterials.Length == j.stream.originalMaterial.Length &&
                   j.stream.originalMaterial.Length > 1)
               {
                  Material[] mats = new Material[j.stream.originalMaterial.Length];
                  for (int x = 0; x < jobs[i].renderer.sharedMaterials.Length; ++x)
                  {
                     mats[x] = j.stream.originalMaterial[x];
                  }
                  j.renderer.sharedMaterials = mats;
               }
               else
               {
                  jobs[i].renderer.sharedMaterial = jobs[i].stream.originalMaterial[0];
               }
            }
            EditorUtility.SetSelectedWireframeHidden(jobs[i].renderer, true);
         }
      }

      void InitMeshes()
      {
         RevertMat();

         List<PaintJob> pjs = new List<PaintJob>();
         Object[] objs = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.OnlyUserModifiable | SelectionMode.Deep);
         for (int i = 0; i < objs.Length; ++i)
         {
            GameObject go = objs[i] as GameObject;
            if (go != null)
            {
               MeshFilter mf = go.GetComponent<MeshFilter>();
               Renderer r = go.GetComponent<Renderer>();
               if (mf != null && r != null && mf.sharedMesh.isReadable)
               {
                  pjs.Add(new PaintJob(mf, r));
               }
            }
         }

         jobs = pjs.ToArray();
         UpdateDisplayMode();
      }

      void UpdateDisplayMode()
      {
         if (painting)
         {
            EndStroke();
         }
         if (VertexInstanceStream.vertexShaderMat == null)
         {
            VertexInstanceStream.vertexShaderMat = new Material(Shader.Find("Hidden/VertexPainterPro_Preview"));
            VertexInstanceStream.vertexShaderMat.hideFlags = HideFlags.HideAndDontSave;
         }
         for (int i = 0; i < jobs.Length; ++i)
         {
            var job = jobs[i];
            EditorUtility.SetSelectedWireframeHidden(job.renderer, hideMeshWireframe);

            if (!showVertexShader)
            {
               if (job.renderer)
               {
                  if (job.renderer.sharedMaterials != null && job.renderer.sharedMaterials.Length > 1 &&
                      job.renderer.sharedMaterials.Length == job.stream.originalMaterial.Length)
                  {
                     Material[] mats = new Material[jobs[i].renderer.sharedMaterials.Length];

                     for (int x = 0; x < job.renderer.sharedMaterials.Length; ++x)
                     {
                        mats[x] = job.stream.originalMaterial[x];
                     }
                     job.renderer.sharedMaterials = mats;
                  }
                  else
                  {
                     job.renderer.sharedMaterial = job.stream.originalMaterial[0];
                  }
               }
            }
            else
            {
               if (job.renderer != null)
               {
                  if (job.renderer.sharedMaterials != null && job.renderer.sharedMaterials.Length > 1)
                  {
                     Material[] mats = new Material[job.renderer.sharedMaterials.Length];
                     for (int x = 0; x < job.renderer.sharedMaterials.Length; ++x)
                     {
                        mats[x] = VertexInstanceStream.vertexShaderMat;
                     }
                     job.renderer.sharedMaterials = mats;
                  }
                  else
                  {
                     job.renderer.sharedMaterial = VertexInstanceStream.vertexShaderMat;
                  }
                  VertexInstanceStream.vertexShaderMat.SetInt("_flowVisualization", (int)flowVisualization);
                  VertexInstanceStream.vertexShaderMat.SetInt("_tab", (int)tab);
                  VertexInstanceStream.vertexShaderMat.SetInt("_flowTarget", (int)flowTarget);
                  VertexInstanceStream.vertexShaderMat.SetInt("_channel", (int)brushMode);
                  VertexInstanceStream.vertexShaderMat.SetVector("_uvRange", uvVisualizationRange);

               }
            }
         }
      }

      void OnUndo()
      {
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i].stream != null)
            {
               jobs[i].stream.Apply(false);
            }
         }
      }

      void FillMesh(PaintJob job)
      {
         PrepBrushMode(job);
         var setter = GetSetter(job.stream);
         var val = GetBrushValue();
         for (int i = 0; i < job.verts.Length; ++i)
         {
            setter.Invoke(i, ref val);
         }
         job.stream.Apply();
      }

      void RandomMesh(PaintJob job)
      {
         Color oldColor = brushColor;
         int oldVal = brushValue;
         float oldFloat = floatBrushValue;
         PrepBrushMode(job);
         var setter = GetSetter(job.stream);
         for (int i = 0; i < job.verts.Length; ++i)
         {
            brushColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), 
                                   UnityEngine.Random.Range(0.0f, 1.0f), 
                                   UnityEngine.Random.Range(0.0f, 1.0f), 
                                   UnityEngine.Random.Range(0.0f, 1.0f));
            brushValue = UnityEngine.Random.Range(0, 255);
            floatBrushValue = UnityEngine.Random.Range(uvVisualizationRange.x, uvVisualizationRange.y);
            object v = GetBrushValue();
            setter(i, ref v);
         }
         job.stream.Apply();
         brushColor = oldColor;
         brushValue = oldVal;
         floatBrushValue = oldFloat;
      }

      void InitColors(PaintJob j)
      {
         Color[] colors = j.stream.colors;
         if (colors == null || colors.Length != j.verts.Length)
         {
            Color[] orig = j.meshFilter.sharedMesh.colors;
            if (j.meshFilter.sharedMesh.colors != null && j.meshFilter.sharedMesh.colors.Length > 0)
            {
               j.stream.colors = orig;
            }
            else
            {
               j.stream.SetColor(Color.white, j.verts.Length);
            }
         }
      }

      void InitUV0(PaintJob j)
      {
         List<Vector4> uvs = j.stream.uv0;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv != null && j.meshFilter.sharedMesh.uv.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(0, nuv);
               j.stream.uv0 = nuv;
            }
            else
            {
               j.stream.SetUV0(Vector4.zero, j.verts.Length);
            }
         }
      }

      void InitUV1(PaintJob j)
      {
         var uvs = j.stream.uv1;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv2 != null && j.meshFilter.sharedMesh.uv2.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(1, nuv);
               j.stream.uv1 = nuv;
            }
            else
            {
               j.stream.SetUV1(Vector2.zero, j.verts.Length);
            }
         }
      }

      void InitUV2(PaintJob j)
      {
         var uvs = j.stream.uv2;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv3 != null && j.meshFilter.sharedMesh.uv3.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(2, nuv);
               j.stream.uv2 = nuv;
            }
            else
            {
               j.stream.SetUV2(Vector2.zero, j.verts.Length);
            }
         }
      }

      void InitUV3(PaintJob j)
      {
         var uvs = j.stream.uv3;
         if (uvs == null || uvs.Count != j.verts.Length)
         {
            if (j.meshFilter.sharedMesh.uv4 != null && j.meshFilter.sharedMesh.uv4.Length == j.verts.Length)
            {
               List<Vector4> nuv = new List<Vector4>(j.meshFilter.sharedMesh.vertices.Length);
               j.meshFilter.sharedMesh.GetUVs(3, nuv);
               j.stream.uv3 = nuv;
            }
            else
            {
               j.stream.SetUV3(Vector2.zero, j.verts.Length);
            }
         }
      }

      void PrepBrushMode(PaintJob j)
      {
         if (tab == Tab.Deform)
         {
            Vector3[] pos = j.stream.positions;
            if (pos == null || pos.Length != j.verts.Length)
            {
               int vc = j.meshFilter.sharedMesh.vertexCount;
               if (j.stream.positions == null || j.stream.positions.Length != vc)
               {
                  j.stream.positions = new Vector3[j.meshFilter.sharedMesh.vertices.Length];
                  j.meshFilter.sharedMesh.vertices.CopyTo(j.stream.positions, 0);
               }
               if (j.stream.normals == null || j.stream.normals.Length != vc)
               {
                  j.stream.normals = new Vector3[j.meshFilter.sharedMesh.vertices.Length];
                  j.meshFilter.sharedMesh.normals.CopyTo(j.stream.normals, 0);
               }
               if (j.stream.tangents == null || j.stream.tangents.Length != vc)
               {
                  j.stream.tangents = new Vector4[j.meshFilter.sharedMesh.vertices.Length];
                  j.meshFilter.sharedMesh.tangents.CopyTo(j.stream.tangents, 0);
               }
            }
            return;
         }
         if (tab == Tab.Flow)
         {
            switch (flowTarget)
            {
               case FlowTarget.ColorRG:
                  goto case FlowTarget.ColorBA;
               case FlowTarget.ColorBA:
                  {
                     InitColors(j);
                     break;
                  }
               case FlowTarget.UV0_XY:
                  {
                     InitUV0(j);
                     break;
                  }
               case FlowTarget.UV1_XY:
                  {
                     InitUV1(j);
                     break;
                  }
               case FlowTarget.UV2_XY:
                  {
                     InitUV2(j);
                     break;
                  }
               case FlowTarget.UV3_XY:
                  {
                     InitUV3(j);
                     break;
                  }
            }
            return;
         }

         // make sure the instance data is initialized
         switch (brushMode)
         {
            case BrushTarget.Color:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueR:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueG:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueB:
               goto case BrushTarget.ValueA;
            case BrushTarget.ValueA:
               {
                  InitColors(j);
                  break;
               }
            case BrushTarget.UV0_X:
               goto case BrushTarget.UV0_Y;
            case BrushTarget.UV0_Y:
               {
                  InitUV0(j);
                  break;
               }
            case BrushTarget.UV1_X:
               goto case BrushTarget.UV1_Y;
            case BrushTarget.UV1_Y:
               {
                  InitUV1(j);
                  break;
               }
            case BrushTarget.UV2_X:
               goto case BrushTarget.UV2_Y;
            case BrushTarget.UV2_Y:
               {
                  InitUV2(j);
                  break;
               }
            case BrushTarget.UV3_X:
               goto case BrushTarget.UV3_Y;
            case BrushTarget.UV3_Y:
               {
                  InitUV3(j);
                  break;
               }

         }

      }

      void PaintMesh(PaintJob j, Vector3 point)
      {
         Profiler.BeginSample("Paint Mesh");
         PrepBrushMode(j);
         // convert point into local space, so we don't have to convert every point
         point = j.renderer.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
         // for some reason this doesn't handle scale, seems like it should
         // we handle it poorly until I can find a better solution
         float scale = 1.0f / Mathf.Abs(j.renderer.transform.lossyScale.x);

         float bz = scale * brushSize;
         var lerper = GetLerper(j.stream);
         var value = GetBrushValue();
         float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;

         bool modPos = !(j.stream.positions == null || j.stream.positions.Length == 0);
         if (tab == Tab.Flow)
         {
            float strength = strokeDir.magnitude;
            Vector3 sd = strokeDir.normalized;
            Vector2 target = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < j.verts.Length; ++i)
            {
               float d = Vector3.Distance(point, modPos ? j.stream.positions[i] : j.verts[i]);
               if (d < bz)
               {
                  Vector3 n = j.normals[i];
                  Vector4 t = j.tangents[i];
                  
                  if (j.stream.normals != null && j.stream.normals.Length == j.verts.Length)
                  {
                     n = j.stream.normals[i];
                  }
                  if (j.stream.tangents != null && j.stream.tangents.Length == j.verts.Length)
                  {
                     t = j.stream.tangents[i];
                  }

                  var mtx = j.meshFilter.transform.localToWorldMatrix;
                  n = mtx.MultiplyVector(n);
                  Vector3 tg = new Vector3(t.x, t.y, t.z);
                  tg = mtx.MultiplyVector(tg);
                  t.x = tg.x;
                  t.y = tg.y;
                  t.z = tg.z;

                  target.x = 0.5f;
                  target.y = 0.5f;
                  if (flowBrushType == FlowBrushType.Direction)
                  {
                     Vector3 b = Vector3.Cross(n, new Vector3(t.x, t.y, t.z) * t.w);


                     float dx = Vector3.Dot(t, sd);
                     float dy = Vector3.Dot(b, sd);
                     
                     target = new Vector2(dx, dy);
                     target.Normalize();

                     /* using matrix; more code for same result. 
                     Matrix4x4 tbn = new Matrix4x4();
                     tbn.SetRow(0, t);
                     tbn.SetRow(1, new Vector4(b.x, b.y, b.z, 0));
                     tbn.SetRow(2, new Vector4(n.x, n.y, n.z, 0));
                     tbn.SetRow(3, new Vector4(0,0,0,1));

                     Vector3 res = tbn.MultiplyVector(sd);
                     target.x = res.x;
                     target.y = res.y;
                     */
                     if (flowTarget == FlowTarget.ColorBA || flowTarget == FlowTarget.ColorRG || flowRemap01)
                     {
                        target.x = target.x * 0.5f + 0.5f;
                        target.y = target.y * 0.5f + 0.5f;
                     }
                  }

                  float str = 1.0f - d / bz;
                  str *= strength;  // take brush speed into account..
                  str = Mathf.Pow(str, brushFalloff);

                  object obj = target;
                  float finalStr = str * (float)deltaTime * brushFlow * pressure;
                  if (finalStr > 0)
                  {
                     lerper.Invoke(i, ref obj, finalStr);
                  }
               }
            }
         } 
         else if (tab == Tab.Deform)
         {
            for (int i = 0; i < j.verts.Length; ++i)
            {
               float d = Vector3.Distance(point, j.verts[i]);
               if (d < bz)
               {
                  float str = 1.0f - d / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  PaintVertPosition(j, i,  str * (float)deltaTime * brushFlow * pressure);
               }
            }
         }
         else
         {
            Profiler.BeginSample("Paint Color");
            for (int i = 0; i < j.verts.Length; ++i)
            {
               float d = Vector3.Distance(point, j.verts[i]);
               if (d < bz)
               {
                  float str = 1.0f - d / bz;
                  str = Mathf.Pow(str, brushFalloff);
                  float finalStr = str * (float)deltaTime * brushFlow * pressure;
                  if (finalStr > 0)
                  {
                     lerper.Invoke(i, ref value, finalStr);
                  }
               }
            }
            Profiler.EndSample();
         }
         

         j.stream.Apply();
         Profiler.EndSample();
      }

      void EndStroke()
      {
         painting = false;
        
         // could possibly make this faster by avoiding the double apply..
         if (tab == Tab.Deform)
         {
            Profiler.BeginSample("Recalculate Normals and Tangents");
            for (int i = 0; i < jobs.Length; ++i)
            {
               PaintJob j = jobs[i];
               if (j.stream.positions != null && j.stream.normals != null && j.stream.tangents != null)
               {
                  Mesh m = j.stream.Apply(false);
                  m.triangles = j.meshFilter.sharedMesh.triangles;

                  m.RecalculateNormals();
                  if (j.stream.normals == null)
                  {
                     j.stream.normals = new Vector3[m.vertexCount];
                  }
                  m.normals.CopyTo(j.stream.normals, 0);

                  m.uv = j.meshFilter.sharedMesh.uv;
                  CalculateMeshTangents(m);
                  if (j.stream.tangents == null)
                  {
                     j.stream.tangents = new Vector4[m.vertexCount];
                  }
                  m.tangents.CopyTo(j.stream.tangents, 0);

                  m.RecalculateBounds();
                  j.stream.Apply();
               }
            }
            Profiler.EndSample();
         }
         //Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
      }

      void CalculateMeshTangents(Mesh mesh)
      {
         //speed up math by copying the mesh arrays
         int[] triangles = mesh.triangles;
         Vector3[] vertices = mesh.vertices;
         Vector2[] uv = mesh.uv;
         Vector3[] normals = mesh.normals;
         
         //variable definitions
         int triangleCount = triangles.Length;
         int vertexCount = vertices.Length;
         
         Vector3[] tan1 = new Vector3[vertexCount];
         Vector3[] tan2 = new Vector3[vertexCount];
         
         Vector4[] tangents = new Vector4[vertexCount];
         
         for (long a = 0; a < triangleCount; a += 3)
         {
            long i1 = triangles[a + 0];
            long i2 = triangles[a + 1];
            long i3 = triangles[a + 2];
            
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];
            Vector3 v3 = vertices[i3];
            
            Vector2 w1 = uv[i1];
            Vector2 w2 = uv[i2];
            Vector2 w3 = uv[i3];
            
            float x1 = v2.x - v1.x;
            float x2 = v3.x - v1.x;
            float y1 = v2.y - v1.y;
            float y2 = v3.y - v1.y;
            float z1 = v2.z - v1.z;
            float z2 = v3.z - v1.z;
            
            float s1 = w2.x - w1.x;
            float s2 = w3.x - w1.x;
            float t1 = w2.y - w1.y;
            float t2 = w3.y - w1.y;
            
            float div = s1 * t2 - s2 * t1;
            float r = div == 0.0f ? 0.0f : 1.0f / div;
            
            Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
            
            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;
            
            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
         }
         
         
         for (long a = 0; a < vertexCount; ++a)
         {
            Vector3 n = normals[a];
            Vector3 t = tan1[a];
            
            //Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
            //tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
            Vector3.OrthoNormalize(ref n, ref t);
            tangents[a].x = t.x;
            tangents[a].y = t.y;
            tangents[a].z = t.z;
            
            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
         }
         
         mesh.tangents = tangents;
      }

      void ConstrainAxis(ref Vector3 cur, Vector3 orig)
      {
         if (vertexContraint == VertexContraint.X)
         {
            cur.y = orig.y;
            cur.z = orig.z;
         }
         else if (vertexContraint == VertexContraint.Y)
         {
            cur.x = orig.x;
            cur.z = orig.z;
         }
         else if (vertexContraint == VertexContraint.Z)
         {
            cur.x = orig.x;
            cur.y = orig.y;
         }
      }

      void PaintVertPosition(PaintJob j, int i, float strength)
      {
         switch (vertexMode)
         {
            case VertexMode.Adjust:
               {
                  switch (vertexContraint)
                  {
                     case VertexContraint.Normal:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = j.stream.normals[i].normalized;
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.Camera:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = strokeDir;
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.X:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = new Vector3(1, 0, 0);
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.Y:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = new Vector3(0, 1, 0);
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                     case VertexContraint.Z:
                        {
                           Vector3 cur = j.stream.positions[i];
                           Vector3 dir = new Vector3(0, 0, 1);
                           dir *= strength;
                           cur += pull ? dir : -dir;
                           j.stream.positions[i] = cur;
                           break;
                        }
                  }
                  break;
               }
            case VertexMode.Smooth:
               {
                  Vector3 cur = j.stream.positions[i];
                  var con = j.vertexConnections[i];
                  for (int x = 0; x < con.Count; ++x)
                  {
                     cur += j.stream.positions[con[x]];
                  }
                  cur /= (con.Count + 1);
                  ConstrainAxis(ref cur, j.stream.positions[i]);

                  j.stream.positions[i] = Vector3.Lerp(j.stream.positions[i], cur, Mathf.Clamp01(strength));
                  break;
               }
            case VertexMode.Smear:
               {
                  Vector3 cur = j.stream.positions[i];
                  Vector3 dir = strokeDir;
                  dir *= strength;
                  cur += pull ? dir : -dir;
                  j.stream.positions[i] = cur;
                  break;
               }
            case VertexMode.HistoryEraser:
               {
                  Vector3 cur = j.stream.positions[i];
                  Vector3 orig = j.verts[i];
                  ConstrainAxis(ref orig, cur);
                  j.stream.positions[i] = Vector3.Lerp(cur, orig, Mathf.Clamp01(strength));
                  break;
               }
         }
      }
      
      double deltaTime = 0;
      double lastTime = 0;
      bool painting = false;
      Vector3 oldMousePosition;
      Vector3 strokeDir = Vector3.zero;

      void OnSceneGUI(SceneView sceneView)
      {
         deltaTime = EditorApplication.timeSinceStartup - lastTime;
         lastTime = EditorApplication.timeSinceStartup;

         if (jobs.Length == 0 && Selection.activeGameObject != null)
         {
            InitMeshes();
         }

         if (!enabled || jobs.Length == 0 || Selection.activeGameObject == null)
         {
            return;
         }

         if (tab == Tab.Bake)
         {
            return;
         }

         if (VertexInstanceStream.vertexShaderMat != null)
         {
            VertexInstanceStream.vertexShaderMat.SetFloat("_time", (float)EditorApplication.timeSinceStartup);
         }

         RaycastHit hit;
         float distance = float.MaxValue;
         Vector3 mousePosition = Event.current.mousePosition;
         mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;

         Vector3 fakeMP = mousePosition;
         fakeMP.z = 20;
         Vector3 point = sceneView.camera.ScreenToWorldPoint(fakeMP);
         Vector3 normal = Vector3.forward;
         Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i] == null || jobs[i].meshFilter == null)
               continue;
            Matrix4x4 mtx = jobs[i].meshFilter.transform.localToWorldMatrix;
            Mesh msh = jobs[i].meshFilter.sharedMesh;
            if (jobs[i].HasStream())
            {
               msh = jobs[i].stream.GetModifierMesh(); 
               if (msh == null)
               {
                  msh = jobs[i].meshFilter.sharedMesh;
               }
            }

            if (RXLookingGlass.IntersectRayMesh(ray, msh, mtx, out hit))
            {
               if (Event.current.shift == false) 
               {
                  if (hit.distance < distance) 
                  {
                     distance = hit.distance;
                     point = hit.point;
                     oldpos = hit.point;
                     normal = hit.normal;
                  }
               } 
               else 
               {
                  point = oldpos;
               }
            } 
            else 
            {
               if (Event.current.shift == true) 
               {
                  point = oldpos;
               }
            }  
         }
         strokeDir = Vector3.zero;
         if (tab == Tab.Flow || vertexMode == VertexMode.Smear)
         {
            if (Event.current.isMouse)
            {
               strokeDir = (point - oldMousePosition);
               strokeDir.x *= Event.current.delta.magnitude;
               strokeDir.y *= Event.current.delta.magnitude;
               strokeDir.z *= Event.current.delta.magnitude;
               oldMousePosition = point;
            }
         }
         else if (vertexMode == VertexMode.Adjust)
         {
            strokeDir = -sceneView.camera.transform.forward;
         }

         if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt == false)
         {
            painting = true;
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RegisterCompleteObjectUndo(jobs[i].stream, "Vertex Painter Stroke");
            }

         }
            
         if (Event.current.type == EventType.KeyUp && Event.current.control)
         {
            if (Event.current.keyCode == KeyCode.W)
            {
               hideMeshWireframe = !hideMeshWireframe;
               for (int i = 0; i < jobs.Length; ++i)
               {
                  EditorUtility.SetSelectedWireframeHidden(jobs[i].renderer, hideMeshWireframe);
               }
            }
            if (Event.current.keyCode == KeyCode.V)
            {
               showVertexShader = !showVertexShader;
               UpdateDisplayMode();
            }
         }



         if (Event.current.type == EventType.MouseMove && Event.current.shift) {
            brushSize += Event.current.delta.x * (float)deltaTime * (float)6;
            brushFalloff -= Event.current.delta.y * (float)deltaTime * (float)48;
         }
         /*Player7 End*/

         if (Event.current.rawType == EventType.MouseUp)
         {
            EndStroke();
         }
         if (Event.current.type == EventType.MouseMove && Event.current.alt)
         {
            brushSize += Event.current.delta.y * (float)deltaTime;
         }

         // set brush color
         if (brushMode == BrushTarget.Color)
         {
            Handles.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.4f);
         }
         else if (brushMode == BrushTarget.ValueR || brushMode == BrushTarget.ValueG || 
            brushMode == BrushTarget.ValueB || brushMode == BrushTarget.ValueA)
         {
            float v = (float)brushValue / 255.0f;
            Handles.color = new Color(v, v, v, 0.4f);
         }
         else
         {
            float v = (floatBrushValue - uvVisualizationRange.x) / Mathf.Max(0.00001f, uvVisualizationRange.y);
            Handles.color = new Color(v, v, v, 0.4f);
         }

         if (tab != Tab.Deform)
         {
            Handles.SphereCap(0, point, Quaternion.identity, brushSize * 2);
         }
         else
         {
            Handles.color = new Color(0.8f, 0, 0, 1.0f);
            float r = Mathf.Pow(0.5f, brushFalloff);
            Handles.DrawWireDisc(point, normal, brushSize * 2 * r);
            Handles.color = new Color(0.9f, 0, 0, 0.8f);
            Handles.DrawWireDisc(point, normal, brushSize * 2);
         }
         // eat current event if mouse event and we're painting
         if (Event.current.isMouse && painting)
         {
            Event.current.Use();
         } 

         if (Event.current.type == EventType.Layout)
         {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
         }

         // only paint once per frame
         if (tab != Tab.Flow && Event.current.type != EventType.Repaint)
         {
            return;
         }


         if (jobs.Length > 0 && painting)
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               PaintMesh(jobs[i], point);
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Stroke");

            }
         }

         // update views
         sceneView.Repaint();
         HandleUtility.Repaint();
      }
   }
}

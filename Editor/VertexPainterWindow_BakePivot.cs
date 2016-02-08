using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

// bake pivots into mesh verticies. This can be very useful for things like tree billboards, object shattering, etc,
// as it allows you to rotate each polygon individually. To use, build your geometry as a bunch of separate meshes,
// bake the data using this tool, and combine the meshes into one mesh. Another useful thing is to bake a random number
// into the spare channel; for instance, if your shattering an object you can use this to make each shard rotate
// in a different direction, etc..

namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow 
   {
      enum PivotTarget
      {
         UV0,
         UV1,
         UV2,
         UV3
      }

      PivotTarget pivotTarget = PivotTarget.UV2;

      void BakePivot()
      {
         switch (pivotTarget)
         {
            case PivotTarget.UV0:
            {
               InitBakeChannel(BakeChannel.UV0);
               foreach (PaintJob job in jobs)
               {
                  Vector3 lp = job.meshFilter.transform.localPosition;
                  job.stream.SetUV0(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
               }
               break;
            }
            case PivotTarget.UV1:
            {
               InitBakeChannel(BakeChannel.UV1);
               foreach (PaintJob job in jobs)
               {
                  Vector3 lp = job.meshFilter.transform.localPosition;
                  job.stream.SetUV1(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
               }
               break;
            }
            case PivotTarget.UV2:
               {
                  InitBakeChannel(BakeChannel.UV2);
                  foreach (PaintJob job in jobs)
                  {
                     Vector3 lp = job.meshFilter.transform.localPosition;
                     job.stream.SetUV2(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
                  }
                  break;
               }
            case PivotTarget.UV3:
            {
               InitBakeChannel(BakeChannel.UV3);
               foreach (PaintJob job in jobs)
               {
                  Vector3 lp = job.meshFilter.transform.localPosition;
                  job.stream.SetUV3(new Vector4(lp.x, lp.y, lp.z, UnityEngine.Random.Range(0.0f, 1.0f)), job.verts.Length);
               }
               break;
            }
         }
      }
   }
}

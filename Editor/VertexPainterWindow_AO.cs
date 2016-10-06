
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow 
   {
      public int     aoSamples = 512;
      public Vector2 aoRange = new Vector2(0.0001f, 1.5f);
      public float   aoIntensity = 2.0f;
      public bool    bakeLighting;
      public Color   aoLightAmbient = new Color(0.05f, 0.05f, 0.05f, 1);

      public enum AOBakeMode
      {
         Replace = 0,
         Multiply
      }
      public AOBakeMode aoBakeMode = AOBakeMode.Replace;


      RaycastHit hit = new RaycastHit();
     
      void ApplyAOLight(ref Color c, Light l, Vector3 pos, Vector3 n)
      {
         if (!l.isActiveAndEnabled)
            return;
         Vector3 dir;
         float intensity = l.intensity;
         if (l.type == LightType.Directional)
         {
            dir = -l.transform.forward;
         }
         else if (l.type == LightType.Point)
         {
            dir = (l.transform.position - pos).normalized;
            intensity *= Mathf.Clamp01((l.range - Vector3.Distance(l.transform.position, pos)) / l.range);
         }
         else
         {
            return;
         }
         
         intensity *= Mathf.Clamp01(Vector3.Dot(n, dir));
        
         c.r += l.color.r * intensity;
         c.g += l.color.g * intensity;
         c.b += l.color.b * intensity;
      }

      void BakeAO()
      {
         Light[] aoLights = null;
         if (bakeLighting)
         {
            aoLights = GameObject.FindObjectsOfType<Light>();
         }
         int sample = 0;
         int numVerts = 0;
         for (int i = 0; i < jobs.Length; ++i)
         {
            numVerts += jobs[i].verts.Length;
         }
         int numSamples = numVerts * aoSamples;

         float oldFloat = floatBrushValue;
         Color oldColor = brushColor;
         int oldVal = brushValue;

         // add temp colliders if needed
         bool[] tempCollider = new bool[jobs.Length];
         for (int jIdx = 0; jIdx < jobs.Length; ++jIdx)
         {
            PaintJob job = jobs[jIdx];

            if (job.meshFilter.GetComponent<Collider>() == null)
            {
               job.meshFilter.gameObject.AddComponent<MeshCollider>();
               tempCollider[jIdx] = true;
            }
         }
        
         // do AO
         for (int jIdx = 0; jIdx < jobs.Length; ++jIdx)
         {
            PaintJob job = jobs[jIdx];

            PrepBrushMode(job);
            // bake down the mesh so we take instance positions into account..
            Mesh mesh = BakeDownMesh(job.meshFilter.sharedMesh, job.stream);
            Vector3[] verts = mesh.vertices;
            if (mesh.normals == null || mesh.normals.Length == 0)
            {
               mesh.RecalculateNormals();
            }
            Vector3[] normals = mesh.normals;

            brushValue = 255;
            floatBrushValue = 1.0f;
            brushColor = Color.white;
            var val = GetBrushValue();
            Lerper lerper = null;
            Multiplier mult = null;

            if (aoBakeMode == AOBakeMode.Replace)
            {
               lerper = GetLerper();
               for (int i = 0; i < job.verts.Length; ++i)
               {
                  lerper.Invoke(job, i, ref val, 1);
               }
            }
            else
            {
               mult = GetMultiplier();
            }

            for (int i = 0; i<verts.Length; i++) 
            {
               Vector3 norm = normals[i];
               // to world space!
               Vector3 v = job.meshFilter.transform.TransformPoint(verts[i]);
               Vector3 n = job.meshFilter.transform.TransformPoint(verts[i] + norm);
               Vector3 worldSpaceNormal = (n-v).normalized;

               float totalOcclusion = 0;

               // the slow part..
               for (int j = 0; j < aoSamples; j++) 
               {
                  // random rotate around hemisphere
                  float rot = 180.0f;
                  float rot2 = rot / 2.0f;
                  float rotx = (( rot * Random.value ) - rot2);
                  float roty = (( rot * Random.value ) - rot2);
                  float rotz = (( rot * Random.value ) - rot2);

                  Vector3 dir = Quaternion.Euler( rotx, roty, rotz ) * Vector3.up;
                  Quaternion dirq = Quaternion.FromToRotation(Vector3.up, worldSpaceNormal);
                  Vector3 ray = dirq * dir;
                  Vector3 offset = Vector3.Reflect( ray, worldSpaceNormal );

                  // raycast
                  ray = ray * (aoRange.y/ray.magnitude);
                  if ( Physics.Linecast( v-(offset*0.1f), v + ray, out hit ) ) 
                  {
                     if ( hit.distance > aoRange.x ) 
                     {
                        totalOcclusion += Mathf.Clamp01( 1 - ( hit.distance / aoRange.y ) );
                     }
                  }

                  sample++;
                  if (sample % 500 == 0) 
                  {
                     EditorUtility.DisplayProgressBar("Baking AO...", "Baking...", (float)sample / (float)numSamples);
                  }
               }

               totalOcclusion = Mathf.Clamp01( 1 - ((totalOcclusion*aoIntensity)/aoSamples) );

               if (aoLights != null && aoLights.Length > 0)
               {
                  Color c = aoLightAmbient;
                  for (int l = 0; l < aoLights.Length; ++l)
                  {
                     Light light = aoLights[l];
                     ApplyAOLight(ref c, light, v, n);
                  }
                  c.r *= totalOcclusion;
                  c.g *= totalOcclusion;
                  c.b *= totalOcclusion;
                  c.a = totalOcclusion;
                  brushColor = c;

                  // if we're lit and targeting a channel other than color, bake max intensity..
                  floatBrushValue = Mathf.Max(Mathf.Max(c.r, c.g), c.b) * totalOcclusion;
                  brushValue = (int)(floatBrushValue * 255);
               }
               else
               {
                  brushColor.r = totalOcclusion;
                  brushColor.g = totalOcclusion;
                  brushColor.b = totalOcclusion;
                  brushColor.a = totalOcclusion;

                  floatBrushValue = totalOcclusion;
                  brushValue = (int)(totalOcclusion * 255);
               }
               val = GetBrushValue();
               if (aoBakeMode == AOBakeMode.Replace)
               {
                  lerper.Invoke(job, i, ref val, 1);
               }
               else
               {
                  mult.Invoke(job.stream, i, ref val);
               }
            }
            job.stream.Apply();
            EditorUtility.SetDirty(job.stream);
            EditorUtility.SetDirty(job.stream.gameObject);
            DestroyImmediate(mesh);
            
            brushValue = oldVal;
            floatBrushValue = oldFloat;
            brushColor = oldColor;
         }
         // remove temp colliders
         for (int jIdx = 0; jIdx < jobs.Length; ++jIdx)
         {
            if (tempCollider[jIdx] == true)
            {
               Collider c = jobs[jIdx].meshFilter.GetComponent<Collider>();
               if (c != null)
               {
                  DestroyImmediate(c);
               }
            }
         }
         
         EditorUtility.ClearProgressBar();
         SceneView.RepaintAll();
         
      }
   }

}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.VertexPainterPro
{
   [System.Serializable]
   public class SaveMeshCache : IVertexPainterUtility
   {
      public string GetName() 
      {
         return "Bake Scene to Mesh Cache";
      }

      bool enabledStaticBatching = false;
      bool keepMeshReadWrite = true;
      bool useCurrentJobs = true;

      public void OnGUI(PaintJob[] jobs)
      {
         EditorGUILayout.HelpBox("Bakes all VertexInstanceStreams down to new meshes, and replaces the current meshes with the baked ones", MessageType.Info);

         enabledStaticBatching = EditorGUILayout.Toggle("Enable Static Batching", enabledStaticBatching);
         keepMeshReadWrite = EditorGUILayout.Toggle("Keep Mesh Editable", keepMeshReadWrite);
         useCurrentJobs = EditorGUILayout.Toggle("Process only current selection", useCurrentJobs);

         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Save and Replace"))
         {
            Bake(enabledStaticBatching, keepMeshReadWrite, useCurrentJobs ? jobs : null);
         }

         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
      }


      public static void Bake(bool enableStaticBatching, bool keepMeshesReadWrite, PaintJob[] jobs = null)
      {
         List<VertexInstanceStream> streams = new List<VertexInstanceStream>();
         if (jobs == null)
         {
            streams = new List<VertexInstanceStream>(GameObject.FindObjectsOfType<VertexInstanceStream>());
         }
         else
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               streams.Add(jobs[i].stream);
            }
         }

         if (streams.Count == 0)
         {
            Debug.Log("No streams to save");
            return;
         }
         
         var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
         string path = scene.path;
         path = path.Replace("\\", "/");
         if (path.Contains(".unity"))
         {
            path = path.Replace(".unity", "");
         }
         path += "_meshcache.asset";

         AssetDatabase.DeleteAsset(path);
         for (int i = 0; i < streams.Count; ++i)
         {
            var stream = streams[i];
            var mf = stream.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
               continue;

            var msci = mf.GetComponent<MegaSplatCollisionInfo>();
            if (msci)
            {
               msci.BakeForSerialize();
               EditorUtility.SetDirty(msci);
            }

            Mesh m = VertexPainterUtilities.BakeDownMesh(mf.sharedMesh, stream);
            m.name = mf.gameObject.name;
            if (i == 0)
            {
               AssetDatabase.CreateAsset(m, path);
            }
            else
            {
               AssetDatabase.AddObjectToAsset(m, path);
            }

            mf.sharedMesh = m;
            mf.gameObject.isStatic = enableStaticBatching;
            m.UploadMeshData(!keepMeshesReadWrite);
            GameObject.DestroyImmediate(mf.gameObject.GetComponent<VertexInstanceStream>());

            EditorUtility.SetDirty(mf.gameObject);
         }
         AssetDatabase.SaveAssets();
        
      }

   }
}
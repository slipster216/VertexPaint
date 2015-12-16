using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

/*  VertexPainterWindow
 *    - Jason Booth
 * 
 *    Uses Unity 5.0+ MeshRenderer.additionalVertexStream so that you can paint per-instance vertex colors on your meshes.
 * A component is added to your mesh to serialize this data and set it at load time. This is more effecient than making
 * duplicate meshes, and certainly less painful than saving them as separate asset files on disk. However, if you only have
 * one copy of the vertex information in your game and want to burn it into the original mesh, you can use the save feature
 * to save a new version of your mesh with the data burned into the verticies, avoiding the need for the runtime component. 
 * 
 * In other words, bake it if you need to instance the paint job - however, if you want tons of the same instances painted
 * uniquely in your scene, keep the component version and skip the baking..
 * 
 * One possible optimization is to have the component free the array after updating the mesh when in play mode..
 * 
 * Also supports burning data into the UV channels, in case you want some additional channels to work with, which also
 * happen to be full 32bit floats. You can set a viewable range; so if your floats go from 0-120, it will remap it to
 * 0-1 for display in the shader. That way you can always see your values, even when they go out of color ranges.
 * 
 * Note that as of this writing Unity has a bug in the additionalVertexStream function. The docs claim the data applied here
 * will supply or overwrite the data in the mesh, however, this is not true. Rather, it will only replace the data that's 
 * there - if your mesh has no color information, it will not upload the color data in additionalVertexStream, which is sad
 * because the original mesh doesn't need this data. As a workaround, if your mesh does not have color channels on the verts,
 * they will be created for you. 
 * 
 * There is another bug in additionalVertexStream, in that the mesh keeps disapearing in edit mode. So the component
 * which holds the data caches the mesh and keeps assigning it in the Update call, but only when running in the editor
 * and not in play mode. 
 * 
 * Really, the additionalVertexStream mesh should be owned by the MeshRenderer and saved as part of the objects instance
 * data. That's essentially what the VertexInstaceStream component does, but it's annoying and wasteful of memory to do
 * it this way since it doesn't need to be on the CPU at all. Enlighten somehow does this with the UVs it generates
 * this way, but appears to be handled specially. Oh, Unity..
*/



namespace JBooth.VertexPainterLite
{
   public partial class VertexPainterWindow : EditorWindow 
   {

      
      bool DrawClearButton(string label)
      {
         if (GUILayout.Button(label, GUILayout.Width(46)))
         {
            return (EditorUtility.DisplayDialog("Confirm", "Clear " + label + " data?", "ok", "cancel"));
         }
         return false;
      }

      Mesh CopyMesh(Mesh mesh)
      {
         var copy = new Mesh();
         foreach(var property in typeof(Mesh).GetProperties())
         {
            if(property.GetSetMethod() != null && property.GetGetMethod() != null)
            {
               property.SetValue(copy, property.GetValue(mesh, null), null);
            }
         }
         copy.hideFlags = 0;
         return copy;
      }
      
      void OnGUI()
      {
         
         if (Selection.activeGameObject == null)
         {
            EditorGUILayout.LabelField("No objects selected. Please select an object with a MeshFilter and Renderer");
            return;
         }

         if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp)
         {
            enabled = !enabled;
         }
         
         enabled = GUILayout.Toggle(enabled, "Enabled (ESC)");
         
         var oldShow = showVertexShader;
         showVertexShader = GUILayout.Toggle(showVertexShader, "Show Vertex Data");
         if (oldShow != showVertexShader)
         {
            UpdateDisplayMode();
         }
         
         bool hasColors = false;
         bool hasUV0 = false;
         bool hasUV1 = false;
         bool hasUV2 = false;
         bool hasUV3 = false;
         
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (jobs[i]._stream != null)
            {
               int vertexCount = jobs[i].verts.Length;
               if (jobs[i].stream.colors != null && jobs[i].stream.colors.Length == vertexCount)
               {
                  hasColors = true;
               }
               if (jobs[i].stream.uv0 != null && jobs[i].stream.uv0.Length == vertexCount)
               {
                  hasUV0 = true;
               }
               if (jobs[i].stream.uv1 != null && jobs[i].stream.uv1.Length == vertexCount)
               {
                  hasUV1 = true;
               }
               if (jobs[i].stream.uv2 != null && jobs[i].stream.uv2.Length == vertexCount)
               {
                  hasUV2 = true;
               }
               if (jobs[i].stream.uv3 != null && jobs[i].stream.uv3.Length == vertexCount)
               {
                  hasUV3 = true;
               }
            }
         }
         
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.PrefixLabel("Clear Channel:");
         if (hasColors && DrawClearButton("Colors"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
               jobs[i].stream.colors = null;
               jobs[i].stream.Apply();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         if (hasUV0 && DrawClearButton("UV0"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
               jobs[i].stream.uv0 = null;
               jobs[i].stream.Apply();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         if (hasUV1 && DrawClearButton("UV1"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
               jobs[i].stream.uv1 = null;
               jobs[i].stream.Apply();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         if (hasUV2 && DrawClearButton("UV2"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
               jobs[i].stream.uv2 = null;
               jobs[i].stream.Apply();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         if (hasUV3 && DrawClearButton("UV3"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
               jobs[i].stream.uv3 = null;
               jobs[i].stream.Apply();
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         EditorGUILayout.EndHorizontal();
         
         var oldBM = brushMode;
         brushMode = (BrushMode)EditorGUILayout.EnumPopup("Brush Mode", brushMode);
         if (oldBM != brushMode)
         {
            UpdateDisplayMode();
         }
         if (brushMode == BrushMode.Color)
         {
            brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);
         }
         else if (brushMode == BrushMode.ValueR || brushMode == BrushMode.ValueG || brushMode == BrushMode.ValueB || brushMode == BrushMode.ValueA)
         {
            brushValue = (int)EditorGUILayout.Slider("Brush Value", (float)brushValue, 0.0f, 256.0f);
         }
         else
         {
            floatBrushValue = EditorGUILayout.FloatField("Brush Value", floatBrushValue);
            var oldUVRange = uvVisualizationRange;
            uvVisualizationRange = EditorGUILayout.Vector2Field("Visualize Range", uvVisualizationRange);
            if (oldUVRange != uvVisualizationRange)
            {
               UpdateDisplayMode();
            }
         }
         
         if (GUILayout.Button("Fill"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               FillMesh(jobs[i]);
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 10.0f);
         brushFlow = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 128.0f);



         if (GUILayout.Button("Save Asset"))
         {
            if (jobs.Length != 0)
            {
               string path = EditorUtility.SaveFilePanel("Save Asset", Application.dataPath, "models", "asset");
               if (!string.IsNullOrEmpty(path))
               {
                  path = FileUtil.GetProjectRelativePath(path);
                  Mesh firstMesh = CopyMesh(jobs[0].meshFilter.sharedMesh);
                  firstMesh.colors = jobs[0].stream.colors;
                  firstMesh.uv = jobs[0].stream.uv0;
                  firstMesh.uv2 = jobs[0].stream.uv1;
                  firstMesh.uv3 = jobs[0].stream.uv2;
                  firstMesh.uv4 = jobs[0].stream.uv3;
                  
                  AssetDatabase.CreateAsset(firstMesh, path);
                  
                  for (int i = 1; i < jobs.Length; ++i)
                  {
                     Mesh m = CopyMesh(jobs[i].meshFilter.sharedMesh);
                     m.colors = jobs[i].stream.colors;
                     m.colors = jobs[0].stream.colors;
                     m.uv = jobs[0].stream.uv0;
                     m.uv2 = jobs[0].stream.uv1;
                     m.uv3 = jobs[0].stream.uv2;
                     m.uv4 = jobs[0].stream.uv3;
                     AssetDatabase.AddObjectToAsset(m, firstMesh);
                  }
                  AssetDatabase.SaveAssets();
                  AssetDatabase.ImportAsset(path);
               }
            }
         }
         
      }
      
      
      void OnFocus() 
      {
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;
         Repaint();
      }
      
      void OnInspectorUpdate()
      {
         // unfortunate...
         Repaint ();
      }
      
      void OnSelectionChange()
      {
         InitMeshes();
         this.Repaint();
      }
      
      void OnDestroy() 
      {
         bool show = showVertexShader;
         showVertexShader = false;
         UpdateDisplayMode();
         showVertexShader = show;
         DestroyImmediate(vertexShaderMat);
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
      }
   }
}
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



namespace JBooth.VertexPainterPro
{
   public partial class VertexPainterWindow : EditorWindow 
   {
      enum Tab
      {
         Paint = 0,
         Deform,
         Flow,
         Bake,
         Custom
      }

      string[] tabNames =
      {
         "Paint",
         "Deform",
         "Flow",
         "Bake",
         "Custom"
      };


      static string sSwatchKey = "VertexPainter_Swatches";

      ColorSwatches swatches = null;


      Tab tab = Tab.Paint;

      bool hideMeshWireframe = false;
      
      bool DrawClearButton(string label)
      {
         if (GUILayout.Button(label, GUILayout.Width(46)))
         {
            return (EditorUtility.DisplayDialog("Confirm", "Clear " + label + " data?", "ok", "cancel"));
         }
         return false;
      }


      Vector2 scroll;
      void OnGUI()
      {
         
         if (Selection.activeGameObject == null)
         {
            EditorGUILayout.LabelField("No objects selected. Please select an object with a MeshFilter and Renderer");
            return;
         }

         if (swatches == null)
         {
            swatches = ColorSwatches.CreateInstance<ColorSwatches>();
            if (EditorPrefs.HasKey(sSwatchKey))
            {
               JsonUtility.FromJsonOverwrite(EditorPrefs.GetString(sSwatchKey), swatches);
            }
            if (swatches == null)
            {
               swatches = ColorSwatches.CreateInstance<ColorSwatches>();
               EditorPrefs.SetString(sSwatchKey, JsonUtility.ToJson(swatches, false));
            }
         }

         DrawChannelGUI();

         var ot = tab;
         tab = (Tab)GUILayout.Toolbar((int)tab, tabNames);
         if (ot != tab)
         {
            UpdateDisplayMode();
         }
         scroll = EditorGUILayout.BeginScrollView(scroll);
         if (tab == Tab.Paint)
         {
            DrawPaintGUI();
         }
         else if (tab == Tab.Deform)
         {
            DrawDeformGUI();
         }
         else if (tab == Tab.Flow)
         {
            DrawFlowGUI();
         }
         else if (tab == Tab.Bake)
         {
            DrawBakeGUI();
         }
         else if (tab == Tab.Custom)
         {
            DrawCustomGUI();
         }
         EditorGUILayout.EndScrollView();
      }


      void DrawChannelGUI()
      {
         EditorGUILayout.Separator();
         GUI.skin.box.normal.textColor = Color.white;
         GUILayout.Box("Vertex Painter", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(18)});
         EditorGUILayout.Separator();
         bool oldEnabled = enabled;
         if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp)
         {
            enabled = !enabled;
         }
         enabled = GUILayout.Toggle(enabled, "Active (ESC)");
         if (enabled != oldEnabled)
         {
            InitMeshes();
            UpdateDisplayMode();
         }
         var oldShow = showVertexShader;
         EditorGUILayout.BeginHorizontal();
         showVertexShader = GUILayout.Toggle(showVertexShader, "Show Vertex Data (ctrl-V)");
         if (oldShow != showVertexShader)
         {
            UpdateDisplayMode();
         }
         bool emptyStreams = false;
         for (int i = 0; i < jobs.Length; ++i)
         {
            if (!jobs[i].HasStream())
               emptyStreams = true;
         }
         if (emptyStreams)
         {
            if (GUILayout.Button("Add Streams"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  jobs[i].EnforceStream();
               }
               UpdateDisplayMode();
            }
         }
         EditorGUILayout.EndHorizontal();

         brushVisualization = (BrushVisualization)EditorGUILayout.EnumPopup("Brush Visualization", brushVisualization);

         showVertexPoints = GUILayout.Toggle(showVertexPoints, "Show Brush Influence");

         bool oldHideMeshWireframe = hideMeshWireframe;
         hideMeshWireframe = !GUILayout.Toggle(!hideMeshWireframe, "Show Wireframe (ctrl-W)");

         if (hideMeshWireframe != oldHideMeshWireframe)
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               EditorUtility.SetSelectedWireframeHidden(jobs[i].renderer, hideMeshWireframe);
            }
         }

         
         bool hasColors = false;
         bool hasUV0 = false;
         bool hasUV1 = false;
         bool hasUV2 = false;
         bool hasUV3 = false;
         bool hasPositions = false;
         bool hasNormals = false;
         bool hasStream = false;
         for (int i = 0; i < jobs.Length; ++i)
         {
            var stream = jobs[i]._stream;
            if (stream != null)
            {
               int vertexCount = jobs[i].verts.Length;
               hasStream = true;
               hasColors = (stream.colors != null && stream.colors.Length == vertexCount);
               hasUV0 = (stream.uv0 != null && stream.uv0.Count == vertexCount);
               hasUV1 = (stream.uv1 != null && stream.uv1.Count == vertexCount);
               hasUV2 = (stream.uv2 != null && stream.uv2.Count == vertexCount);
               hasUV3 = (stream.uv3 != null && stream.uv3.Count == vertexCount);
               hasPositions = (stream.positions != null && stream.positions.Length == vertexCount);
               hasNormals = (stream.normals != null && stream.normals.Length == vertexCount);
            }
         }

         if (hasStream && (hasColors || hasUV0 || hasUV1 || hasUV2 || hasUV3 || hasPositions || hasNormals))
         {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Clear Channel:");
            if (hasColors && DrawClearButton("Colors"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  var stream = jobs[i].stream;
                  stream.colors = null;
                  stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            if (hasUV0 && DrawClearButton("UV0"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  var stream = jobs[i].stream;
                  stream.uv0 = null;
                  stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            if (hasUV1 && DrawClearButton("UV1"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  var stream = jobs[i].stream;
                  stream.uv1 = null;
                  stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            if (hasUV2 && DrawClearButton("UV2"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  var stream = jobs[i].stream;
                  stream.uv2 = null;
                  stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            if (hasUV3 && DrawClearButton("UV3"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  var stream = jobs[i].stream;
                  stream.uv3 = null;
                  stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            if (hasPositions && DrawClearButton("Pos"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  jobs[i].stream.positions = null;
                  Mesh m = jobs[i].stream.GetModifierMesh();
                  if (m != null)
                     m.vertices = jobs[i].meshFilter.sharedMesh.vertices;
                  jobs[i].stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            if (hasNormals && DrawClearButton("Norm"))
            {
               for (int i = 0; i < jobs.Length; ++i)
               {
                  Undo.RecordObject(jobs[i].stream, "Vertex Painter Clear");
                  jobs[i].stream.normals = null;
                  jobs[i].stream.tangents = null;
                  jobs[i].stream.Apply();
               }
               Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }

            EditorGUILayout.EndHorizontal();
         }
         else if (hasStream)
         {
            if (GUILayout.Button("Remove Unused Stream Components"))
            {
               RevertMat();
               for (int i = 0; i < jobs.Length; ++i)
               {
                  if (jobs[i].HasStream())
                  {
                     DestroyImmediate(jobs[i].stream);
                  }
               }
               UpdateDisplayMode();
            }
         }


         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();
 
      }

      void DrawBrushSettingsGUI()
      {
         brushSize      = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 30.0f);
         brushFlow      = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 128.0f);
         brushFalloff   = EditorGUILayout.Slider("Brush Falloff", brushFalloff, 0.1f, 4.0f);
         if (tab == Tab.Paint && flowTarget != FlowTarget.ColorBA && flowTarget != FlowTarget.ColorRG)
         {
            flowRemap01 = EditorGUILayout.Toggle("use 0->1 mapping", flowRemap01);
         }
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();

      }


      void DrawCustomGUI()
      {

         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         customBrush = EditorGUILayout.ObjectField("Brush", customBrush, typeof(VertexPainterCustomBrush), false) as VertexPainterCustomBrush;

         DrawBrushSettingsGUI();

         if (customBrush != null)
         {
            customBrush.DrawGUI();
         }

         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               FillMesh(jobs[i]);
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }

         EditorGUILayout.EndHorizontal();

      }

      void DrawPaintGUI()
      {

         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         var oldBM = brushMode;
         brushMode = (BrushTarget)EditorGUILayout.EnumPopup("Target Channel", brushMode);
         if (oldBM != brushMode)
         {
            UpdateDisplayMode();
         }
         if (brushMode == BrushTarget.Color || brushMode == BrushTarget.UV0_AsColor || brushMode == BrushTarget.UV1_AsColor
            || brushMode == BrushTarget.UV2_AsColor || brushMode == BrushTarget.UV3_AsColor)
         {
            brushColorMode = (BrushColorMode)EditorGUILayout.EnumPopup("Blend Mode", (System.Enum)brushColorMode);

            if (brushColorMode == BrushColorMode.Overlay || brushColorMode == BrushColorMode.Normal)
            {
               brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

               if (GUILayout.Button("Reset Palette", EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(16)))
               {
                  if (swatches != null)
                  {
                     DestroyImmediate(swatches);
                  }
                  swatches = ColorSwatches.CreateInstance<ColorSwatches>();
                  EditorPrefs.SetString(sSwatchKey, JsonUtility.ToJson(swatches, false));
               }
            
               GUILayout.BeginHorizontal();

               for (int i = 0; i < swatches.colors.Length; ++i)
               {
                  if (GUILayout.Button("", EditorStyles.textField, GUILayout.Width(16), GUILayout.Height(16)))
                  {
                     brushColor = swatches.colors[i];
                  }
                  EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().x + 1, GUILayoutUtility.GetLastRect().y + 1, 14, 14), swatches.colors[i]);
               }
               GUILayout.EndHorizontal();
               GUILayout.BeginHorizontal();
               for (int i = 0; i < swatches.colors.Length; i++)
               {
                  if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(16), GUILayout.Height(12)))
                  {
                     swatches.colors[i] = brushColor;
                     EditorPrefs.SetString(sSwatchKey, JsonUtility.ToJson(swatches, false));
                  }
               }
               GUILayout.EndHorizontal();
            }
         }
         else if (brushMode == BrushTarget.ValueR || brushMode == BrushTarget.ValueG || brushMode == BrushTarget.ValueB || brushMode == BrushTarget.ValueA)
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
 
         DrawBrushSettingsGUI();
 
         //GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               FillMesh(jobs[i]);
            }
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
         }
         if (GUILayout.Button("Random"))
         {
            for (int i = 0; i < jobs.Length; ++i)
            {
               Undo.RecordObject(jobs[i].stream, "Vertex Painter Fill");
               RandomMesh(jobs[i]);
            }
         }
         EditorGUILayout.EndHorizontal();

      }

      void DrawDeformGUI()
      {
         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         pull = (Event.current.shift);

         vertexMode = (VertexMode)EditorGUILayout.EnumPopup("Vertex Mode", vertexMode);
         vertexContraint = (VertexContraint)EditorGUILayout.EnumPopup("Vertex Constraint", vertexContraint);

         DrawBrushSettingsGUI();

         EditorGUILayout.LabelField(pull ? "Pull (shift)" : "Push (shift)");

      }

      void DrawFlowGUI()
      {
         GUILayout.Box("Brush Settings", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         var oldV = flowVisualization;
         flowVisualization = (FlowVisualization)EditorGUILayout.EnumPopup("Visualize", flowVisualization);
         if (flowVisualization != oldV)
         {
            UpdateDisplayMode();
         }
         var ft = flowTarget;
         flowTarget = (FlowTarget)EditorGUILayout.EnumPopup("Target", flowTarget);
         if (flowTarget != ft)
         {
            UpdateDisplayMode();
         }
         flowBrushType = (FlowBrushType)EditorGUILayout.EnumPopup("Mode", flowBrushType);

         DrawBrushSettingsGUI();
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         
         
         
         if (GUILayout.Button("Reset"))
         {
            Vector2 norm = new Vector2(0.5f, 0.5f);
            
            foreach (PaintJob job in jobs)
            {
               PrepBrushMode(job);
               switch (flowTarget)
               {
                  case FlowTarget.ColorRG:
                     job.stream.SetColorRG(norm, job.verts.Length); break;
                  case FlowTarget.ColorBA:
                     job.stream.SetColorBA(norm, job.verts.Length); break;
                  case FlowTarget.UV0_XY:
                     job.stream.SetUV0_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV0_ZW:
                     job.stream.SetUV0_ZW(norm, job.verts.Length); break;
                  case FlowTarget.UV1_XY:
                     job.stream.SetUV1_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV1_ZW:
                     job.stream.SetUV1_ZW(norm, job.verts.Length); break;
                  case FlowTarget.UV2_XY:
                     job.stream.SetUV2_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV2_ZW:
                     job.stream.SetUV2_ZW(norm, job.verts.Length); break;
                  case FlowTarget.UV3_XY:
                     job.stream.SetUV3_XY(norm, job.verts.Length); break;
                  case FlowTarget.UV3_ZW:
                     job.stream.SetUV3_ZW(norm, job.verts.Length); break;
               }
            }
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
         
      }


      void DrawBakeGUI()
      {
         GUILayout.Box("Ambient Occlusion", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         brushMode = (BrushTarget)EditorGUILayout.EnumPopup("Target Channel", brushMode);
         aoSamples = EditorGUILayout.IntSlider("Samples", aoSamples, 64, 1024);
         EditorGUILayout.BeginHorizontal();
         aoRange = EditorGUILayout.Vector2Field("Range (Min, Max)", aoRange);
         aoRange.x = Mathf.Max(aoRange.x, 0.0001f);
         EditorGUILayout.EndHorizontal();
         aoIntensity = EditorGUILayout.Slider("Intensity", aoIntensity, 0.25f, 4.0f);
         bakeLighting = EditorGUILayout.Toggle("Bake Lighting", bakeLighting);
         if (bakeLighting)
         {
            aoLightAmbient = EditorGUILayout.ColorField("Light Ambient", aoLightAmbient);
         }
         aoBakeMode = (AOBakeMode)EditorGUILayout.EnumPopup("Mode", aoBakeMode);


         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake"))
         {
            BakeAO();
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();

         GUILayout.Space(10);
         GUILayout.Box("Bake From Texture", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         bakingTex = EditorGUILayout.ObjectField("Texture", bakingTex, typeof(Texture2D), false) as Texture2D;

         bakeSourceUV = (BakeSourceUV)EditorGUILayout.EnumPopup("Source UVs", bakeSourceUV);
         bakeChannel = (BakeChannel)EditorGUILayout.EnumPopup("Bake To", bakeChannel);
         if (bakeSourceUV == BakeSourceUV.WorldSpaceXY || bakeSourceUV == BakeSourceUV.WorldSpaceXZ || bakeSourceUV == BakeSourceUV.WorldSpaceYZ)
         {
            worldSpaceLower = EditorGUILayout.Vector2Field("Lower world position", worldSpaceLower);
            worldSpaceUpper = EditorGUILayout.Vector2Field("Upper world position", worldSpaceUpper);
         }
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake"))
         {
            if (bakingTex != null)
            {
               BakeFromTexture();
            }
            else
            {
               EditorUtility.DisplayDialog("Error", "Baking texture is not set", "ok");
            }
         }
         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();

         GUILayout.Space(10);
         GUILayout.Box("Bake Pivot", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         pivotTarget = (PivotTarget)EditorGUILayout.EnumPopup("Store in", pivotTarget);
         bakePivotUseLocal = EditorGUILayout.Toggle("Use Local Space", bakePivotUseLocal);

         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Bake Pivot"))
         {
            BakePivot();
         }
         if (GUILayout.Button("Bake Rotation"))
         {
            BakeRotation();
         }

         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
         GUILayout.Space(10);
         GUILayout.Box("Mesh Combiner", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Combine Meshes"))
         {
            MergeMeshes();
         }
         if (GUILayout.Button("Combine and Save"))
         {
            if (jobs.Length != 0)
            {
               string path = EditorUtility.SaveFilePanel("Save Asset", Application.dataPath, "models", "asset");
               if (!string.IsNullOrEmpty(path))
               {
                  path = FileUtil.GetProjectRelativePath(path);
                  GameObject go = MergeMeshes();
                  Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
                  AssetDatabase.CreateAsset(m, path);
                  AssetDatabase.SaveAssets();
                  AssetDatabase.ImportAsset(path);
                  DestroyImmediate(go);
               }
            }
         }
         EditorGUILayout.EndHorizontal();
         GUILayout.Space(10);
         GUILayout.Box("Mesh Save", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(20)});
         EditorGUILayout.BeginHorizontal();
         EditorGUILayout.Space();
         if (GUILayout.Button("Save Mesh"))
         {
            SaveMesh();
         }

         EditorGUILayout.Space();
         EditorGUILayout.EndHorizontal();
      }

      void SaveMesh()
      {
         if (jobs.Length != 0)
         {
            string path = EditorUtility.SaveFilePanel("Save Asset", Application.dataPath, "models", "asset");
            if (!string.IsNullOrEmpty(path))
            {
               path = FileUtil.GetProjectRelativePath(path);
               Mesh firstMesh = BakeDownMesh(jobs[0].meshFilter.sharedMesh, jobs[0].stream);
               
               AssetDatabase.CreateAsset(firstMesh, path);
               
               for (int i = 1; i < jobs.Length; ++i)
               {
                  Mesh m = BakeDownMesh(jobs[i].meshFilter.sharedMesh, jobs[i].stream);
                  AssetDatabase.AddObjectToAsset(m, firstMesh);
               }
               AssetDatabase.SaveAssets();
               AssetDatabase.ImportAsset(path);
            }
         }
      }

      // copy a mesh, and bake it's vertex stream into the mesh data. 
      Mesh BakeDownMesh(Mesh mesh, VertexInstanceStream stream)
      {
         var copy = Instantiate(mesh);
         /*
         foreach(var property in typeof(Mesh).GetProperties())
         {
            if(property.GetSetMethod() != null && property.GetGetMethod() != null)
            {
               property.SetValue(copy, property.GetValue(mesh, null), null);
            }
         }
         copy.hideFlags = 0;
*/

         copy.colors = stream.colors;
         if (stream.uv0 != null && stream.uv0.Count > 0) { copy.SetUVs(0, stream.uv0); }
         if (stream.uv1 != null && stream.uv1.Count > 0) { copy.SetUVs(1, stream.uv1); }
         if (stream.uv2 != null && stream.uv2.Count > 0) { copy.SetUVs(2, stream.uv2); }
         if (stream.uv3 != null && stream.uv3.Count > 0) { copy.SetUVs(3, stream.uv3); }

         if (stream.positions != null && stream.positions.Length == copy.vertexCount)
         {
            copy.vertices = stream.positions;
         }
         if (stream.normals != null && stream.normals.Length == copy.vertexCount)
         {
            copy.normals = stream.normals;
         }
         if (stream.tangents != null && stream.tangents.Length == copy.vertexCount)
         {
            copy.tangents = stream.tangents;
         }
         copy.Optimize();
         copy.RecalculateBounds();
         copy.UploadMeshData(false);

         return copy;
      }

      GameObject MergeMeshes()
      {
         if (jobs.Length == 0)
            return null;
         List<CombineInstance> meshes = new List<CombineInstance>();
         for (int i = 0; i < jobs.Length; ++i)
         {
            Mesh m = BakeDownMesh(jobs[i].meshFilter.sharedMesh, jobs[i].stream);
            CombineInstance ci = new CombineInstance();
            ci.mesh = m;
            ci.transform = jobs[i].meshFilter.transform.localToWorldMatrix;
            meshes.Add(ci);
         }

         Mesh mesh = new Mesh();
         mesh.CombineMeshes(meshes.ToArray());
         GameObject go = new GameObject("Combined Mesh");
         go.AddComponent<MeshRenderer>();
         var mf = go.AddComponent<MeshFilter>();
         mesh.Optimize();
         mesh.RecalculateBounds();
         mesh.UploadMeshData(false);
         mf.sharedMesh = mesh;
         for (int i = 0; i < meshes.Count; ++i)
         {
            DestroyImmediate(meshes[i].mesh);
         }
         return go;
      }

      void OnFocus() 
      {
         if (painting)
         {
            EndStroke();
         }

         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;
         this.titleContent = new GUIContent("Vertex Paint");
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
         DestroyImmediate(VertexInstanceStream.vertexShaderMat);
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
      }
   }
}
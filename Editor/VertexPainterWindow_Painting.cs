using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;


namespace JBooth
{
   public partial class VertexPainterWindow : EditorWindow 
   {
   	public bool          enabled;
   	public float         brushSize = 1;
   	public float         brushFlow = 8;
   	public Color         brushColor = Color.red;
   	public int           brushValue = 255;
      public float         floatBrushValue = 1.0f;
      public Vector2       uvVisualizationRange = new Vector2(0,1);

   	public enum BrushMode
   	{
   		Color = 0,
   		ValueR,
   		ValueG,
   		ValueB,
   		ValueA,
         U0,
         V0,
         U1,
         V1,
         U2,
         V2,
         U3,
         V3
   	}

   	public BrushMode brushMode = BrushMode.Color;
      public bool  showVertexShader = false;

   	public Material vertexShaderMat;

   	[MenuItem("Window/Vertex Painter")]
   	public static void ShowWindow()
   	{
   		var window = GetWindow<VertexPainterWindow>();
   		window.InitMeshes();
   		window.Show();
   	}

   	public class PaintJob
   	{
   		public MeshFilter meshFilter;
   		public Renderer renderer;
         public VertexInstanceStream _stream;
   		public Material originalMat;
   		public Vector3[] verts;

         public VertexInstanceStream stream
         {
            get
            {
               if (_stream == null)
               {
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

   		public PaintJob(MeshFilter mf, Renderer r)
   		{
   			meshFilter = mf;
   			renderer = r;
   			originalMat = r.sharedMaterial;
   			verts = mf.sharedMesh.vertices;

   		}
   	}

   	public PaintJob[] jobs = new PaintJob[0];

   	void InitMeshes()
   	{
         // revert old materials
         for (int i = 0; i < jobs.Length; ++i)
         {
            jobs[i].renderer.sharedMaterial = jobs[i].originalMat;
         }

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
   		if (vertexShaderMat == null)
   		{
   			vertexShaderMat = new Material(Shader.Find("Hidden/VertexColor"));
   		}
   		for (int i = 0; i < jobs.Length; ++i)
   		{
   			if (!showVertexShader)
   			{
   				jobs[i].renderer.sharedMaterial = jobs[i].originalMat;
   			}
   			else
   			{
   				jobs[i].renderer.sharedMaterial = vertexShaderMat;
   				vertexShaderMat.SetFloat("_channel", (int)brushMode);
               vertexShaderMat.SetVector("_uvRange", uvVisualizationRange);
   			}
   		}
   	}

      void SetValue(VertexInstanceStream stream, int i)
      {
         float bv = (float)brushValue/255.0f;
         switch (brushMode)
         {
            case BrushMode.Color:
            {
               stream.colors[i] = brushColor;
               break;
            }
            case BrushMode.ValueR:
            {
               stream.colors[i].r = bv;
               break;
            }
            case BrushMode.ValueG:
            {
               stream.colors[i].g = bv;
               break;
            }
            case BrushMode.ValueB:
            {
               stream.colors[i].b = bv;
               break;
            }
            case BrushMode.ValueA:
            {
               stream.colors[i].a = bv;
               break;
            }
            case BrushMode.U0:
            {
               stream.uv0[i].x = floatBrushValue;
               break;
            }
            case BrushMode.V0:
            {
               stream.uv0[i].y = floatBrushValue;
               break;
            }
            case BrushMode.U1:
            {
               stream.uv1[i].x = floatBrushValue;
               break;
            }
            case BrushMode.V1:
            {
               stream.uv1[i].y = floatBrushValue;
               break;
            } 
            case BrushMode.U2:
            {
               stream.uv2[i].x = floatBrushValue;
               break;
            }
            case BrushMode.V2:
            {
               stream.uv2[i].y = floatBrushValue;
               break;
            }
            case BrushMode.U3:
            {
               stream.uv3[i].x = floatBrushValue;
               break;
            }
            case BrushMode.V3:
            {
               stream.uv3[i].y = floatBrushValue;
               break;
            } 
         }
      }

      void OnUndo()
      {
         for (int i = 0; i < jobs.Length; ++i)
         {
            jobs[i].stream.Apply();
         }
      }

      void FillMesh(PaintJob job)
      {
         PrepBrushMode(job);
         for (int i = 0; i < job.stream.colors.Length; ++i)
         {
            SetValue(job.stream, i);
         }
         job.stream.Apply();
      }

      void PrepBrushMode(PaintJob j)
      {
         // make sure the instance data is initialized
         switch (brushMode)
         {
            case BrushMode.Color: goto case BrushMode.ValueA;
            case BrushMode.ValueR:goto case BrushMode.ValueA;
            case BrushMode.ValueG:goto case BrushMode.ValueA;
            case BrushMode.ValueB:goto case BrushMode.ValueA;
            case BrushMode.ValueA:
            {
               Color[] colors = j.stream.colors;
               if (colors == null || colors.Length != j.verts.Length)
               {
                  j.stream.SetColor(Color.white, j.verts.Length);
                  colors = j.stream.colors;
               }
               break;
            }
            case BrushMode.U0:goto case BrushMode.V0;
            case BrushMode.V0:
            {
               Vector2[] uvs = j.stream.uv0;
               if (uvs == null || uvs.Length != j.verts.Length)
               {
                  if (j.meshFilter.sharedMesh.uv != null && j.meshFilter.sharedMesh.uv.Length == j.verts.Length)
                  {
                     j.stream.uv0 = new Vector2[j.meshFilter.sharedMesh.uv.Length];
                     j.meshFilter.sharedMesh.uv.CopyTo(j.stream.uv0, 0);
                  }
                  else
                  {
                     j.stream.SetUV0(Vector2.zero, j.verts.Length);
                  }
               }
               break;
            }
            case BrushMode.U1: goto case BrushMode.V1;
            case BrushMode.V1:
            {
               Vector2[] uvs = j.stream.uv1;
               if (uvs == null || uvs.Length != j.verts.Length)
               {
                  if (j.meshFilter.sharedMesh.uv2 != null && j.meshFilter.sharedMesh.uv2.Length == j.verts.Length)
                  {
                     j.stream.uv1 = new Vector2[j.meshFilter.sharedMesh.uv2.Length];
                     j.meshFilter.sharedMesh.uv2.CopyTo(j.stream.uv1, 0);
                  }
                  else
                  {
                     j.stream.SetUV1(Vector2.zero, j.verts.Length);
                  }
               }
               break;
            }
            case BrushMode.U2: goto case BrushMode.V2;
            case BrushMode.V2:
            {
               Vector2[] uvs = j.stream.uv2;
               if (uvs == null || uvs.Length != j.verts.Length)
               {
                  if (j.meshFilter.sharedMesh.uv3 != null && j.meshFilter.sharedMesh.uv3.Length == j.verts.Length)
                  {
                     j.stream.uv2 = new Vector2[j.meshFilter.sharedMesh.uv3.Length];
                     j.meshFilter.sharedMesh.uv3.CopyTo(j.stream.uv2, 0);
                  }
                  else
                  {
                     j.stream.SetUV2(Vector2.zero, j.verts.Length);
                  }
               }
               break;
            }
            case BrushMode.U3: goto case BrushMode.V3;
            case BrushMode.V3:
            {
               Vector2[] uvs = j.stream.uv3;
               if (uvs == null || uvs.Length != j.verts.Length)
               {
                  if (j.meshFilter.sharedMesh.uv4 != null && j.meshFilter.sharedMesh.uv4.Length == j.verts.Length)
                  {
                     j.stream.uv3 = new Vector2[j.meshFilter.sharedMesh.uv4.Length];
                     j.meshFilter.sharedMesh.uv4.CopyTo(j.stream.uv3, 0);
                  }
                  else
                  {
                     j.stream.SetUV3(Vector2.zero, j.verts.Length);
                  }
               }
               break;
            }
         }
      }

   	void PaintMesh(PaintJob j, Vector3 point)
   	{
         PrepBrushMode(j);
   		// convert point into local space, so we don't have to convert every point
   		point = j.renderer.transform.worldToLocalMatrix.MultiplyPoint(point);

   		// we could do a spacial hash for more speed
   		for (int i = 0; i < j.verts.Length; ++i)
   		{
   			float d = Vector3.Distance(point, j.verts[i]);
   			if (d < brushSize)
   			{
   				float str = 1.0f - d/brushSize;
               float pressure = Event.current.pressure > 0 ? Event.current.pressure : 1.0f;
               PaintVert(j, i, str * (float)deltaTime * brushFlow * pressure);
   			}
   		}
         j.stream.Apply();
   	}

  
   	void PaintVert(PaintJob j, int i, float strength)
   	{
         float bv = (float)brushValue/255.0f;
   		switch (brushMode)
   		{
   			case BrushMode.Color:
   			{
   				j.stream.colors[i] = Color.Lerp(j.stream.colors[i], brushColor, strength);
               break;
   			}
   			case BrushMode.ValueR:
   			{
               j.stream.colors[i].r = Mathf.Lerp (j.stream.colors[i].r, bv, strength);
               break;
   			}
   			case BrushMode.ValueG:
   			{
               j.stream.colors[i].g = Mathf.Lerp (j.stream.colors[i].g, bv, strength);
               break;
   			}
   			case BrushMode.ValueB:
   			{
               j.stream.colors[i].b = Mathf.Lerp (j.stream.colors[i].b, bv, strength);
               break;
   			}
   			case BrushMode.ValueA:
   			{
               j.stream.colors[i].a = Mathf.Lerp (j.stream.colors[i].a, bv, strength);
               break;
   			}
            case BrushMode.U0:
            {
               j.stream.uv0[i].x = Mathf.Lerp(j.stream.uv0[i].x, floatBrushValue, strength);
               break;
            }
            case BrushMode.V0:
            {
               j.stream.uv0[i].y = Mathf.Lerp(j.stream.uv0[i].y, floatBrushValue, strength);
               break;
            }
            case BrushMode.U1:
            {
               j.stream.uv1[i].x = Mathf.Lerp(j.stream.uv1[i].x, floatBrushValue, strength);
               break;
            }
            case BrushMode.V1:
            {
               j.stream.uv1[i].y = Mathf.Lerp(j.stream.uv1[i].y, floatBrushValue, strength);
               break;
            }
            case BrushMode.U2:
            {
               j.stream.uv2[i].x = Mathf.Lerp(j.stream.uv2[i].x, floatBrushValue, strength);
               break;
            }
            case BrushMode.V2:
            {
               j.stream.uv2[i].y = Mathf.Lerp(j.stream.uv2[i].y, floatBrushValue, strength);
               break;
            }
            case BrushMode.U3:
            {
               j.stream.uv3[i].x = Mathf.Lerp(j.stream.uv3[i].x, floatBrushValue, strength);
               break;
            }
            case BrushMode.V3:
            {
               j.stream.uv3[i].y = Mathf.Lerp(j.stream.uv3[i].y, floatBrushValue, strength);
               break;
            }
            
      	}
   	}
      
      double deltaTime = 0;
   	double lastTime = 0;
   	bool painting = false;

   	List<PaintJob> stroke = new List<PaintJob>();
   	void OnSceneGUI(SceneView sceneView) 
   	{
   		stroke.Clear();
   		deltaTime = EditorApplication.timeSinceStartup - lastTime;
   		lastTime = EditorApplication.timeSinceStartup;

   		if (jobs.Length == 0 && Selection.activeGameObject != null)
   		{
   			InitMeshes();
   		}

   		if (!enabled || jobs.Length == 0 || Selection.activeGameObject == null)
   			return;

   		RaycastHit hit;
   		float distance = float.MaxValue;
   		Vector3 mousePosition = Event.current.mousePosition;
   		mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;

   		Vector3 fakeMP = mousePosition;
   		fakeMP.z = 20;
   		Vector3 point = sceneView.camera.ScreenToWorldPoint(fakeMP);
   		Ray ray = sceneView.camera.ScreenPointToRay(mousePosition);
   		for (int i = 0; i < jobs.Length; ++i)
   		{
   			if (RXLookingGlass.IntersectRayMesh(ray, jobs[i].meshFilter, out hit))
   			{
   				stroke.Add(jobs[i]);
   				if (hit.distance < distance)
   				{
   					distance = hit.distance;
   					point = hit.point;
   				}
   			}
   		}

   		if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.alt == false)
   		{
   			painting = true;
   		}
   		if (Event.current.type == EventType.MouseUp)
   		{
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
   			painting = false;
   		}
   		if (Event.current.type == EventType.MouseMove && Event.current.alt)
   		{
   			brushSize += Event.current.delta.y * (float)deltaTime;
   		}
   		if (stroke.Count > 0 && painting)
   		{
   			for (int i = 0; i < stroke.Count; ++i)
   			{
               Undo.RecordObject(stroke[i].stream, "Vertex Painter Stroke");
   				PaintMesh(stroke[i], point);
   			}
   		}


   		// set brush color
         if (brushMode == BrushMode.Color)
         {
            Handles.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.4f);
         }
         else if (brushMode == BrushMode.ValueR || brushMode == BrushMode.ValueG || 
                  brushMode == BrushMode.ValueB || brushMode == BrushMode.ValueA)
         {
            float v = (float)brushValue/255.0f;
            Handles.color = new Color(v, v, v, 0.4f);
         }
         else
         {
            float v = (floatBrushValue-uvVisualizationRange.x) / Mathf.Max(0.00001f, uvVisualizationRange.y);
            Handles.color = new Color(v, v, v, 0.4f);
         }

         // draw brush
   		Handles.SphereCap(0, point, Quaternion.identity, brushSize*2);
   		
         // eat current event if mouse event and we're painting
   		if (Event.current.isMouse && painting)
   			Event.current.Use (); 

   		if (Event.current.type == EventType.layout)
   			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

         // update views
   		HandleUtility.Repaint();
   		sceneView.Repaint();
   	}
   }
}

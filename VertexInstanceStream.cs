using UnityEngine;
using System.Collections;
using System;

/* Holds streams of data to override the colors or UVs on a mesh without making the mesh unique. This is more
 * memory efficient than burning the color data into many copies of a mesh, and much easier to manage. 
 * 
*/

[ExecuteInEditMode]
public class VertexInstanceStream : MonoBehaviour 
{
   [HideInInspector]
   [SerializeField]
   private Color[] _colors;

   [HideInInspector]
   [SerializeField]
   private Vector2[] _uv0;

   [HideInInspector]
   [SerializeField]
   private Vector2[] _uv1;

   [HideInInspector]
   [SerializeField]
   private Vector2[] _uv2;

   [HideInInspector]
   [SerializeField]
   private Vector2[] _uv3;

   public Color[] colors 
   { 
      get 
      { 
         return _colors; 
      }
      set
      {
         _colors = value;
         Apply();
      }
   }

   public Vector2[] uv0 { get { return _uv0; } set { _uv0 = value; Apply(); } }
   public Vector2[] uv1 { get { return _uv1; } set { _uv1 = value; Apply(); } }
   public Vector2[] uv2 { get { return _uv2; } set { _uv2 = value; Apply(); } }
   public Vector2[] uv3 { get { return _uv3; } set { _uv3 = value; Apply(); } }

	void Start()
   {
      Apply();
   }

   public void SetColor(Color[] iColors)
   {
      _colors = new Color[iColors.Length];
      iColors.CopyTo(_colors, 0);
      Apply();
   }

   public void SetColor(Color c, int count)
   {
      _colors = new Color[count];
      for (int i = 0; i < count; ++i)
      {
         _colors[i] = c;
      }
      Apply();
   }

   public void SetUV0(Vector2[] uvs) { _uv0 = new Vector2[uvs.Length]; uvs.CopyTo(_uv0, 0); Apply(); }
   public void SetUV0(Vector2 uv, int count) { _uv0 = new Vector2[count]; for (int i = 0; i < count; ++i) { _uv0[i] = uv; } Apply(); }
   public void SetUV1(Vector2[] uvs) { _uv1 = new Vector2[uvs.Length]; uvs.CopyTo(_uv1, 0); Apply(); }
   public void SetUV1(Vector2 uv, int count) { _uv1 = new Vector2[count]; for (int i = 0; i < count; ++i) { _uv1[i] = uv; } Apply(); }
   public void SetUV2(Vector2[] uvs) { _uv2 = new Vector2[uvs.Length]; uvs.CopyTo(_uv2, 0); Apply(); }
   public void SetUV2(Vector2 uv, int count) { _uv2 = new Vector2[count]; for (int i = 0; i < count; ++i) { _uv2[i] = uv; } Apply(); }
   public void SetUV3(Vector2[] uvs) { _uv3 = new Vector2[uvs.Length]; uvs.CopyTo(_uv3, 0); Apply(); }
   public void SetUV3(Vector2 uv, int count) { _uv3 = new Vector2[count]; for (int i = 0; i < count; ++i) { _uv3[i] = uv; } Apply(); }

   public void Apply()
   {
      MeshRenderer mr = GetComponent<MeshRenderer>();
      MeshFilter mf = GetComponent<MeshFilter>();

      if (mr != null && mf != null)
      {
         Mesh m = new Mesh();
         m.vertices = mf.sharedMesh.vertices;

         if (_colors != null && _colors.Length == mf.sharedMesh.vertexCount)
         {
            // workaround for unity bug; dispite docs claim, color channels must exist on the original mesh
            // for the additionalVertexStream to work.
            
            Color[] orig = new Color[mf.sharedMesh.vertexCount];
            for (int i = 0; i < orig.Length; ++i)
            {
               orig[i] = Color.white;
            }
            mf.sharedMesh.colors = orig;
            
            
            m.colors = _colors;
         }

         if (_uv0 != null && _uv0.Length == mf.sharedMesh.vertexCount) { m.uv = _uv0; }
         if (_uv1 != null && _uv1.Length == mf.sharedMesh.vertexCount) { m.uv2 = _uv1; }
         if (_uv2 != null && _uv2.Length == mf.sharedMesh.vertexCount) { m.uv3 = _uv2; }
         if (_uv3 != null && _uv3.Length == mf.sharedMesh.vertexCount) { m.uv4 = _uv3; }


         mr.additionalVertexStreams = m;
         m.UploadMeshData(true);

         // another Unity bug, when in editor, the paint job will just disapear sometimes. So we have to re-assign
         // it every update (even though this doesn't get called each frame, it appears to loose the data during
         // the editor update call, which only happens occationaly. 
#if UNITY_EDITOR
         stream = m;
#endif
      }
   }

   // hack around unity bugs in the editor..
#if UNITY_EDITOR
   private Mesh stream;
   void Update()
   {
      if (!Application.isPlaying)
      {
         MeshRenderer r = GetComponent<MeshRenderer>();
         r.additionalVertexStreams = stream;
      }
   }
#endif
}

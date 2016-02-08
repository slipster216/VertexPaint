using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SplatMapShaderGUI : ShaderGUI 
{

   void DrawLayer(MaterialEditor editor, int i, MaterialProperty[] props, string[] keyWords, bool hasGloss, bool isParallax, bool hasEmis)
   {
      EditorGUIUtility.labelWidth = 0f;
      var albedoMap = FindProperty ("_Tex" + i, props);
      var normalMap = FindProperty ("_Normal" + i, props);
      var smoothness = FindProperty("_Glossiness" + i, props);
      var glossinessMap = FindProperty("_GlossinessTex" + i, props);
      var metallic = FindProperty("_Metallic" + i, props);
      var emissionTex = FindProperty("_Emissive" + i, props);
      var emissionMult = FindProperty("_EmissiveMult" + i, props);
      var parallax = FindProperty("_Parallax" + i, props);
      var texScale = FindProperty("_TexScale" + i, props);

      //editor.TexturePropertySingleLine("Albedo (RGB) Height (A)", albedoMap);
      editor.TexturePropertySingleLine(new GUIContent("Albedo/Height"), albedoMap);
      editor.TexturePropertySingleLine(new GUIContent("Normal"), normalMap);
      editor.TexturePropertySingleLine(new GUIContent("Metal(R)/Smoothness(A)"), glossinessMap);
      if (!hasGloss)
      { 
         editor.ShaderProperty(smoothness, "Smoothness");
         editor.ShaderProperty(metallic, "Metallic");
      }
      editor.TexturePropertySingleLine(new GUIContent("Emission"), emissionTex);
      editor.ShaderProperty(emissionMult, "Emissive Multiplier");

      editor.ShaderProperty(texScale, "Texture Scale");

      if (isParallax)
      {
         editor.ShaderProperty(parallax, "Parallax Height");
      }

      if (i != 1)
      {
         editor.ShaderProperty(FindProperty("_Contrast"+i, props), "Interpolation Contrast");
      }
   }

   enum FlowChannel
   {
      None = 0,
      One,
      Two,
      Three,
      Four,
      Five
   }

   string[] flowChannelNames = new string[]
   {
      "None", "One", "Two", "Three", "Four", "Five"
   };

   public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] props)
   {
      // get the current keywords from the material
      Material targetMat = materialEditor.target as Material;
      string[] keyWords = targetMat.shaderKeywords;

      bool parallax = keyWords.Contains ("_PARALLAXMAP");
      int layerCount = 1;
      if (targetMat.shader.name == "VertexPainter/SplatBlend_2Layer")
         layerCount = 2;
      else if (targetMat.shader.name == "VertexPainter/SplatBlend_3Layer")
         layerCount = 3;
      else if (targetMat.shader.name == "VertexPainter/SplatBlend_4Layer")
         layerCount = 4;
      else if (targetMat.shader.name == "VertexPainter/SplatBlend_5Layer")
         layerCount = 5;

      FlowChannel fchannel = FlowChannel.None;
      if (keyWords.Contains("_FLOW1"))
         fchannel = FlowChannel.One;
      if (keyWords.Contains("_FLOW2"))
         fchannel = FlowChannel.Two;
      if (keyWords.Contains("_FLOW3"))
         fchannel = FlowChannel.Three;
      if (keyWords.Contains("_FLOW4"))
         fchannel = FlowChannel.Four;
      if (keyWords.Contains("_FLOW5"))
         fchannel = FlowChannel.Five;

      bool flowDrift = keyWords.Contains("_FLOWDRIFT");
      bool hasGloss = (HasTexture(layerCount, targetMat, "_GlossinessTex"));
      bool hasEmis = (HasTexture(layerCount, targetMat, "_Emissive"));

      EditorGUI.BeginChangeCheck();
      int oldLayerCount = layerCount;
      layerCount = EditorGUILayout.IntField("Layer Count", layerCount);
      if (oldLayerCount != layerCount)
      {
         if (layerCount < 1)
            layerCount = 1;
         if (layerCount > 5)
            layerCount = 5;

         targetMat.shader = Shader.Find("VertexPainter/SplatBlend_" + layerCount + "Layer");
         return;
      }


      parallax = EditorGUILayout.Toggle ("Parallax Offset", parallax);

      for (int i = 0; i < layerCount; ++i)
      {
         DrawLayer(materialEditor, i+1, props, keyWords, hasGloss, parallax, hasEmis);

         EditorGUILayout.Space();
      }

      EditorGUILayout.Space();

      fchannel = (FlowChannel)EditorGUILayout.Popup((int)fchannel, flowChannelNames);
      if (fchannel != FlowChannel.None)
      {

         var flowSpeed = FindProperty("_FlowSpeed", props);
         var flowIntensity = FindProperty("_FlowIntensity", props);

         materialEditor.ShaderProperty(flowSpeed, "Flow Speed");
         materialEditor.ShaderProperty(flowIntensity, "Flow Intensity");
         flowDrift = EditorGUILayout.Toggle("Flow Drift", flowDrift);
      }

      if (EditorGUI.EndChangeCheck())
      {
         var newKeywords = new List<string>();

         newKeywords.Add("_LAYERS" + layerCount.ToString());
         if (parallax) 
         {
            newKeywords.Add("_PARALLAXMAP");
         }
         if (HasTexture(layerCount, targetMat, "_Normal"))
         {
            newKeywords.Add("_NORMALMAP");
         }
         if (hasGloss)
         {
            newKeywords.Add("_METALLICGLOSSMAP");
         }
         if (hasEmis)
         {
            newKeywords.Add("_EMISSION"); 
         }

         if (fchannel != FlowChannel.None)
         {
            newKeywords.Add("_FLOW" + (int)fchannel);
         }

         if (flowDrift)
         {
            newKeywords.Add("_FLOWDRIFT");
         }
         targetMat.shaderKeywords = newKeywords.ToArray ();
         EditorUtility.SetDirty (targetMat);
      }
   } 

   bool HasTexture(int numLayers, Material mat, string key)
   {
      for (int i = 0; i < numLayers; ++i)
      {
         int index = i+1;
         if (mat.GetTexture(key + index.ToString()) != null)
            return true;
      }
      return false;
   }

}

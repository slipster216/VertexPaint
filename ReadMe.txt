Vertex Painter for Unity
©2015 Jason Booth
Tested with Unity 5.3.2p2

Example Video:

quick tutorial:
https://www.youtube.com/watch?v=mtbCtwgI440&feature=youtu.be
water and refraction:
https://www.youtube.com/watch?v=V70GQjOH8_Y&feature=youtu.be

Donate: https://pledgie.com/campaigns/31113

Additional Vertex Streams

	This package allows you to paint information onto the vertices of a mesh in the Unity editor as well as modify any attribute of the mesh. It uses the new 'additionalVertexStream' system of Unity5, which allows you to override per-instance data on meshes without paying the cost of duplicating a full mesh. This makes it ideal for painting vertex information across many instances of a mesh. The tool also allows you to easily bake that information back to mesh assets if you'd prefer to make a modified mesh and store that in disk, instead of with the instance in the scene. 

Features
	The toolset contains several different painting modes as well as a few tools that come in handy when doing this type of work. You can modify the positions, normals, UVs and colors of a mesh, painting colors, values, or direction vectors taken from the stroke. You can bake lighting or ambient occlusion data into the vertices, or bake any of the changes you’ve made into a new mesh asset on disk. 


Precision

	Different channels of mesh data support different levels of precision. For instance, the color channel stores a color in 8888 format, which means you can only store 0 to 1 values with 256 possible values in each component of the color channel. The 4 UV channels, however, provide 4 32-bit floats each. It is important to understand these limitations when working with vertex data. 

Interface


Paint
	This tool allows you to paint information directly onto the vertices Color or UV channels. Data can be viewed as either color information or greyscale information if you are working on individual channels.

Deform
	This tool allows you to modify the vertices positions, properly recalculating normals and tangents. 

Flow

	This tool allows you to paint directional vectors into the color or UV channels, which can be used to create flowing effects in shaders. Note that the tool currently computes direction with a dot product of the tangent and bitangent of your mesh- this means that if your tangents are not in the expected space, the flow direction may be computed wrong.

Bake
	This tab contains a number of utilities, such as the ability to bake AO and lighting into the mesh data. You can also bake information from a texture into your vertices, bake pivot points into the UV channels of the mesh (allowing you to combine many objects and animate them individually in the shader). Finally, you can save meshes as disk assets from this tab.

Shaders

	Included with the package is a ‘SplatBlend’ shader. The SplatBlend shader is closely mirrors Unity’s Standard Shader, but allows you to blend up to 5 different textures together per vertex. The blending is handled via a hight map stored in the alpha channel of the diffuse map, to produce a natural looking blend that allows sand to appear in the cracks of the stone before covering the stone, etc. Values painted into the color channels of the vertices control how much of each texture appears at each vertex. The Specular, Emissive, Metallic, and Normal data is handled in the same manner as with the Standard Shader. 

Each layer has controls for texture scaling, parallax height, and a contrast which controls the width of the blending area between each layer. 

Finally, the shader allows you to designate one layer of the shader to flow mode. This distorts the texture along directional vectors painted into the third UV set (The first UV set is used for your UV mapping, and Unity uses the second UV set for enlighten data). This is useful to create flowing water, lava, or other effects. The flow layer contains controls for speed and intensity, allowing you to modify the effect globally across the material. The flow layer can optionally use alpha and refraction to distort the layers below it. The normal map will be used as the refraction direction.  

The performance of this shader is highly variable based on how many layers you use and which features you enable. Features which are not used in any layers are compiled out; while a feature used on any layer is computed for every layer. In other words, if you don’t use emissive textures on any layer you won’t pay for the feature, but if you use it on one layer an emissive value will be sampled for every layer.


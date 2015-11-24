Vertex Painter for Unity
©2015 Jason Booth
	
	This package allows you to paint information onto the vertices of a mesh in the Unity editor. It uses the new 'additionalVertexStream' system of Unity5, which allows you to override per-instance data on meshes without paying the cost of duplicating a full mesh. This makes it ideal for painting vertex information across many instances of a mesh. The tool also allows you to easily bake that information back to mesh assets if you'd prefer to make a modified mesh.
	After copying the package into your Unity project, open the vertex painter via the “Window->Vertex Painter” menu item. Next, select the objects you wish to paint in your scene and make sure the “Enabled” check box is checked. You can toggle this option with the ESC key when the window is active. 
	Checking “Show Vertex Data” will temporarily change the material on your object to use a special shader which shows the vertex data you are currently editing. 
	Once “Enabled” and “Show Vertex Data” are checked, you can use the brush in the scene view to paint vertex colors onto your mesh. The Brush Mode property will change the target of your painting, allowing you to paint into all color channels at once or just one channel at a time, or paint into the UV coordinates on the mesh. 
	You can easily remove the data from any channel by clicking on the button in the ‘clear channel’ menu, restoring the mesh to it’s original form. 
	If “Show Vertex Data” is on, the display will update to show you the information you are currently painting. This will be displayed as a color when set to color mode and a black (0) to white (1) gradient on the R/G/B/A color channels. When painting values into one of the 4 UV coordinate sets, there is an option to set the display range. This is because UV coordinates, unlike colors, are given a full 32 bit value to work with and can represent information out of the 0-1 range. As such, the range options allow to to set the low and high value used to remap the range into a 0-1 display range.
	The brush has sizing and flow options which should be self explanatory. The brush is also pressure sensitive if you are using a tablet which supports it. 



	
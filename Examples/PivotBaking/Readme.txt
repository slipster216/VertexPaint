Pivot Rotation Example:

   Baking information about the pivot of an object into it's UVs can be a useful technique. The Vertex Painter's "Bake" interface lets you easily bake this information into UV channels. You can bake the position or rotation into a UV channel, with the last component of both being a random number assigned to that object. 

   Some ways I've used this in the past:
   - Creating fields of asteroids that rotate in the vertex shader, taking no CPU time to update
   - Animating a pre-shattered object without particles, physics, or other costly techniques
   - Vertex animation for plants and other simple hierarchies

   The included example shows several boxes spinning around their local pivot positions, while all being baked into a single mesh, which is more efficient to draw and update than having several separate objects.

   There are two example scenes, "pivot_start", which contains the original boxes used in the demo before baking and combining, and "pivot_complete", which has the final scene. The four boxes in the start scene are all separate objects. 

   A quick tutorial:

   - Load the "pivot_start" scene
   - Select the four boxes
   - Go to the "Bake" tab of the vertex painter
   - Under the "Bake Pivot" section, make sure "Store In" is set to UV2 and "Local Space" is true. Press the "Bake Pivot" button.
   - Change the "Store In" option to UV3 and press the "Bake Rotation" button
   - From the "Mesh Combiner" section, press the "Combine and Save" button to save the mesh somewhere

   - Create a new scene
   - Drag the combined object into the scene
   - Create a material and assign it the "pivot" shader
   - Assign the material to the combined object and press play

   At this point, you will see the objects rotating around the Y axis. 

   If you swap the shader for the pivot_aligned shader, the objects will rotate around their local space axis instead of world space. 

   Note that the objects are shaded different shades of grey- this is showing the random value assigned in the 4th component of the uv2 channel. You can use this value to randomize the speed of the rotation, offset the animation, or as the seed into a noise function. The random value will be assigned the same to every vertex in each original mesh. 



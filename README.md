# TS4 SimRipper
### A fork of CmarNYC's SimRipper that aims to improve speed and more.

Many thanks to her for allowing me to continue this project!

Improvements in this version currently include performance improvements with lots of CC and Mods, the ability to craete rigs for in-game IK, and improved startup time!

# Get support here on [Discord!](https://discord.gg/E6VJF7XgbA)

Original Description, modified to remove older release notes, is as follows.

This is a tool to read TS4 save files, list the sims, and create a mesh of the sim with all the appropriate morphs applied, hopefully duplicating the appearance of the sim in the game. The mesh can then be saved in a choice of mesh formats. The composited diffuse and specular textures are also saved. Optionally a composited bumpmap and emissionmap can be saved, but these are experimental and may not be useful.

# Tl;dr It's a tool for extracting Sims 4 sims for use in other 3d programs like Blender.

Meshes exported as Collada DAE can be imported into Blender (or other 3D editors that support DAE) with rig and bone assignments.

To use: Extract the tool folder from the attached zip and run TS4SimRipper.exe. Select a save file, then click on any of the sim's names which appear on the left. After several seconds the sim should appear and you can use the outfit dropdown to change outfits and the save buttons to save the sim in the current outfit.

**WHEN ASKING FOR HELP WITH A PROBLEM, PLEASE UPLOAD THE SIM TROUBLESHOOTING INFO, ALONG WITH PICTURES AND ERROR MESSAGES IF APPROPRIATE. IF THE SIM WON'T DISPLAY, REMOVE ALL CC AND MODS. IF IT STILL WON'T DISPLAY, UPLOAD YOUR SAVED GAME. IF IT DOES DISPLAY, ADD YOUR CC AND MODS IN A FEW AT A TIME AND FIGURE OUT WHICH IS CAUSING THE ERROR, THEN UPLOAD THAT CC/MOD PACKAGE AND YOUR SAVED GAME.**

**WE CAN ALL SAVE TIME AND ANNOYANCE IF I DON'T HAVE TO ASK EVERY TIME.**

Requirements: .NET 4.5, 64-bit Windows. No Mac and no plans to support Mac, sorry.

Please note that with custom content and default replacements supported, if you're using adult skins and meshes they may show up.

**Tutorial on the basics of using these meshes in Blender, including importing textures and animations and transferring normals: http://www.modthesims.info/showthread.php?t=639931**

**Known problems/limitations:**
- Some, maybe all, big dogs may have distortion in the meshes.
- Animations for big dogs look like they were made for a much larger animal and there's distortion of the head. Animations for little dogs and cats are fine. No idea why.
- Pet collars and clothing do not (yet) compress fluffy fur and will disappear into the fur on floofy pets.
- When loading toddlers or children, you may get errors saying one or more physique morphs (heavy/muscle/thin/bony) was not found. As far as I can tell this is normal and caused by some of the body morphs not being implemented for children/toddlers.
- The edges of hair meshes may be slightly visible, especially in Blender. This may be a normals problem.
- Some skin details and makeup don't show up as visibly as they should.
- Cleaning DAE meshes seems not to be working very well.

**A note about error messages:**
- Error messages saying it can't find presets, sim modifiers, BGEOs, DMaps, or BoneDeltas may or may not mean anything. In most cases it's caused by a custom preset or slider you used to have but have removed. The preset/slider reference may still be in the sim's data.
- Error messages saying it can't find a LRLE texture may or may not mean anything. Because of the workarounds used for CC makeup and skins the 'LRLE' texture may actually be an RLE.
- If the sim looks right, don't worry about the messages.

Please note the Collada DAE format will import into Blender with all skeleton and joint assignments intact. Unfortunately their importer will not keep the original normals. IMO in most cases that probably won't make a significant difference but the people who may use these meshes in Blender will have to determine that. Possibly they can export in both DAE and OBJ and transfer data in Blender.

**Texturing and alpha transparency:**
- Starting in version 2.3, textures are linked into DAE meshes and will be loaded automatically as long as the textures are in the same folder as the DAE. All you should have to do is change the Viewport Shading (next to the Mode selection) to Material.
- To make transparency work and to adjust shine:
-- If alpha hairs or other meshes that use alpha transparency aren't displaying correctly: After importing the DAE, expand the rig and select any mesh, click the Texture icon in the properties under the objects listing, select the first texture, scroll down to the Influence section, and under Diffuse put a check next to Alpha.
-- To adjust shine intensity: select a mesh and click on the Texture icon as above, then select the second texture (which should be the specular) scroll down to the Influence section, and under Specular change the Intensity number. Higher numbers = more shine.

Pictures include sims in SimRipper and in Milkshape. I'm hopeless in Blender but have managed to do a couple of screenshots. 

**If you're reporting a problem, please, please don't just say 'It doesn't work'. I need to see error messages, a description of the problem, pictures if possible, whether it's happening with all sims or certain sims. If you have an error message saying the tool can't read something, please post a screenshot of the message or the text. You can blur or edit any file paths that contain personal information. Please only include the first 10 lines or so in comments.**

**As of version 3.3, you can click the "Sim Troubleshooting Info" button and save a zip including the information and error listings and the CC resources the sim is using. Please upload this zip with your report of a problem, it'll save everyone a lot of time. PLEASE INCLUDE IT WHEN YOU POST SO I DON'T HAVE TO ASK.**


Image and package handling is done with s4pi: https://github.com/s4ptacle/Sims4Tools

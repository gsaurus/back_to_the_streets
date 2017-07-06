# Back to the Streets

## Workflow to create/edit characters:
1. Use asset-packer Unity project to setup and pack your Unity entities (3D models, sprite animations etc)
2. On menu select Assets -> Export Asset Bundles. Assets will be exported to be used by the editor, and the game
3. Run the command "update_assets" (.sh or .bat depending on your OS). This will copy the bundles across editor and game
4. Open the editor project in Unity. Here you can create or open a character and will have the assets updated from the asset-packer.
5. Once you are satisfied with your character, save it, then run the command "update_assets" again
6. Open the unity-game project. You may have to edit VersusWorldController to edit the startup characters in the scene.
7. Run, and you should see your characters in action.

## Workflow to edit source code
BTTS code relies in a pure model-view-controller architecture.
You can edit View and Controller code directly on the unity-game project, but the Model code is in separate projects.
In the folder data-model you find:
* storage model: entities storage format, shared by the editors and the game
* engine model: low level data information (generic entities, etc)
* game model: game specific information
* The serializer projects are directly related with protocol buffers serialization.
When you edit the model, the resulting dlls must be copied to the editor and unity-game projects. Ideally they all share it
but at the present moment they have to be copied around. To do so use the command "build_all" passing "Debug" as argument.
This command will compile the model sources into dlls, and copy them to editor and unity-game projects

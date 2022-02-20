# PlumploriGame
 Point and Click Game made with Unity
 
 ## Instructions for setting up the development environment
 
1. Install the unity hub
2. Install blender. If you do not have blender installed, unity will not be able to import the blender models.
3. Fetch the repository.
4. Load the project in unity hub. There you can see which version of the Unity Editor is required. If the older version no longer exists, the project can be rebuilt on a new version. The Unity Editor does this on its own. Usually there are no problems.
5. In the package manager (Window -> package manager) the packages for DotTween must be downloaded and imported. For DotTween, a setup is needed before execution,

After that, the project should be able to run in playmode.

Maybe initially Unity does not load the correct start scene. You need to switch to the Main scene (Assets/Scenes/). Only this scene can start the game. In all other scenes, errors will occur because the most important references are not available in these scenes.

## Troubleshooting
When the project is opened without Blender installed, the Unity Editor will not be able to display the blend files in the Models asset folder.
Instead, only white boxes can be seen, and nothing of the model is shown in the scene or in prefabs.
This can be easily fixed by installing Blender. After that, a reimport has to be done on the main folder Models in the Assets. Then the models can be seen as a small previews.

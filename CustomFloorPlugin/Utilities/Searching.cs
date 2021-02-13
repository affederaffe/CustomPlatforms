using System;

using UnityEngine.SceneManagement;


namespace CustomFloorPlugin.Utilities
{
    internal static class Searching
    {
        /// <summary>
        /// Finds the current environment <see cref="Scene"/>, prioritizing non-menu environments
        /// </summary>
        /// <exception cref="EnvironmentSceneNotFoundException"></exception>
        /// <returns>The current environment <see cref="Scene"/></returns>
        internal static Scene GetCurrentEnvironment()
        {
            Scene scene = new Scene();
            Scene environmentScene = scene;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                scene = SceneManager.GetSceneAt(i);
                if (scene.name.EndsWith("Environment") && (!environmentScene.IsValid() || environmentScene.name.StartsWith("Menu")))
                {
                    if (!scene.name.StartsWith("Multiplayer"))
                    {
                        environmentScene = scene;
                    }
                    else
                    {
                        environmentScene = SceneManager.GetSceneByName("GameCore");
                    }
                }
            }
            if (environmentScene.IsValid())
            {
                return environmentScene;
            }
            throw new Exception("No Environment Found");
        }
    }
}

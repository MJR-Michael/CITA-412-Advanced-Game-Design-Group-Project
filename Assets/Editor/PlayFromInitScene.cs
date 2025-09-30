using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

//Pure ChatGPT code w/ some modifications. I will break down what is happening logically through the script.

[InitializeOnLoad]
public static class PlayFromInitScene
{
    //Store the path to the initialization scene (Assets/Scenes/initSceneName.unity)
    private const string InitScenePath = "Assets/Scenes/Initialization.unity";

    //Create session keys for the current scene. Basically, these are strings to act as variables in the SessionState class
    //Session keys act like Editor-persistent variables that survive domain reloads
    private const string SessionKeyPrevScene = "PlayFromInitScene.prevScene";

    //1/0 variable to determine whether the scene did switch. 1 if true, 0 if false
    private const string SessionKeyDidSwitch = "PlayFromInitScene.didSwitch";

    //Runs when the Unity editor reloads
    static PlayFromInitScene()
    {
        //Handle logic for when the play mode state changes. (i.e., entering/exiting edit/play mode)
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        //Handle logic for exiting edit mode (entering play mode)
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            //If init scene doesn't exist, cancel and warn
            if (!System.IO.File.Exists(InitScenePath))
            {
                Debug.LogError($"PlayFromInitScene: Init scene not found at '{InitScenePath}'. Cancelling play.");
                EditorApplication.isPlaying = false;
                return;
            }

            //Store the current scene
            var current = EditorSceneManager.GetActiveScene();

            // If we're already on the init scene, do nothing (don't overwrite stored path)
            if (current.path == InitScenePath)
            {
                SessionState.SetInt(SessionKeyDidSwitch, 0);
                SessionState.SetString(SessionKeyPrevScene, "");
                return;
            }

            // Ask the user to save modified scenes. If they cancel, cancel play.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // store the current scene path in SessionState so it survives domain reload
            SessionState.SetString(SessionKeyPrevScene, current.path);
            SessionState.SetInt(SessionKeyDidSwitch, 1);

            //Store the scene name for init obj to reference
            PlayerPrefs.SetString("PlayFromInitScene.PreviousScene", current.name);
            PlayerPrefs.Save();

            // open the init scene (single mode so it becomes the active scene)
            EditorSceneManager.OpenScene(InitScenePath, OpenSceneMode.Single);
        }

        //Handle logic for returning to edit mode
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            //Determine if the scene did switch
            if (SessionState.GetInt(SessionKeyDidSwitch, 0) != 1)
            {
                // we didn't switch earlier, nothing to restore
                return;
            }

            //Store the previous scene path from session state
            var prev = SessionState.GetString(SessionKeyPrevScene, "");

            //Check if the scene can be opened
            if (!string.IsNullOrEmpty(prev) && prev != InitScenePath && System.IO.File.Exists(prev))
            {
                //Open the scene
                EditorSceneManager.OpenScene(prev, OpenSceneMode.Single);
            }
            else
            {
                // nothing to restore (either lost path or previous scene was removed)
                Debug.Log("PlayFromInitScene: No previous scene to restore (it may have been removed or was the init scene).");
            }

            //cleanup session state
            SessionState.SetInt(SessionKeyDidSwitch, 0);
            SessionState.SetString(SessionKeyPrevScene, "");
        }
    }
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeGame : MonoBehaviour
{
    [SerializeField]
    GameObject[] singletonManagersAndObject;

    string sceneToLoad = string.Empty;

    const string MAIN_MENU_SCENE = "MainMenu";

    private void Awake()
    {
        //Create all given singletons
        foreach (GameObject singleton in singletonManagersAndObject)
        {
            //craete the singleton
            GameObject newSingleton = Instantiate(singleton);
            newSingleton.name = singleton.name;
        }

        //Check if there is a specific scene to reload
        sceneToLoad = PlayerPrefs.GetString("PlayFromInitScene.PreviousScene", "");

        //Delele the entry if it existed
        PlayerPrefs.DeleteKey("PlayFromInitScene.PreviousScene");
    }

    private void Start()
    {
        //Load next scene behavior
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Load the previous scene
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            // Fallback: load main menu
            SceneManager.LoadScene(MAIN_MENU_SCENE);
        }
    }
}

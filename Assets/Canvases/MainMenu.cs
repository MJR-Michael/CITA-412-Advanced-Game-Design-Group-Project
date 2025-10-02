using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    TMP_InputField inputField;

    const string GAME_SCENE_STRING = "GameScene";

    public void OnButtonPressed()
    {
        Debug.Log("Button pressed. Input text value: " + inputField.text);

        Debug.Log($"Can this be parsed to an integer? {int.TryParse(inputField.text, out int i)}");

        if (int.TryParse(inputField.text, out int gameSeed))
        {
            RNGSeedManager.Instance.SetGameSeed(gameSeed);
        }
        else
        {
            RNGSeedManager.Instance.SetGameSeedRandom();
        }

        SceneManager.LoadScene(GAME_SCENE_STRING);
    }
}

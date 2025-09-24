using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI inputText;

    const string GAME_SCENE_STRING = "GameScene";

    public void OnButtonPressed()
    {
        if (int.TryParse(inputText.text, out int gameSeed))
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

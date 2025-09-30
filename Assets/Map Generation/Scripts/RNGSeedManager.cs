using UnityEngine;

public class RNGSeedManager : MonoBehaviour
{
    [SerializeField]
    int gameSeed = 1041234;

    public static RNGSeedManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UnityEngine.Random.InitState(gameSeed);
    }

    public void SetGameSeed(int newGameSeed)
    {
        gameSeed = newGameSeed;

        Debug.Log("Game seed: " + gameSeed);

        UnityEngine.Random.InitState(gameSeed);
    }

    public void SetGameSeedRandom()
    {
        SetGameSeed(Random.Range(0, int.MaxValue));

    }
}

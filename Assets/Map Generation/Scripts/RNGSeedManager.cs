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
        UnityEngine.Random.InitState(gameSeed);
    }

    public void SetGameSeedRandom()
    {
        gameSeed = Random.Range(0, int.MaxValue);
        UnityEngine.Random.InitState(gameSeed);
    }
}

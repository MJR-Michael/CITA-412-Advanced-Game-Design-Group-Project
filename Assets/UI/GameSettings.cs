using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameSettings : MonoBehaviour
{
    // === UI & HUD SETTINGS ===
    [Header("UI & HUD")]
    public bool hideHUD = false;
    public bool showCrosshair = true;
    public bool hideHealth = false;
    public bool showSpeedometer = true;
    public bool showMapAlways = false;
    public bool highlightAllies = true;
    public bool enemyOutline = true;
    public bool allyOutline = true;

    // === CAMERA & VISUALS ===
    [Header("Camera & Visuals")]
    [Range(60, 120)] public float fov = 90f;
    public bool fovBoostDuringAbilities = true;
    public bool cameraShake = true;

    [Header("Graphics")]
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public bool fullscreen = true;
    public float resolutionScale = 1.0f;
    public bool vsync = true;
    public int maxFPS = 144;

    public bool motionBlur = true;
    public bool bloom = true;
    public bool ambientOcclusion = true;
    public bool highDetailTextures = true;
    public bool reflections = true;
    public bool rayTracedReflections = false;
    public bool shadows = true;
    public bool rayTracedShadows = false;

    // === ACCESSIBILITY & COMFORT ===
    [Header("Accessibility")]
    public bool reducedFlash = false;
    public bool reducedBlood = false;
    public bool autoStopOnLedges = true;
    public bool halfGameSpeed = false;

    // === GAMEPLAY ===
    [Header("Gameplay")]
    public bool weaponsAutoFire = false;

    // === AUDIO ===
    [Header("Audio")]
    [Range(0, 1)] public float masterVolume = 1.0f;
    [Range(0, 1)] public float soundEffectsVolume = 1.0f;
    [Range(0, 1)] public float musicVolume = 1.0f;
    public bool dynamicMusic = true;

    // === EVENTS ===
    [Header("Events (Optional)")]
    public UnityEvent onSettingsApplied;
    public UnityEvent onSettingsReset;

    // --- INTERNAL STATE ---
    private static GameSettings instance;

    public static GameSettings Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameSettings>();
            return instance;
        }
    }

    // === LIFECYCLE ===
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // === APPLY / SAVE / LOAD ===
    public void ApplySettings()
    {
        // Placeholder: implement later (apply resolution, vsync, audio, etc.)
        onSettingsApplied?.Invoke();
        Debug.Log("Settings applied!");
    }

    public void ResetToDefaults()
    {
        // Placeholder: reset all settings to defaults
        onSettingsReset?.Invoke();
        Debug.Log("Settings reset to defaults!");
    }

    public void SaveSettings()
    {
        // TODO: Save to PlayerPrefs or JSON file
        Debug.Log("Settings saved.");
    }

    public void LoadSettings()
    {
        // TODO: Load from PlayerPrefs or JSON file
        Debug.Log("Settings loaded.");
    }
}

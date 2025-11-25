using UnityEngine;
using UnityEngine.UI;
using TMPro; // if using TextMeshPro

public class CompendiumManager : MonoBehaviour
{
    [Header("References")]
    public CompendiumUpdater compendiumUpdater;
    public Transform buttonContainer;   // The parent UI panel for buttons
    public GameObject compendiumButtonPrefab;     // A prefab for a single codex button (with a TMP_Text)

    [Header("Data")]
    public CompendiumEntry[] entries;   // Assign all 30 entries here

    void Start()
    {
        PopulateCodex();
    }

    void PopulateCodex()
    {
        foreach (CompendiumEntry entry in entries)
        {
            GameObject newButton = Instantiate(compendiumButtonPrefab, buttonContainer);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = entry.entryName;

            // Capture local variable for closure
            CompendiumEntry capturedEntry = entry;
            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                compendiumUpdater.DisplayEntry(capturedEntry);
            });
        }
    }
}

using UnityEngine;
using TMPro;

public class TropheyUpdater : MonoBehaviour
{
    [Header("References")]
    public Transform modelParent;
    public Camera modelCamera;
    public TMP_Text descriptionText;

    public void DisplayEntry(CompendiumEntry entry)
    {
        // --- Update UI ---
        if (descriptionText != null)
            descriptionText.text = entry.description;
    }
}

using UnityEngine;
using TMPro;

public class CompendiumUpdater : MonoBehaviour
{
    [Header("References")]
    public Transform modelParent;
    public Camera modelCamera;
    public TMP_Text descriptionText;
    private GameObject currentModel;

    public void DisplayEntry(CompendiumEntry entry)
    {
        // --- Update UI ---
        if (descriptionText != null)
            descriptionText.text = entry.description;

        // --- Destroy old model ---
        if (currentModel != null)
            Destroy(currentModel);

        // --- Spawn new model ---
        if (entry.modelPrefab != null)
        {
            currentModel = Instantiate(entry.modelPrefab, modelParent);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;

            // Recenter model based on bounds
            CenterModel();

            // Adjust camera distance based on model size
            FitModelInView();
        }
    }

    private void CenterModel()
    {
        if (currentModel == null) return;

        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        // Shift model so its center is at modelParent origin
        Vector3 offset = bounds.center - currentModel.transform.position;
        currentModel.transform.position -= offset;
    }

    private void FitModelInView()
    {
        if (modelCamera == null || currentModel == null) return;

        Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
        foreach (var r in renderers)
            bounds.Encapsulate(r.bounds);

        float size = bounds.extents.magnitude;
        float distance = size * 2f; // tweak multiplier as needed

        modelCamera.transform.position = modelParent.position - modelCamera.transform.forward * distance;
    }
}

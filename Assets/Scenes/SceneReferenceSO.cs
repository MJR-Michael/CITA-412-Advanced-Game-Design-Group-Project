using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "ScriptableObject/SceneReferenceSO", fileName = "New SceneReferenceSO")]
public class SceneReferenceSO : ScriptableObject
{
    [SerializeField]
    SceneAsset sceneAsset;

    [HideInInspector] public string SceneName { get; private set; }

    private void OnValidate()
    {
        if (sceneAsset == null)
        {
            Debug.LogWarning($"Warning: {this} is not initialized with a scene asset! give it one!");
            return;
        }

        //Set the value of the scene name
        SceneName = sceneAsset.name;
    }
}

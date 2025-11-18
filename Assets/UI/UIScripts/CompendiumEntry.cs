using UnityEngine;

[CreateAssetMenu(fileName = "CompendiumEntry", menuName = "Compendium/Entry")]
public class CompendiumEntry : ScriptableObject
{
    public string entryName;
    [TextArea(3,6)] public string description;
    public GameObject modelPrefab;
}

using UnityEngine;

[CreateAssetMenu(fileName = "New AbilitySO", menuName = "ScriptableObject/Abilities/AuraAbility")]
public class AbilityAuraSO : AbilitySO
{
    public override AbilityBase ApplyTo(GameObject target)
    {
        return target.AddComponent<AuraAbility>();
    }
}

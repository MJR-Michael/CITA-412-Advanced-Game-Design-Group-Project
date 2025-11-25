using UnityEngine;

[CreateAssetMenu(fileName = "New AbilitySO", menuName = "ScriptableObject/Abilities/ProjectileAbility")]
public class AbilityProjectileSO : AbilitySO
{
    public override AbilityBase ApplyTo(GameObject target)
    {
        return target.AddComponent<ProjectileAbility>();
    }
}

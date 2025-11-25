using UnityEngine;

[CreateAssetMenu(fileName = "New AbilitySO", menuName = "ScriptableObject/Abilities/DashAbility")]
public class AbilityDashSO : AbilitySO
{
    public override AbilityBase ApplyTo(GameObject target)
    {
        return target.AddComponent<AbilityDash>();
    }
}

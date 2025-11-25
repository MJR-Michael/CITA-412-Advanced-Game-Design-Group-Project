using UnityEngine;

public abstract class AbilitySO : ScriptableObject
{
    public abstract AbilityBase ApplyTo(GameObject target);
}

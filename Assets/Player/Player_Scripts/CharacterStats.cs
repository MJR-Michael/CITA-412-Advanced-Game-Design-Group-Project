using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public WeaponBase weapon;

    public List<AbilityBase> allAbilities = new List<AbilityBase>();

    private List<IAugment> activeAugments = new List<IAugment>();

    public void AddAugment(IAugment augment)
    {
        activeAugments.Add(augment);
        augment.Apply(this);
    }

    public void RemoveAugment(IAugment augment)
    {
        activeAugments.Remove(augment);
        augment.Remove(this);
    }
}

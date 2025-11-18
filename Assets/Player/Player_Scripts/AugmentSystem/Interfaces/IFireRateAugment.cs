using UnityEngine;

public class FireRateAugment : IAugment
{
    public float multiplier = 1.2f;

    private WeaponBase weapon;

    public void Apply(CharacterStats character)
    {
        weapon = character.weapon;
        weapon.FireRate /= multiplier;
    }

    public void Remove(CharacterStats character)
    {
        weapon.FireRate *= multiplier;
    }
}

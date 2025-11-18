using UnityEngine;

public class ProjectileSizeAugment : IAugment
{
    public float sizeMultiplier = 1.5f;

    public void Apply(CharacterStats character)
    {
        character.weapon.OnProjectileSpawned += ScaleProjectile;
    }

    public void Remove(CharacterStats character)
    {
        character.weapon.OnProjectileSpawned -= ScaleProjectile;
    }

    void ScaleProjectile(GameObject proj)
    {
        proj.transform.localScale *= sizeMultiplier;
    }
}

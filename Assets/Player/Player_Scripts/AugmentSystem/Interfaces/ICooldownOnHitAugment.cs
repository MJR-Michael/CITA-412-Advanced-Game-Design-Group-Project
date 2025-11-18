public class CooldownOnHitAugment : IAugment
{
    int abilityIndex = 0;
    System.Action onHitCallback;

    public void Apply(CharacterStats character)
    {
        onHitCallback = () =>
        {
            // validate index
            if (abilityIndex < 0 || abilityIndex >= character.allAbilities.Count)
                return;

            var ability = character.allAbilities[abilityIndex];

            // only abilities with cooldown are affected
            if (ability is IHasCooldown cdAbility)
            {
                cdAbility.ReduceCooldown(1f);
            }
        };

        character.weapon.OnHit += onHitCallback;
    }

    public void Remove(CharacterStats character)
    {
        character.weapon.OnHit -= onHitCallback;
    }
}

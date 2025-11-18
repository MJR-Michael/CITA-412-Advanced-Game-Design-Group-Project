using UnityEngine;

public class DurationAugment : IAugment
{
    public float flatIncrease = 1f;

    public void Apply(CharacterStats character)
    {
        foreach (var ability in character.allAbilities)
        {
            ability.Duration += flatIncrease;
        }
    }

    public void Remove(CharacterStats character)
    {
        foreach (var ability in character.allAbilities)
        {
            ability.Duration -= flatIncrease;
        }
    }
}

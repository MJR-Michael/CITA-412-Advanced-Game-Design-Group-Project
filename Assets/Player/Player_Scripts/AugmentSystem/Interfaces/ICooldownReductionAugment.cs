public class CooldownReductionAugment : IAugment
{
    public float multiplier = 0.8f;     //reduces cooldown by 20%

    AbilityBase ability;

    public void Apply(CharacterStats character)
    {
        ability = character.allAbilities[0]; // example
        ability.Cooldown *= multiplier;
    }

    public void Remove(CharacterStats character)
    {
        ability.Cooldown /= multiplier;
    }
}

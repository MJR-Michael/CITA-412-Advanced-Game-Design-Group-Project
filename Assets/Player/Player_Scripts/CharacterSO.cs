using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CharacterSO", menuName = "ScriptableObject/Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Health Attributes")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Space(25)]
    [Header("Movement Attributes")]
    public float moveSpeed = 20f;
    public float sprintSpeed = 12f;
    public float jumpHeight = 3.6f;
    public float gravityEffect = -60f;

    [Space(25)]
    [Header("Base Dodge Roll Attributes")]
    public float rollSpeed = 15f;
    public float rollDistance = 10f;
    public float rollCooldown = 1f;

    [Space(25)]
    [Header("Character Abilities")]
    public AbilitySO[] characterAbilities;

    [Space(25)]
    [Header("Character Weapons")]
    public WeaponBase[] characterWeapons;

    public List<AbilityBase> ApplyAbilitiesTo(GameObject target)
    {
        List<AbilityBase> abilities = new List<AbilityBase>();

        foreach (AbilitySO ability in characterAbilities)
        {
            AbilityBase appliedAbility = ability.ApplyTo(target);
            abilities.Add(appliedAbility);
        }

        return abilities;
    }

    public List<WeaponBase> ApplyWeaponsTo(GameObject target)
    {
        List<WeaponBase> weapons = new List<WeaponBase>();

        foreach (WeaponBase weapon in characterWeapons)
        {
            Instantiate(weapon, target.transform);
            weapons.Add(weapon);
        }

        return weapons;
    }
}

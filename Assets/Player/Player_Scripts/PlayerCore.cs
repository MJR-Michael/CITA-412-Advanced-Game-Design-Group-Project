
using NUnit.Framework;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] Health health;
    [SerializeField] DodgeRoll dodgeRoll;
    [SerializeField] FirstPersonController firstPersonController;
    [SerializeField] CharacterSO characterSO;
    [SerializeField] CharacterStats characterStats;
    [SerializeField, Tooltip("The parent object that will hold the ability components")] GameObject abilityTarget;
    [SerializeField, Tooltip("The parent object that will hold the weapon prefab(s)")] GameObject weaponTarget;
    public bool isInvulnerable = false;

    List<AbilityBase> appliedAbilities;
    List<WeaponBase> appliedWeapons;

    protected virtual void Awake()
    {
        //Initialize Health
        if (health == null)
        {
            if (!TryGetComponent<Health>(out Health healthComponent))
            {
                Debug.LogWarning($"Warning: No health component given or attached to {gameObject}. Creating one manually");
                health = gameObject.AddComponent<Health>();
            }
            else
            {
                health = healthComponent;
                health.Initialize(characterSO.maxHealth);
            }
        }
        health.OnDeath += (_, _, _) => HandleDeath();


        //Initialize Dodge Roll
        if (dodgeRoll != null)
        {
            dodgeRoll.Initialize(characterSO.rollSpeed, characterSO.rollDistance, characterSO.rollCooldown);
        }
        else
        {
            if (TryGetComponent<DodgeRoll>(out DodgeRoll dodgeRollComponent))
            {
                Debug.LogWarning("Warning: Dodge roll component found on {gameObject}, but reference was missing");
                dodgeRollComponent.Initialize(characterSO.rollSpeed, characterSO.rollDistance, characterSO.rollCooldown);
            }
            else
            {
                Debug.LogWarning("Warning: No Dodge roll component found on {gameObject}. Creating one.");
                DodgeRoll appliedDodgeRollComponent = gameObject.AddComponent<DodgeRoll>();
                appliedDodgeRollComponent.Initialize(characterSO.rollSpeed, characterSO.rollDistance, characterSO.rollCooldown);
            }
        }

        //Initialize Abilities
        if (abilityTarget != null)
        {
            appliedAbilities = characterSO.ApplyAbilitiesTo(abilityTarget);
        }
        else
        {
            Debug.LogWarning($"Warning: no abiltiy target reference for {gameObject}. Applying abilities to {gameObject} instead.");
            appliedAbilities = characterSO.ApplyAbilitiesTo(gameObject);
        }


        //Initialize Weapon(s)
        if (weaponTarget != null)
        {
            appliedWeapons = characterSO.ApplyWeaponsTo(weaponTarget);
        }
        else
        {
            Debug.LogWarning($"Warning: no weapon target reference for {gameObject}. Applying weapons to {gameObject} instead.");
            appliedWeapons = characterSO.ApplyWeaponsTo(weaponTarget);
        }


        //Initialize Character Stats
        if (characterStats != null)
        {
            characterStats.InitializeWeapon(appliedWeapons[0]);
            characterStats.InitializeAbilities(appliedAbilities);
        }
        else
        {
            if (TryGetComponent<CharacterStats>(out CharacterStats characterStatsComponent))
            {
                Debug.LogWarning($"Warning: Reference to character stat found for {gameObject}, but it was not assigned in the editor");
                characterStatsComponent.InitializeWeapon(appliedWeapons[0]);
                characterStatsComponent.InitializeAbilities(appliedAbilities);
            }
            else
            {
                CharacterStats appliedCharacterStatsComponent = gameObject.AddComponent<CharacterStats>();
                Debug.LogWarning($"Warning: No character stats reference given to {gameObject}. Creating one");
                appliedCharacterStatsComponent.InitializeWeapon(appliedWeapons[0]);
                appliedCharacterStatsComponent.InitializeAbilities(appliedAbilities);
            }
        }


        //Initialize FirstPersonController
        if (firstPersonController != null)
        {
            firstPersonController.MoveSpeed = characterSO.moveSpeed;
            firstPersonController.SprintSpeed = characterSO.sprintSpeed;
            firstPersonController.JumpHeight = characterSO.jumpHeight;
            firstPersonController.Gravity = characterSO.gravityEffect;
        }
        else
        {
            if (TryGetComponent<FirstPersonController>(out FirstPersonController firstPersonControllerComponent))
            {
                Debug.LogWarning($"Warning: Reference to character stat found for {gameObject}, but it was not assigned in the editor");
                firstPersonControllerComponent.MoveSpeed = characterSO.moveSpeed;
                firstPersonControllerComponent.SprintSpeed = characterSO.sprintSpeed;
                firstPersonControllerComponent.JumpHeight = characterSO.jumpHeight;
                firstPersonControllerComponent.Gravity = characterSO.gravityEffect;
            }
            else
            {
                FirstPersonController appliedFirstPersonControllerComponent = gameObject.AddComponent<FirstPersonController>();
                Debug.LogWarning($"Warning: No character stats reference given to {gameObject}. Creating one");
                appliedFirstPersonControllerComponent.MoveSpeed = characterSO.moveSpeed;
                appliedFirstPersonControllerComponent.SprintSpeed = characterSO.sprintSpeed;
                appliedFirstPersonControllerComponent.JumpHeight = characterSO.jumpHeight;
                appliedFirstPersonControllerComponent.Gravity = characterSO.gravityEffect;
            }
        }
    }

    protected abstract void HandleDeath();
    public void TakeDamage(GameObject sender, float damage, DamageType damageType) => health.TakeDamage(sender, damage, damageType);
    public void Heal(GameObject sender, int amount) => health.Heal(sender, amount);

}

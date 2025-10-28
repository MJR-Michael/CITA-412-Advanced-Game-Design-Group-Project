using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [System.Serializable]
    public class DamageTypeResistances
    {
        [Tooltip("The type of damage")]
        public DamageType damageType;

        [Tooltip("The percentage of the damage that is resisted on scale 0% to 100%")]
        public float resistancePercent;
    }


    public event Action<Health> OnMaxHealthChanged;
    public event Action<GameObject, Health, DamageType> OnCurrentHealthChanged;
    public event Action<GameObject, Health, DamageType> OnDeath;

    [Header("HP Settings")]
    [SerializeField, Tooltip("The base max HP of this object")]
    float maxHealth;

    [Header("Damage Resistances")]
    [SerializeField, Tooltip("The array of damage resistances this object has. If no resistance is given, full damage will be taken by that damage type")]
    DamageTypeResistances[] damageResistances;


    [Tooltip("The current Max HP of this object")]
    public float MaxHealth { get { return maxHealth; } }

    [Tooltip("The current HP of this object")]
    float currentHealth;
    public float CurrentHealth { get { return currentHealth; } }

    private void Awake()
    {
        //Set current health to max health
        currentHealth = MaxHealth;

        OnMaxHealthChanged += HandleMaxHealthChanged;
        OnCurrentHealthChanged += (GameObject sender, Health throwAway, DamageType damageType) => CheckForDeath(sender, damageType);
    }

    public void TakeDamage(GameObject sender, float damageAmount, DamageType damageType)
    {

        //Get the real damage amount based on the object's resistance to this damage type
        float realDamageAmount = damageAmount;

        foreach (DamageTypeResistances damageTypeResistance in damageResistances)
        {
            if (damageTypeResistance.damageType == damageType)
            {
                //Calculate the new real damage amount
                realDamageAmount -= (realDamageAmount * damageTypeResistance.resistancePercent / 100);

                //No need to continue looping
                break;
            }
        }

        //If damage amount is 0, no damage taken
        if (realDamageAmount == 0)
        {
            return;
        }
        //Check if damage is negative
        if (realDamageAmount < 0)
        {
            //Heal instead
            Heal(sender, -realDamageAmount);
            return;
        }

        //Update health, clamp between 0 and max health
        currentHealth = Mathf.Clamp(currentHealth - realDamageAmount, 0, MaxHealth);
        OnCurrentHealthChanged?.Invoke(sender, this, damageType);
    }

    public void Heal(GameObject sender, float healAmount)
    {
        //If heal amount is 0, then no health change
        if (healAmount == 0)
        {
            return;
        }
        //Check if heal amount is negative
        if (healAmount < 0)
        {
            //Take damage instead
            TakeDamage(sender, -healAmount, DamageType.Unknown);
            return;
        }

        //Update health; clamp between 0 and max HP
        currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, MaxHealth);
        OnCurrentHealthChanged?.Invoke(sender, this, DamageType.Unknown);
    }

    public void TakeMaxHPDamage(float damageAmount, DamageType damageType)
    {
        //If damage amount is 0, no damage taken
        if (damageAmount == 0)
        {
            return;
        }
        //Check if damage is negative
        if (damageAmount < 0)
        {
            //Gain max HP instead
            GainMaxHP(-damageAmount);
            return;
        }

        //Update max HP, clamp above 0
        maxHealth = Mathf.Clamp(MaxHealth - damageAmount, 0, Mathf.Infinity);
        OnMaxHealthChanged?.Invoke(this);
    }

    public void GainMaxHP(float damageAmount)
    {
        //Update max HP, clamp above 0
        maxHealth = Mathf.Clamp(MaxHealth - damageAmount, 0, Mathf.Infinity);
        OnMaxHealthChanged?.Invoke(this);
    }


    private void HandleMaxHealthChanged(Health health)
    {
        //Check if the current health is greater than max health
        currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);

        CheckForDeath(null, DamageType.Unknown);
    }

    private void CheckForDeath(GameObject sender, DamageType damageType)
    {
        if (currentHealth == 0)
        {
            OnDeath?.Invoke(sender, this, damageType);
        }
    }
}

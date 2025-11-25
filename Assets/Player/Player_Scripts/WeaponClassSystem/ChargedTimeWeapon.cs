using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargedTimeWeapon : WeaponBase
{
    Action<float> OnHitScannerFired;

    [Header("Canvas Settings")]
    [SerializeField]
    Canvas weaponOverlayCanvas;

    [SerializeField]
    RectTransform hitScannerSpawnPos;

    [SerializeField]
    RectTransform damageAnimTextStartingPos;
    

    [Space(25)]
    [Header("Prefab References")]
    [SerializeField]
    ChargedWeaponHitScanner chargedWeaponHitScannerPrefab;

    [SerializeField]
    DamageTextAnim damageAnimTextPrefab;


    [Space(25)]
    [Header("Weapon Settings")]
    [SerializeField]
    float fireRate = 2f;

    [SerializeField, Min(1)]
    int numOfHitScannersToFire;

    [SerializeField, Min(0)]
    float baseDelayBestweenHitScannersSpawning = 0.5f;

    [SerializeField, Min(0)]
    float randomDelayBetweenHitScannersFiring = 0.2f;

    [SerializeField, Min(0)]
    int maxBaseDamage = 12;

    [SerializeField, Min(0)]
    int perfectAccuracyBonusDamage = 25;

    [SerializeField, Range(0,1)]
    float accuracyBracket1 = 0.3f;

    [SerializeField, Range(0, 1)]
    float accuracyBracket2 = 0.3f;

    [SerializeField, Range(0, 1)]
    float accuracyBracket3 = 0.3f;

    [SerializeField, Range(0, 1)]
    float accuracyBracket4 = 0.1f;


    DamageTextAnim activeDamageTextAnim = null;
    List<ChargedWeaponHitScanner> activeChargedWeaponHitScanners = new List<ChargedWeaponHitScanner>();

    bool isFiring = false;
    bool isSpawningHitScanners = false;
    int totalDamageFromShot = 0;

    private void Awake()
    {
        OnHitScannerFired += HandleHitScannerFired;
        OnShoot += HandleWeaponShot;

        FireRate = fireRate;
    }

    private void Update()
    {
        if (!CanFire) return;

        if (InputManager.Instance.GetPrimaryFireInputPressedThisFrame() && !isFiring)
        {
            //Debug.Log("Player started firing");
            StartCoroutine(BeginFiring());
        }
    }

    private int GetDamageFromAccuracyRatio(float accuracyRatio)
    {
        //A little hard-coded...

        if (accuracyRatio < accuracyBracket1)
        {
            //Poor accuracy, no damaage
            return 0;
        }
        else if (accuracyRatio < accuracyBracket1 + accuracyBracket2)
        {
            //Moderate accuracy
            return maxBaseDamage / 3;
        }
        else if (accuracyRatio < accuracyBracket1 + accuracyBracket2 + accuracyBracket3)
        {
            //Good accuracy
            return maxBaseDamage / 2;
        }
        else
        {
            //Perfect accuracy
            return maxBaseDamage + perfectAccuracyBonusDamage;
        }
    }

    private int CalculateDamage(float hitScannerUISliderPosition)
    {
        int damage = 0;

        //Static values from UI slider
        float maxUISliderValue = 1;
        float midPoint = maxUISliderValue / 2;

        //Handle positions off numbered line (default to 0 / worse accuracy)
        if (hitScannerUISliderPosition < 0 || hitScannerUISliderPosition > maxUISliderValue)
        {
            damage += GetDamageFromAccuracyRatio(0);
        }
        //Handle when the hit position was left of center
        else if (hitScannerUISliderPosition < midPoint)
        {
            damage += GetDamageFromAccuracyRatio(hitScannerUISliderPosition / midPoint);
        }
        //Handle perfect accuracy
        else if (hitScannerUISliderPosition == midPoint)
        {
            damage += GetDamageFromAccuracyRatio(1);
        }
        //Handle when the hit position was right of center
        else
        {
            damage += GetDamageFromAccuracyRatio(midPoint / hitScannerUISliderPosition);
        }

        return damage;
    }

    private void HandleHitScannerFired(float hitScannerUISliderValue)
    {
        //Handle logic for next hit scanner being the one to fire
        StartCoroutine(HandleNextHitScannerActive());

        //Debug.Log("Hit scanner fired!");
        int hitScannerDamage = CalculateDamage(hitScannerUISliderValue);

        //Store damage in overall weapon damage
        totalDamageFromShot += hitScannerDamage;

        if (activeDamageTextAnim == null)
        {
            DamageTextAnim damageAnimText = Instantiate(damageAnimTextPrefab, weaponOverlayCanvas.transform);
            damageAnimText.Initialize(damageAnimTextStartingPos, totalDamageFromShot);
            activeDamageTextAnim = damageAnimText;
        }
        else
        {
            activeDamageTextAnim.UpdateDamageText(totalDamageFromShot);
        }
    }

    private void HandleWeaponShot()
    {
        //Debug.Log("Weapon shot!");
        nextFireTime = Time.time + FireRate;

        //Determine if player hit anything
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        //Player shot at nothing
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        //Debug.Log("Weapon hit an object");

        if (hit.collider.transform.TryGetComponent<Enemy>(out Enemy enemenemy))
        {
            enemenemy.TakeDamage(totalDamageFromShot);
            InvokeHit();
        }
        else if (hit.collider.transform.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(gameObject, totalDamageFromShot, DamageType.Projectile);
            InvokeHit();
        }

        //Reset damage for next shot
        totalDamageFromShot = 0;

        //Remove damage text anim
        activeDamageTextAnim = null;
    }

    IEnumerator BeginFiring()
    {
        isSpawningHitScanners = true;
        isFiring = true;
        int numOfSpawnedHitScanners = 0;
        bool isTopHitScanner = true;

        Debug.Log("Hit scanner spawned!");

        //Create the initial hit scanner
        ChargedWeaponHitScanner hitScanner = Instantiate(chargedWeaponHitScannerPrefab, hitScannerSpawnPos.transform);
        hitScanner.Initialize(OnHitScannerFired, hitScannerSpawnPos, isTopHitScanner);

        //Handle hit scanner created
        activeChargedWeaponHitScanners.Add(hitScanner);
        numOfSpawnedHitScanners++;

        //Generate a random delay between hit scanners being fired
        float delayBetweenHitScanners = baseDelayBestweenHitScannersSpawning + 
            UnityEngine.Random.Range(-randomDelayBetweenHitScannersFiring, randomDelayBetweenHitScannersFiring);
        if(delayBetweenHitScanners < 0)
        {
            delayBetweenHitScanners = 0;
        }

        while (numOfSpawnedHitScanners < numOfHitScannersToFire)
        {
            yield return new WaitForSeconds(delayBetweenHitScanners);

            //Spawn next hit scanner
            //Create the initial hit scanner
            hitScanner = Instantiate(chargedWeaponHitScannerPrefab, hitScannerSpawnPos.transform);
            hitScanner.Initialize(
                OnHitScannerFired, 
                hitScannerSpawnPos,  
                activeChargedWeaponHitScanners.Count == 0? isTopHitScanner : !isTopHitScanner);

            //Handle hit scanner created
            activeChargedWeaponHitScanners.Add(hitScanner);
            numOfSpawnedHitScanners++;

            //Generate a random delay between hit scanners being fired
            delayBetweenHitScanners = baseDelayBestweenHitScannersSpawning +
                UnityEngine.Random.Range(-randomDelayBetweenHitScannersFiring, randomDelayBetweenHitScannersFiring);
            if (delayBetweenHitScanners < 0)
            {
                delayBetweenHitScanners = 0;
            }
        }

        isSpawningHitScanners = false;
    }

    IEnumerator HandleNextHitScannerActive()
    {
        //Remove top/current hit scanner
        activeChargedWeaponHitScanners.RemoveAt(0);

        //Wait for next frame
        yield return null;

        //Check for other hit scanners
        if (activeChargedWeaponHitScanners.Count > 0)
        {
            activeChargedWeaponHitScanners[0].SetTopHitScanner(true);
        }
        else if (!isSpawningHitScanners)
        {
            //Done firing
            isFiring = false;
            InvokeShoot();
        }
    }
}

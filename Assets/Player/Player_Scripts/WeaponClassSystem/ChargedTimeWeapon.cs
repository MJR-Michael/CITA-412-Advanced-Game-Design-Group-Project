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
    RectTransform startingRectTransformPosition;

    [SerializeField]
    RectTransform endingRectTransformPosition;

    [SerializeField]
    RectTransform hitScannerAreaRectTransform;

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

    private int CalculateDamage(float hitScannerPosition)
    {
        /*
         * To calculate the accuracy, consider the hit area as a numbered line:
         *  x1    c     x2
         *  |-|-|-|-|-|-|
         *  
         *  where x1 is the lower x bounds defined by the starting position of the hit scanner, c is the center, and x2 is the upper x bounds defined
         *  by the ending position of the hit scanner.
         *  
         *  We can zero the scale by performin the operations:
         *  x1-x1=0,
         *  x2-x1=x
         *  
         *  Let this equal the new number line
         *  0     c     x
         *  |-|-|-|-|-|-|
         *  
         *  since the midpoint from 0 to x is x/2, c=x/2
         *  0    x/2    x
         *  |-|-|-|-|-|-|
         *  
         *  This will define how the accuracy bounds will be calculated.
         *  
         *  Let z represent our position along the line. We can calculate our accuracy relative to the center by breaking the number line into 3 components:
         *  z < x/2
         *  z = x/2
         *  z > x/2
         *  
         *  When z follows either of these behaviors, we will use a different ratio to calculate accuracy:
         *  
         *  z < x/2 => acc = z/(x/2) -------------------------------<<<
         *  0     z    x/2
         *  |---|---|---|
         *  
         *  z = x/2 => acc = 1 -------------------------------------<<<
         *  
         *  z > x/2 => acc = (x/2)/z -------------------------------<<<
         * x/2    z     x
         *  |---|---|---|
         */

        int damage = 0;

        //calcaulte scale from 0 to x
        float x = endingRectTransformPosition.position.x - startingRectTransformPosition.position.x;
        float midPoint = x / 2;

        //Get the position on the new zeroes numbered line
        float zeroedScannerPosition = hitScannerPosition - startingRectTransformPosition.position.x;

        //Handle positions off numbered line (default to 0 / worse accuracy)
        if (zeroedScannerPosition < 0 || zeroedScannerPosition > x)
        {
            damage += GetDamageFromAccuracyRatio(0);
        }
        //Handle when the hit position was left of center
        else if (zeroedScannerPosition < midPoint)
        {
            damage += GetDamageFromAccuracyRatio(zeroedScannerPosition / midPoint);
        }
        //Handle perfect accuracy
        else if (zeroedScannerPosition == midPoint)
        {
            damage += GetDamageFromAccuracyRatio(1);
        }
        //Handle when the hit position was right of center
        else
        {
            damage += GetDamageFromAccuracyRatio(midPoint / zeroedScannerPosition);
        }

        return damage;
    }

    private void HandleHitScannerFired(float hitScannerXPos)
    {
        //Handle logic for next hit scanner being the one to fire
        StartCoroutine(HandleNextHitScannerActive());

        //Debug.Log("Hit scanner fired!");
        int hitScannerDamage = CalculateDamage(hitScannerXPos);

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
        activeDamageTextAnim.RemoveAfterDelay();
        activeDamageTextAnim = null;
    }

    IEnumerator BeginFiring()
    {
        isSpawningHitScanners = true;
        isFiring = true;
        int numOfSpawnedHitScanners = 0;
        bool isTopHitScanner = true;

        //Debug.Log("Hit scanner spawned!");

        //Create the initial hit scanner
        ChargedWeaponHitScanner hitScanner = Instantiate(chargedWeaponHitScannerPrefab, hitScannerAreaRectTransform.transform);
        hitScanner.Initialize(OnHitScannerFired, startingRectTransformPosition, endingRectTransformPosition, isTopHitScanner);

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
            hitScanner = Instantiate(chargedWeaponHitScannerPrefab, hitScannerAreaRectTransform.transform);
            hitScanner.Initialize(
                OnHitScannerFired, 
                startingRectTransformPosition, 
                endingRectTransformPosition, 
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

using System;
using UnityEngine;

public class ChargedWeaponHitScanner : MonoBehaviour
{
    Action<float> OnHitScannerFired;

    [SerializeField]
    RectTransform rectTransform;

    [SerializeField]
    float baseSpeed = 50f;

    float endingXPos;
    bool isFired = false;
    bool isTopHitScanner;

    private void Update()
    {
        if (isFired) return;
        MoveHorizontally();

        if (rectTransform.position.x >= endingXPos)
        {
            //Debug.Log("Player missed opportunity to hit");
            OnHitScannerFired?.Invoke(rectTransform.position.x);
            return;
        }

        if (!isTopHitScanner) return;

        if (InputManager.Instance.GetPrimaryFireInputPressedThisFrame())
        {
            OnHitScannerFired?.Invoke(rectTransform.position.x);
        }
    }

    private void MoveHorizontally()
    {
        //TODO: Change horizontal movement to use Vector3.Lerp instead, having the factor based on time. The current setup does not scale the speed based on screen size, giving
        //an unfair disadvantage to smaller-screen users
        rectTransform.position = new Vector3(
            rectTransform.position.x + Time.deltaTime * baseSpeed,
            rectTransform.position.y,
            rectTransform.position.z
            );
    }

    public void SetTopHitScanner(bool isTopHitScanner)
    {
        this.isTopHitScanner = isTopHitScanner;
    }

    public void Initialize(
        Action<float> OnHitScannerFired, 
        RectTransform startingPosition,
        RectTransform endingPosiiton,
        bool isTopHitScanner
        )
    {
        this.OnHitScannerFired = OnHitScannerFired;
        rectTransform.position = startingPosition.position;
        endingXPos = endingPosiiton.position.x;
        this.OnHitScannerFired += HandleHitScannerFired;
        this.isTopHitScanner = isTopHitScanner;
    }

    private void HandleHitScannerFired(float obj)
    {
        isFired = true;
        Destroy(gameObject);
    }
}

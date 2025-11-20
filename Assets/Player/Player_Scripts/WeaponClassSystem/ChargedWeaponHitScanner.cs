using System;
using UnityEngine;
using UnityEngine.UI;

public class ChargedWeaponHitScanner : MonoBehaviour
{
    Action<float> OnHitScannerFired;

    [SerializeField]
    RectTransform rectTransform;

    [SerializeField]
    Slider uiSlider;

    [SerializeField]
    float baseSpeed = 50f;

    bool isFired = false;
    bool isTopHitScanner;

    static int STARTING_SLIDER_VALUE = 0;
    static int ENDING_SLIDER_VALUE = 1;

    private void Update()
    {
        if (isFired) return;
        MoveHorizontally();

        if (uiSlider.value == ENDING_SLIDER_VALUE)
        {
            Debug.Log("Player missed opportunity to hit");
            OnHitScannerFired?.Invoke(uiSlider.value);
            return;
        }

        if (!isTopHitScanner) return;

        if (InputManager.Instance.GetPrimaryFireInputPressedThisFrame())
        {
            OnHitScannerFired?.Invoke(uiSlider.value);
        }
    }

    private void MoveHorizontally()
    {
        //TODO: Change horizontal movement to use Vector3.Lerp instead, having the factor based on time. The current setup does not scale the speed based on screen size, giving
        //an unfair disadvantage to smaller-screen users

        uiSlider.value = Mathf.Clamp(uiSlider.value + Time.deltaTime * baseSpeed, 0, 1);
    }

    public void SetTopHitScanner(bool isTopHitScanner)
    {
        this.isTopHitScanner = isTopHitScanner;
    }

    public void Initialize(
        Action<float> OnHitScannerFired, 
        RectTransform startingPosition,
        bool isTopHitScanner
        )
    {
        this.OnHitScannerFired = OnHitScannerFired;
        rectTransform.position = startingPosition.position;
        this.OnHitScannerFired += HandleHitScannerFired;
        this.isTopHitScanner = isTopHitScanner;
        uiSlider.value = STARTING_SLIDER_VALUE;
    }

    private void HandleHitScannerFired(float obj)
    {
        isFired = true;
        Destroy(gameObject);
    }
}

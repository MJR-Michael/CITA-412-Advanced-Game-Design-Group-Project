using System.Collections;
using TMPro;
using UnityEngine;

public class DamageTextAnim : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI damageText;

    [SerializeField]
    float timeToDisappear = 1.5f;

    [SerializeField]
    DestroyAfterInactivity destroyAfterInactivity;

    public void Initialize(RectTransform startingPos, int damageAmount)
    {
        damageText.text = "+" + damageAmount.ToString();
        destroyAfterInactivity.InvokeOnActivity();
        //Maybe add an animation where the damage slowly floats and scales away in the future?
    }

    public void UpdateDamageText(int damageAmount)
    {
        damageText.text = "+" + damageAmount.ToString();
        destroyAfterInactivity.InvokeOnActivity();
        //Maybe add an animation where the damage slowly floats and scales away in the future?
    }
}

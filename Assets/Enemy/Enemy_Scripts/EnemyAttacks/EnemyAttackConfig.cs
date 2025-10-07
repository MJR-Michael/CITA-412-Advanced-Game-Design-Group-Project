using UnityEngine;

public enum FirePattern
{
    SingleTarget,
    Spread,
    RandomDirection,
    Area
}

[CreateAssetMenu(fileName = "EnemyAttackConfig", menuName = "Enemies/AttackConfig")]
public class EnemyAttackConfig : ScriptableObject
{
    public GameObject projectilePrefab;
    public float fireRate = 2f;
    public float projectileSpeed = 10f;
    public int spreadCount = 1;        // for spread pattern
    public float spreadAngle = 30f;    // degrees
    public FirePattern firePattern = FirePattern.SingleTarget;
}

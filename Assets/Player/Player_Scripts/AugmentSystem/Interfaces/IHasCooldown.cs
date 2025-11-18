public interface IHasCooldown
{
    float Cooldown { get; set; }
    void ReduceCooldown(float amount);
}

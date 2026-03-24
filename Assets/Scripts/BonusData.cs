using UnityEngine;
public enum BonusType
{
    Heal,
    SpeedBoost,
    Invincibility
}
[CreateAssetMenu(menuName = "Bonus Data")]
public class BonusData : ScriptableObject
{
    public BonusType bonusType;
    public int healAmount;
    public float speedMultiplier;
    public float effectDuration; 
}
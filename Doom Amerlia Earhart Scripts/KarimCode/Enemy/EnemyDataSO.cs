using UnityEngine;

/// <summary>
/// This is a scriptable object that holds the data definitions of each enemy type.
/// ex.
///		base crabs have 10 health
///		red crabs have 20 health
///		somethign else blah blah you get it
/// </summary>
[CreateAssetMenu(fileName = "EnemyStatBlock", menuName = "ScriptableObjects/CreateEnemyStatBlock", order = 1)]
public class EnemyDataSO : ScriptableObject
{
	public CrabType crabType;
	public int MaxHealth, Damage, CreditCost, score;
}

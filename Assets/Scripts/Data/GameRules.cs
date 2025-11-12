using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Rules")]
public class GameRules : ScriptableObject
{
    [Header("Penalties")]
    public int wrongForgePenalty = 5;

    [Header("Defeat Conditions")]
    public bool defeatOnOutOfMoves = true;
    public bool defeatOnTimeout = true;

    [Header("Score")]
    public bool clampScoreToZero = true;
}
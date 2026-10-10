using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceData", menuName = "Piece/Piece Data")]
public class PieceData : ScriptableObject {
    public string pieceName;
    public int cost;
    public Sprite mapIcon;
    public float costMultiplier; // Multiplied by the base to determine cost | 1.0-2.0 Rank One, 2.1-3.2 Rank Two, 3.3-4.0 Rank Three
    public float mvRgMultiplier = 0.1f; // Move range multiplier from 0.1 to 1.0, 1.0 being can move the whole length of the board
    public float healthMultiplier = 0.75f; // .75 to 3, multiplied by init average tiles

    // Stats
    public int initMaxHealth; // Piece types initial maximum health
    public int moveRange;
    public List<int> dice = new List<int>(); // A list of dice, with the ints being the number of sides of each die
    public string id; // Unique identifier for this piece type
    public Rank rank;
    public List<CardData> initCards;
    public bool hasRewardCards; // If true, then this piece will get a card(s) on every rewardCardRound turns
    public int rewardCardRound;
    public List<CardData> rewardCards;
}
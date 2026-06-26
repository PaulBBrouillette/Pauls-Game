using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPieceData", menuName = "Piece/Piece Data")]
public class PieceData : ScriptableObject {
    public string pieceName;
    public int cost = 0;
    public GameObject modelPrefab; // The 3D model for this specific piece
    public Sprite shopIcon;        // Image for the UI button
    public float multiplier; // Multiplied by the base to determine cost

    // Stats
    public int initMaxHealth = 10; // Piece types initial maximum health
    public int moveRange = 3;
    public List<int> dice = new List<int>(); // A list of dice, with the ints being the number of sides of each die
    public string id; // Unique identifier for this piece type
    public Rank rank;
}
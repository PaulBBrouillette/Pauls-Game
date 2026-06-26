using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Team team;
    public PieceElement element;
    public PieceData data;
    public TileSide currentSide;
    public bool isOnSideA; // corresponds to occupantA vs occupantB
    public float currentHealth;
    public int movesRemaining; // The amount of moves remaining for this piece for this turn
    public Tile currentTile;

    public void Initialize(PieceData data, Team team) {
        this.data = data;
        movesRemaining = data.moveRange;
        setTeam(team);
        GameObject visuals = Instantiate(data.modelPrefab, transform);
    }

    public Team getTeam() {
        return team;
    }

    public void setTeam(Team team) {
        this.team = team;
    }
    public PieceElement getElement() {
        return element;
    }

    public void SetElement(PieceElement element) {
        this.element = element;
    }

    public void SetPosition(TileSide side, bool isA) {
        currentSide = side;
        isOnSideA = isA;
    }

    public void ResetMoves() {
        movesRemaining = data.moveRange;
    }

    // Get move range, plus any extra range from effects
    public int getTotalMoveRange() {
        int totalMoves = movesRemaining;

        StatusHost host = GetComponent<StatusHost>();
        foreach (StatusEffect effect in host.getEffects()) {
            Debug.Log("Effect: " + effect.effectName);
            // Extra turns
            if (effect.id == CardId.N_XTRTRN) {
                totalMoves += effect.power;
            }
            // Bind, return 0 if found
            if (effect.id == CardId.N_BIND) { 
                return 0;
            }
        }
        return totalMoves;
    }

    public int[] RollTheDice() {
        Debug.Log($"{data.pieceName} RollTheDice() Start with {data.dice.Count} dice");
        int[] total = new int[data.dice.Count];

        for (int i = 0; i < total.Length; i++) {
            total[i] = Random.Range(1, data.dice[i] + 1);
        }

        return total;
    }
}

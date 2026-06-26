using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Shared Sides")]
    public TileSide topSide;
    public TileSide bottomSide;
    public TileSide leftSide;
    public TileSide rightSide;

    public Tile topNeighbor;
    public Tile bottomNeighbor;
    public Tile leftNeighbor;
    public Tile rightNeighbor;

    public Team team;
    public Vector2Int gridPos;
    private Renderer rend;

    void Start() {
        rend = GetComponent<Renderer>();   
    }

    public List<Piece> GetAllPiecesOnTile() {
        List<Piece> pieces = new List<Piece>();
        // Check all 4 sides. Remember: we check the specific occupant 
        // slot (A or B) that belongs to THIS tile.
        AddOccupant(topSide, pieces);
        AddOccupant(bottomSide, pieces);
        AddOccupant(leftSide, pieces);
        AddOccupant(rightSide, pieces);
        return pieces;
    }

    private void AddOccupant(TileSide side, List<Piece> list) {
        // We check which side of the TileSide corresponds to this tile
        GameObject go = (side.tileA == this) ? side.occupantA : side.occupantB;
        if (go != null) list.Add(go.GetComponent<Piece>());
    }

    public void setTeam(Team team) {
        this.team = team;
        rend = GetComponent<Renderer>();
        if (rend != null) {
            switch (team) {
                case (Team.One): rend.material.color = Color.red; break;
                case (Team.Two): rend.material.color = Color.blue; break;
                case (Team.Three): rend.material.color = Color.yellow; break;
                case (Team.Four): rend.material.color = Color.green; break;
            }
        }
    }
}
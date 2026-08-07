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

    public SpriteRenderer baseRenderer;
    public SpriteRenderer overlayRenderer;

    void Start() {
        baseRenderer = GetComponent<SpriteRenderer>();   
    }

    public List<Piece> GetAllPiecesOnTile() {
        List<Piece> pieces = new List<Piece>();
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
        baseRenderer = GetComponent<SpriteRenderer>();
        if (baseRenderer != null) {
            baseRenderer.color = TeamColorHelper.GetTeamColor(team);
        }
    }
}
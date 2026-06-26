using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSide {
    public Tile tileA;
    public Tile tileB;
    public GameObject occupantA; // Object belonging to tileA
    public GameObject occupantB; // Object belonging to tileB

    public List<TileSideConnection> connections = new List<TileSideConnection>();

    // Helper to see if a specific tile has an occupant on this side
    public bool IsOccupiedBy(Tile tile) {
        if (tile == tileA) return occupantA != null;
        if (tile == tileB) return occupantB != null;
        return false;
    }
}
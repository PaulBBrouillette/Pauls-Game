using System.Collections.Generic;
using UnityEngine;

public class ChangeTileTeam : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        if (targetTile != null) {
            List<Piece> list = targetTile.GetAllPiecesOnTile();
            foreach (Piece p in list) {
                if (p != null) {
                    p.team = owner.team;
                }
            }
            // Placeholder change the color of the tile
            Color c = Color.red;
            if (owner.team == Team.One) {
                c = Color.red;
            }
            else if (owner.team == Team.Two) {
                c = Color.blue;
            }
            else if (owner.team == Team.Three) {
                c = Color.green;
            }
            else {
                c = Color.yellow;
            }
            targetTile.GetComponent<Renderer>().material.color = c;
            targetTile.team = owner.team;
            owner.ownedTiles++;
            targetOwner.ownedTiles--;
        }
    }
}

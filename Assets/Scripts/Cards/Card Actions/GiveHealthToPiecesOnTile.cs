using System.Collections.Generic;
using UnityEngine;

public class GiveHealthToPiecesOnTile : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        List<Piece> pieces = targetTile.GetAllPiecesOnTile();
        if (pieces.Count == 0) return;
        foreach (Piece piece in pieces) piece.currentHealth += Mathf.CeilToInt(piece.data.initMaxHealth * effectAmount);
    }
}
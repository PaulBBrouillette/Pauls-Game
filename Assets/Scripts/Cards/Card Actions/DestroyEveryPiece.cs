using System.Collections.Generic;
using UnityEngine;

public class DestroyEveryPiece : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        List<Piece> pieces = GameplayManager.Instance.piecesOnBoard;
        foreach (Piece piece in pieces) {
            UnityEngine.Object.Destroy(piece.gameObject);
        }
        GameplayManager.Instance.piecesOnBoard.Clear();
    }
}
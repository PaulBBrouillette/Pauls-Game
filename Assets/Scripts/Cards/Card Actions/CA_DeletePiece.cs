
public class CA_DeletePiece : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        GameplayManager.Instance.piecesOnBoard.Remove(targetPiece);
        UnityEngine.Object.Destroy(targetPiece.gameObject);
    }
}

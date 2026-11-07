[System.Serializable]
public abstract class CardAction {

    // Start method for this effect
    public void Activate(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, int duration, bool existsUntilDestroyed, float effectAmount, CardContext context) {
        DoAction(owner, targetOwner, targetPiece, secondPiece, targetTile, secondTile, effectAmount, duration, existsUntilDestroyed, context);
    }

    // Do the effect
    public abstract void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed, CardContext context);
}
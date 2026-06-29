[System.Serializable]
public abstract class CardAction {

    public int duration;
    public bool existsUntilDestroyed;
    public float effectAmount;

    // Start method for this effect
    public void Activate(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, int duration, bool existsUntilDestroyed, float effectAmount) {
        this.duration = duration;
        this.existsUntilDestroyed = existsUntilDestroyed;
        this.effectAmount = effectAmount;
        DoAction(owner, targetOwner, targetPiece, secondPiece, targetTile, secondTile, effectAmount, duration, existsUntilDestroyed);
    }

    // Do the effect
    public abstract void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed);
}
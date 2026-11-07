using UnityEngine;

public class StillRemains : CardAction
{
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed, CardContext context) {
        StatusHost host = targetPiece.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("StillRemains", duration, (int)effectAmount, existsUntilDestroyed, CardId.AB_STLRMN, context));
        Debug.Log("Survive Battle effect added!");
    }
}

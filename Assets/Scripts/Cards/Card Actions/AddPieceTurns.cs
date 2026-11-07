using UnityEngine;
public class AddPieceTurns : CardAction {

    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed, CardContext context) {
        StatusHost host = targetPiece.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("ExtraTurns", duration, (int)effectAmount, existsUntilDestroyed, CardId.N_XTRTRN, context));
        Debug.Log("Extra turns effect added!");
    }
}

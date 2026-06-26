using UnityEngine;
public class CA_AddTurns : CardAction {

    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        StatusHost host = targetPiece.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("ExtraTurns", duration, (int)effectAmount, existsUntilDestroyed, EffectType.ExtraMove, CardId.N_XTRTRN));
        Debug.Log("Extra turns effect added!");
    }
}

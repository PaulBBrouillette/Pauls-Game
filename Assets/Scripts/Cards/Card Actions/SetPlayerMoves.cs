using UnityEngine;

public class SetPlayerMoves : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed, CardContext context) {
        StatusHost host = targetOwner.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("HomeopathicDelusion", duration, (int)effectAmount, existsUntilDestroyed, CardId.N_HPTHDL, context));
        Debug.Log("Effect added!");
    }
}
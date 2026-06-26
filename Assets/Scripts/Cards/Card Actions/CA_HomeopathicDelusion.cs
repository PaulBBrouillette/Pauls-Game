using UnityEngine;

public class CA_HomeopathicDelusion : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        StatusHost host = targetOwner.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("HomeopathicDelusion", duration, (int)effectAmount, existsUntilDestroyed, EffectType.ExtraMove, CardId.N_HPTHDL));
        Debug.Log("Effect added!");
    }

}

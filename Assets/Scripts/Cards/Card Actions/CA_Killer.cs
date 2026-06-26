using UnityEngine;

[System.Serializable]
public class CA_Killer : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        StatusHost host = targetPiece.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("Killer", duration, (int)effectAmount, existsUntilDestroyed, EffectType.BattleStartEnd, CardId.AB_KLLER));
        Debug.Log("Killer effect added!");
    }
}

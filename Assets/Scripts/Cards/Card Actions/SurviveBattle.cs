using UnityEngine;

[System.Serializable]
public class SurviveBattle : CardAction {

    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        StatusHost host = targetPiece.GetComponent<StatusHost>();
        host.AddEffect(new StatusEffect("SurviveBattle", duration, (int)effectAmount, existsUntilDestroyed, CardId.AB_SRVBTL));
        Debug.Log("Survive Battle effect added!");
    }
}

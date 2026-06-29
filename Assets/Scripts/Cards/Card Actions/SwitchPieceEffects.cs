using System.Collections.Generic;
using System.Linq;

public class SwitchPieceEffects : CardAction {
    public override void DoAction(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile, float effectAmount, int duration, bool existsUntilDestroyed) {
        List<StatusEffect> fpEffects = new();
        List<StatusEffect> spEffects = new();
        StatusHost sh1 = targetPiece != null ? targetPiece.GetComponent<StatusHost>() : null;
        StatusHost sh2 = secondPiece != null ? secondPiece.GetComponent<StatusHost>() : null;

        if (sh1 != null) {
            fpEffects = sh1.getEffects().ToList();
            sh1.RemoveAllEffects();
        }

        if (sh2 != null) {
            spEffects = sh2.getEffects().ToList();
            sh2.RemoveAllEffects();
        }

        if (sh1 != null) {
            sh1.AddMultipleEffects(spEffects);
        }

        if (sh2 != null) {
            sh2.AddMultipleEffects(fpEffects);
        }
    }
}
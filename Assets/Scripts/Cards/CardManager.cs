using UnityEngine;

public class CardManager : MonoBehaviour {
    public static CardManager Instance;
    private CardDisplay selectedGraphic;

    void Awake() => Instance = this;

    // Do whatever action(s) this card can
    public void ActivateCard(Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile) {
        if (selectedGraphic == null) return;

        // Execute the logic inside the ScriptableObject
        //selectedGraphic.cardData.Play(GameplayManager.Instance.currentPlayerScript, targetPiece, secondPiece, targetTile, secondTile);

        // Remove the card from the UI/Hand
        //Destroy(selectedGraphic.gameObject);
        //selectedGraphic = null;
    }
}
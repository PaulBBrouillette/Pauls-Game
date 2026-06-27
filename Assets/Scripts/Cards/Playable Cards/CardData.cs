using System.Collections.Generic;
using UnityEngine;

public enum CardContext { MainMap, BattleStart, BattleEnd } // For right now cards can only be activated on the main map but CardContext tells where they will be activated, i.e. Bear One Another's Burdens will be CardContext MainMap and CardType OneTime which will activate immediately, but Wiederganger will be Imbue and BattleEnd and only be activated at the end of a battle 
public enum TargetType { FriendlyPiece, EnemyPiece, MultiSelectPiece, FriendlyTile, EnemyTile, MultiSelectTile, MyTeam, EnemyTeam } // After selecting a card, this will determine what targets you can choose from
public enum CardType { Imbue, Single } // Used in CardAction to determine if to apply an imbued effect to be used overtime or at a later time or do an action immediately

[CreateAssetMenu(menuName = "Cards/New Card")]
public class CardData : ScriptableObject {
    public string cardName;
    public CardContext context;
    public TargetType targetType;
    public CardType cardType;
    [SerializeReference, SelectSubclass]
    public List<CardAction> actions = new List<CardAction>();
    public int turnDuration = -1; // If imbued, how many turns the effect will last. If it will last forever, then it is -1
    public bool existsUntilDestroyed; // If true, then this will exist forever until it is used or the piece is destroyed
    public string id; // Unique identifier for this card for purposes of referencing it
    public Sprite shopIcon;
    public int cost;
    public float costMultiplier; // 1.0-2.0 Rank One, 2.1-3.2 Rank Two, 3.3-4.0 Rank Three
    public float effectAmount; // Amount if this card adds things, such as extra turns
    public Rank rank;

    public void PlayCard(Player owner, Player targetOwner, Piece targetPiece, Piece secondPiece, Tile targetTile, Tile secondTile) {
        foreach (CardAction action in actions) {
            action.Activate(owner, targetOwner, targetPiece, secondPiece, targetTile, secondTile, turnDuration, existsUntilDestroyed, effectAmount);
            Debug.Log("CardData: Calling Activate on card");
        }
    }
}
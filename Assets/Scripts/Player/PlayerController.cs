using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    public int initPieceMoves = 10; // At the beginning of a turn, set the amount of pieces moves to this amount
    public int remainingPieceMoves;
    public int money;
    public Team team;
    public PlayerType type;
    public List<CardData> cardsInStock; // Purchased cards from the store
    public List<PieceData> piecesInStock; // Purchased pieces from the store
    public List<CardDisplay> imbuedCards;
    public int ownedTiles;

    public void setTeam(Team team) {
        this.team = team;
    }

    public Team getTeam() {
        return this.team;
    }

    public void setMoney(int money) {
        this.money = money;
    }

    public void AddMoney(int money) {
        this.money += money;
    }

    public void setPlayerType(PlayerType type) {
        this.type = type;
    }

    public PlayerType getPlayerType() {
        return this.type;
    }

    public void ResetMoves() {
        this.remainingPieceMoves = initPieceMoves;
    }

    public void AddPieceToStock(PieceData piece) {
        piecesInStock.Add(piece);
    }

    public void RemovePieceFromStock(PieceData piece) {
        piecesInStock.Remove(piece);
    }

    public void AddCardToStock(CardData card) {
        cardsInStock.Add(card);
    }

    public void RemoveCardFromStock(CardData card) { 
        cardsInStock.Remove(card);
    }

    public void ImbueCard(CardDisplay card) {
        imbuedCards.Add(card);
    }

    public void RemoveImbueCard(CardDisplay card) {
        imbuedCards.Remove(card);
    }
}

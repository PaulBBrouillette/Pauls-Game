public enum TurnPhase {
    RoundOneStart,
    Start,
    RoundOneWait, // First round, every player can only place pieces but not move
    RoundOneTurnEnd, // During RoundOne, this replaces TurnEnd
    Wait,
    MovePiece, // Currently moving existing piece
    Buy,
    PlacePiece, // After buying, pieces go to the stock. This is for placing those pieces
    ChooseCardTarget, // When waiting and then selecting a card, this is for choosing targets
    ChooseSecondCardTarget, // For multi select cards
    ChoosePlayer, // When selecting a card that targets players
    AwaitYesNoSelection, // When selecting a card that targets yourself to prevent misclicks since every other card requires you to drag the mouse
    Attack,
    BattleStart,
    BattleEnd,
    TurnEnd,
    AfterCaptureCheck, // After a tile is captured, check if a player has anymore remaining tiles
    GameEnd
}
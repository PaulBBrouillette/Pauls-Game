[System.Serializable]
public class StatusEffect {
    public string effectName;
    public CardId id;
    public int remainingTurns;
    public int power; // The value (e.g., +2 moves, +5 attack)
    public bool existsUntilDestroyed;
    public CardContext context;

    public StatusEffect(string name, int duration, int value, bool existsUntilDestroyed, CardId id, CardContext context) {
        effectName = name;
        remainingTurns = duration;
        power = value;
        this.existsUntilDestroyed = existsUntilDestroyed;
        this.id = id;
        this.context = context;
    }

    // Called at the start/end of the turn to tick down
    // Returns true if it should be removed
    public bool Tick() {
        if (existsUntilDestroyed) return false;
        if (context == CardContext.Battle) return false;
        remainingTurns--;
        return remainingTurns <= 0; 
    }

    public bool BattleTick() {
        if (existsUntilDestroyed) return false;
        if (context == CardContext.MainMap) return false;
        remainingTurns--;
        return remainingTurns <= 0;
    }
}
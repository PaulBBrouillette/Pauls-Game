[System.Serializable]
public class StatusEffect {
    public string effectName;
    public CardId id;
    public int remainingTurns;
    public int power; // The value (e.g., +2 moves, +5 attack)
    public bool existsUntilDestroyed;

    public StatusEffect(string name, int duration, int value, bool existsUntilDestroyed, CardId id) {
        effectName = name;
        remainingTurns = duration;
        power = value;
        this.existsUntilDestroyed = existsUntilDestroyed;
        this.id = id;
    }

    // Called at the start/end of the turn to tick down
    // Returns true if it should be removed
    public bool Tick() {
        if (existsUntilDestroyed) return false;
        remainingTurns--;
        return remainingTurns <= 0; 
    }
}
using System.Collections.Generic;
using UnityEngine;

public class StatusHost : MonoBehaviour {
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    public void AddEffect(StatusEffect newEffect) {
        activeEffects.Add(newEffect);
    }

    public void ProcessTurn() {
        // Remove effects that hit 0 turns
        activeEffects.RemoveAll(e => e.Tick());
    }

    public List<StatusEffect> getEffects() {
        return activeEffects;
    }

    // Manually remove an effect from the list as opposed to waiting for the duration to tick down
    public void RemoveEffect(StatusEffect effect) {
        activeEffects.Remove(effect);
    }
}
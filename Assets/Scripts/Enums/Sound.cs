using UnityEngine;

public enum SoundType {
    BATTLE_HIT,
    UI_SELECT
}

[System.Serializable]
public struct SoundGroup {
    public SoundType type;
    public AudioClip[] clips;
}
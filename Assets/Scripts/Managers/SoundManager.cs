using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    [Header("Audio Channels")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Data")]
    [SerializeField] private List<SoundGroup> soundGroups;

    private Dictionary<SoundType, AudioClip[]> soundDictionary = new Dictionary<SoundType, AudioClip[]>();

    private void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        foreach (var group in soundGroups) {
            if (!soundDictionary.ContainsKey(group.type)) {
                soundDictionary.Add(group.type, group.clips);
            }
        }
    }

    public void PlaySoundByType(SoundType type) {
        if (soundDictionary.TryGetValue(type, out AudioClip[] clips)) {
            if (clips.Length == 0) return;

            int randomIndex = Random.Range(0, clips.Length);
            AudioClip clipToPlay = clips[randomIndex];

            sfxSource.PlayOneShot(clipToPlay);
        }
        else {
            Debug.LogWarning($"Sound type {type} not found in AudioManager!");
        }
    }
}
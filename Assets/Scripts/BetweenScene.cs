using UnityEngine;

public class BetweenScene : MonoBehaviour
{
    // Default some values for testing purposes and not starting on the main menu
    public int gridWidth = 7;
    public int gridHeight = 7;
    public int numPlayers = 2;
    public MapLayout mapLayout = MapLayout.Grouped;
    public static BetweenScene Instance { get; private set; }
    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
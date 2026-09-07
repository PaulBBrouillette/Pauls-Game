using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour {

    [SerializeField] private ToggleGroup numTeamsOptions;
    [SerializeField] private ToggleGroup mapSizeOptions;
    [SerializeField] private ToggleGroup mapTypeOptions;
    [SerializeField] private GameObject playSettingsPanel;

    [SerializeField] private UIDocument uiDocument;
    private UnityEngine.UIElements.Button playButton;
    private UnityEngine.UIElements.Button quitButton;
    private UnityEngine.UIElements.Button backButton;
    private UnityEngine.UIElements.Button launchGameButton;
    private RadioButtonGroup teams;
    private RadioButtonGroup size;
    private RadioButtonGroup type;
    private VisualElement mainPanel;
    private VisualElement gameOptionsPanel;

    void Start() {
        VisualElement root = uiDocument.rootVisualElement;

        teams = root.Q<RadioButtonGroup>("NumTeamsGroup");
        size = root.Q<RadioButtonGroup>("MapSizeGroup");
        type = root.Q<RadioButtonGroup>("MapTypeGroup");
        teams.value = 0;
        size.value = 0;
        type.value = 0;

        playButton = root.Q<UnityEngine.UIElements.Button>("StartGame");
        quitButton = root.Q<UnityEngine.UIElements.Button>("Quit");
        backButton = root.Q<UnityEngine.UIElements.Button>("Back");
        launchGameButton = root.Q<UnityEngine.UIElements.Button>("LaunchGame");
        mainPanel = root.Q<VisualElement>("MainMenuContainer");
        gameOptionsPanel = root.Q<VisualElement>("GameOptionsContainer");

        playButton.clicked += OpenGameOptions;
        quitButton.clicked += Quit;
        backButton.clicked += CloseGameOptions;
        launchGameButton.clicked += LaunchGame;
    }
    public void Quit() {
        SoundManager.Instance.PlaySoundByType(SoundType.UI_SELECT);
        Application.Quit();
        Debug.Log("Quit bro");
    }

    public void OpenGameOptions() {
        SoundManager.Instance.PlaySoundByType(SoundType.UI_SELECT);
        mainPanel.style.display = DisplayStyle.None;
        gameOptionsPanel.style.display = DisplayStyle.Flex;
    }

    public void CloseGameOptions() {
        SoundManager.Instance.PlaySoundByType(SoundType.UI_SELECT);
        mainPanel.style.display = DisplayStyle.Flex;
        gameOptionsPanel.style.display = DisplayStyle.None;
    }

    public void LaunchGame() {
        SoundManager.Instance.PlaySoundByType(SoundType.UI_SELECT);
        Debug.Log("Launch game start");
        BetweenScene.Instance.numPlayers = teams.value + 2;
        if (type.value == 0) {
            BetweenScene.Instance.mapLayout = MapLayout.Grouped;
        }
        else {
            BetweenScene.Instance.mapLayout = MapLayout.Scrambled;
        }
        BetweenScene.Instance.gridWidth = GetDimension(size.value);
        BetweenScene.Instance.gridHeight = GetDimension(size.value);
        Debug.Log("Launch game about to transition scenes");
        SceneManager.LoadScene("MainGame");
    }

    public void Play() {
        // Choose from the player amounts and map sizes, random for now
        string teams = numTeamsOptions.ActiveToggles().FirstOrDefault().name;
        string mapSize = mapSizeOptions.ActiveToggles().FirstOrDefault().name;
        string mapType = mapTypeOptions.ActiveToggles().FirstOrDefault().name;
        Debug.Log($"Selected {teams} teams, {mapSize} map size, and Scrambled map type");
        BetweenScene.Instance.numPlayers = int.Parse(teams);
        //BetweenScene.Instance.gridWidth = GetDimension(mapSize);
        //BetweenScene.Instance.gridHeight = GetDimension(mapSize);
        if (mapType.Equals("Grouped")) {
            BetweenScene.Instance.mapLayout = MapLayout.Grouped;
        }
        else {
            BetweenScene.Instance.mapLayout = MapLayout.Scrambled;
        }
            
        SceneManager.LoadScene("MainGame");
    }

    int GetDimension(int mapSize) {
        if (mapSize == 0) {
            return Random.Range(6, 8);
        }
        else if (mapSize == 1) {
            return Random.Range(9, 13);
        }
        else if (mapSize == 2) {
            return Random.Range(14, 19);
        }
        else {
            return 7;
        }
    }
}

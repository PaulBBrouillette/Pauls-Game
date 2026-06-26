using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

    [SerializeField] private ToggleGroup numTeamsOptions;
    [SerializeField] private ToggleGroup mapSizeOptions;
    [SerializeField] private ToggleGroup mapTypeOptions;
    [SerializeField] private GameObject playSettingsPanel;
    public void Quit() {
        Application.Quit();
    }

    public void OpenGameOptions() {
        playSettingsPanel.SetActive(true);
    }

    public void CloseGameOptions() {
        playSettingsPanel.SetActive(false);
    }

    public void Play() {
        // Choose from the player amounts and map sizes, random for now
        string teams = numTeamsOptions.ActiveToggles().FirstOrDefault().name;
        string mapSize = mapSizeOptions.ActiveToggles().FirstOrDefault().name;
        string mapType = mapTypeOptions.ActiveToggles().FirstOrDefault().name;
        Debug.Log($"Selected {teams} teams, {mapSize} map size, and Scrambled map type");
        BetweenScene.Instance.numPlayers = int.Parse(teams);
        BetweenScene.Instance.gridWidth = GetDimension(mapSize);
        BetweenScene.Instance.gridHeight = GetDimension(mapSize);
        if (mapType.Equals("Grouped")) {
            BetweenScene.Instance.mapLayout = MapLayout.Grouped;
        }
        else {
            BetweenScene.Instance.mapLayout = MapLayout.Scrambled;
        }
            
        SceneManager.LoadScene("MainGame");
    }

    int GetDimension(string mapSize) {
        if (mapSize.Equals("Small")) {
            return Random.Range(5, 8);
        }
        else if (mapSize.Equals("Medium")) {
            return Random.Range(9, 13);
        }
        else if (mapSize.Equals("Large")) {
            return Random.Range(17, 22);
        }
        else {
            return 7;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

public class DiceTest : MonoBehaviour {
    [SerializeField] private List<DiceUI> dice;

    void Update() {
        if (InputManager.Controls.Player.BattleAdvance.WasPressedThisFrame()) {
            foreach (DiceUI die in dice) {
                StartCoroutine(die.Roll(Random.Range(1, 12)));
            }
        }
    }
}
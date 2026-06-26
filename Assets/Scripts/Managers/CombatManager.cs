using System.Linq;
using UnityEngine;
using TMPro;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;
    [SerializeField] private TextMeshProUGUI atkDiceText;
    [SerializeField] private TextMeshProUGUI dfdDiceText;
    [SerializeField] private TextMeshProUGUI atkHealthText;
    [SerializeField] private TextMeshProUGUI dfdHealthText;
    [SerializeField] private TextMeshProUGUI resultText;

    void Awake() => Instance = this;

    public void Attack(Piece attacker, Piece defender, bool isSneakAttack) {
        int[] atkDice = attacker.RollTheDice();
        int[] dfdDice = defender.RollTheDice();

        var dStrings = GetDiceText(attacker, defender, atkDice, dfdDice, isSneakAttack);
        atkDiceText.text = dStrings.atk;
        dfdDiceText.text = dStrings.dfd;

        float attackerDmg = (float)atkDice.Sum();
        float defenderDmg = (float)dfdDice.Sum();
        if (isSneakAttack) { attackerDmg *= 1.5f; }
        Debug.Log("Attack turn start");
        
        attacker.currentHealth -= defenderDmg;
        defender.currentHealth -= attackerDmg;

        Debug.Log($"Attacker health: {attacker.currentHealth} | Defender Health: {defender.currentHealth}");

        bool battleDone = HandleAfterBattle(attacker, defender);
        if (battleDone) {
            ShowResult(attacker, defender);
            GameplayManager.Instance.SetPhase(TurnPhase.BattleEnd);
        }
        Debug.Log("Attack turn end");
    }

    // Returns true if battle is over, false otherwise
    private bool HandleAfterBattle(Piece attacker, Piece defender) {
        bool battleDone = false;
        bool destroyAtk = false;
        bool destroyDfd = false;

        if (attacker.currentHealth <= 0) {
            destroyAtk = true;
        }
        if (defender.currentHealth <= 0) {
            destroyDfd = true;
        }

        var atkEffects = DoBattleEffect(attacker, defender, destroyAtk, battleDone);
        var dfdEffects = DoBattleEffect(defender, attacker, destroyDfd, battleDone);

        destroyAtk = atkEffects.destroy;
        destroyDfd = dfdEffects.destroy;
        if (atkEffects.battleDone && dfdEffects.battleDone) {
            battleDone = true;
        }

        if (destroyAtk) {
            GameplayManager.Instance.piecesOnBoard.Remove(attacker);
            Destroy(attacker.gameObject);
            Debug.Log("Attacker was destroyed");
            battleDone = true;
        }
        if (destroyDfd) {
            GameplayManager.Instance.piecesOnBoard.Remove(defender);
            Destroy(defender.gameObject);
            Debug.Log("Defender was destroyed");
            battleDone = true;
        }
        UpdateUI(attacker, defender);

        return battleDone;
    }

    public (bool destroy, bool battleDone) DoBattleEffect(Piece piece1, Piece piece2, bool destroy, bool battleDone) {
        StatusHost aHost = piece1.gameObject.GetComponent<StatusHost>();
        if (aHost != null) {
            for (int i = aHost.getEffects().Count - 1; i >= 0; i--) {
                StatusEffect effect = aHost.getEffects()[i];
                switch (effect.id) {
                    case CardId.AB_SRVBTL: // Survive battle
                        if (piece1.currentHealth <= 0) {
                            Debug.Log($"{piece1.name} is supposed to die, but has Survive Battle");
                            aHost.RemoveEffect(effect);
                            piece1.currentHealth = 1;
                            battleDone = true;
                            destroy = false;
                            Debug.Log("Survive Battle activated!");
                        }
                        break;

                    case CardId.AB_STLRMN: // Still remains (gain negative health from other piece)
                        if (piece2.currentHealth <= 0) {
                            aHost.RemoveEffect(effect);
                            float hth = Mathf.Abs(piece2.currentHealth);
                            piece1.currentHealth += hth;
                            Debug.Log("Still Remains activated!");
                        }
                        break;
                }
            }
        }
        return (destroy, battleDone);
    }

    public (string atk, string dfd) GetDiceText(Piece attacker, Piece defender, int[] atkDice, int[] dfdDice, bool isSneakAttack) {
        string atkDiceString = "(";
        string dfdDiceString = "(";

        foreach (int num in atkDice) {
            atkDiceString += num.ToString() + " + ";
        }
        foreach (int num in dfdDice) {
            dfdDiceString += num.ToString() + " + ";
        }
        atkDiceString = atkDiceString.Substring(0, atkDiceString.Length - 3);
        dfdDiceString = dfdDiceString.Substring(0, dfdDiceString.Length - 3);
        if (isSneakAttack) {
            atkDiceString += ") * 1.5 = " + (float)atkDice.Sum() * 1.5;
        }
        else {
            atkDiceString += ") = " + atkDice.Sum();
        }
        dfdDiceString += ") = " + dfdDice.Sum();

        return (atkDiceString, dfdDiceString);
    }

    private void UpdateUI(Piece attacker, Piece defender) {
        atkHealthText.text = attacker.currentHealth.ToString();
        dfdHealthText.text = defender.currentHealth.ToString();
    }

    public void ResetUI() {
        atkHealthText.text = "";
        dfdHealthText.text = "";
        atkDiceText.text = "";
        dfdDiceText.text = "";
        resultText.text = "";
    }

    // Maybe also play sounds here too for when they die and do animations
    private void ShowResult(Piece attacker, Piece defender) {
        if (attacker.currentHealth <= 0 && defender.currentHealth <= 0) {
            resultText.text = "Both destroyed";
        }
        else if (attacker.currentHealth <= 0) {
            resultText.text = "Attacker destroyed";
        }
        else if (defender.currentHealth <= 0) {
            resultText.text = "Defender destroyed";
        }
    }
}

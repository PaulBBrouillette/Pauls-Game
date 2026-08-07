using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;
    private enum CombatPhase { Start, During, RollingDice, AfterDiceTest, End, Inactive }
    private CombatPhase phase;

    [SerializeField] private TextMeshProUGUI atkDiceText;
    [SerializeField] private TextMeshProUGUI dfdDiceText;
    [SerializeField] private TextMeshProUGUI atkHealthText;
    [SerializeField] private TextMeshProUGUI dfdHealthText;
    [SerializeField] private TextMeshProUGUI resultText;
    private Piece attacker;
    private Piece defender;
    private bool isSneakAttack;
    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        phase = CombatPhase.Inactive;
    }

    public void BeginCombat(Piece attacker, Piece defender, bool isSneakAttack) {
        this.attacker = attacker;
        this.defender = defender;
        this.isSneakAttack = isSneakAttack;
        phase = CombatPhase.Start;
    }

    void Update() {
        switch (phase) {
            case CombatPhase.Start:
                ExecutePreBattlePhase(attacker, defender);
                break;

            case CombatPhase.During:
                if (InputManager.Controls.Player.BattleAdvance.WasPressedThisFrame()) {
                    Attack(attacker, defender, isSneakAttack);
                }
                break;

            case CombatPhase.End:
                break;

            case CombatPhase.RollingDice:

                break;

            case CombatPhase.AfterDiceTest:
                Debug.Log("After Dice Test");
                break;

            case CombatPhase.Inactive:
                break;
        }
    }

    public void Attack(Piece attacker, Piece defender, bool isSneakAttack) {
        Debug.Log("--- Combat Initiated ---");

        // 2. MID-BATTLE PHASE: Handle dice rolls, math calculations, and on-roll cards (Coin Flips)
        float attackerDmg = 0f;
        float defenderDmg = 0f;
        ExecuteMidBattlePhase(attacker, defender, isSneakAttack, out attackerDmg, out defenderDmg);

        // 3. DAMAGE APPLICATION: Apply calculated values to health pools
        ApplyDamage(attacker, defender, attackerDmg, defenderDmg);

        // 4. POST-BATTLE PHASE: Process survival abilities, clean up dead pieces, and update UI
        bool battleDone = ExecutePostBattlePhase(attacker, defender);

        if (battleDone) {
            ShowResult(attacker, defender);
            GameplayManager.Instance.SetPhase(TurnPhase.BattleEnd);
            phase = CombatPhase.Inactive;
        }

        Debug.Log("--- Combat Ended ---");
    }

    private void ExecutePreBattlePhase(Piece attacker, Piece defender) {
        Debug.Log("Pre-Battle Phase: Checking initialization cards...");
        // Place any effects that trigger before rolling cards/dice here
        resultText.text = "Press Space to roll!";
        UpdateUI(attacker, defender);
        phase = CombatPhase.During;
    }

    private void ExecuteMidBattlePhase(Piece attacker, Piece defender, bool isSneakAttack, out float attackerDmg, out float defenderDmg) {
        int[] atkDice = attacker.RollTheDice();
        int[] dfdDice = defender.RollTheDice();

        attackerDmg = atkDice.Sum();
        defenderDmg = dfdDice.Sum();

        // Evaluate mid-battle cards/status effects for both participants
        attackerDmg = EvaluateMidBattleEffects(attacker, attackerDmg);
        defenderDmg = EvaluateMidBattleEffects(defender, defenderDmg);

        if (isSneakAttack) {
            attackerDmg *= 1.5f;
            Debug.Log("Sneak Attack applied: x1.5 damage");
        }
        UpdateDiceDisplayText(attacker, defender, atkDice, dfdDice, isSneakAttack);
    }

    private float EvaluateMidBattleEffects(Piece piece, float baseDamage) {
        StatusHost host = piece.GetComponent<StatusHost>();
        if (host == null) return baseDamage;

        float modifiedDamage = baseDamage;
        foreach (StatusEffect effect in host.getEffects()) {
            if (effect.id == CardId.N_CONFLP) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    modifiedDamage *= 2.0f;
                    Debug.Log($"{piece.name} Coin Flip: x2");
                }
                else {
                    modifiedDamage *= 0.25f;
                    Debug.Log($"{piece.name} Coin Flip: x0.25");
                }
            }
        }
        return modifiedDamage;
    }

    private void ApplyDamage(Piece attacker, Piece defender, float attackerDmg, float defenderDmg) {
        attacker.currentHealth -= defenderDmg;
        defender.currentHealth -= attackerDmg;

        Debug.Log($"Attacker Health: {attacker.currentHealth} | Defender Health: {defender.currentHealth}");
    }

    private bool ExecutePostBattlePhase(Piece attacker, Piece defender) {
        bool destroyAtk = attacker.currentHealth <= 0;
        bool destroyDfd = defender.currentHealth <= 0;
        bool battleDone = false;

        // Process post-battle status effects (Survive Battle, Still Remains, etc.)
        var atkEffects = ProcessPostBattleEffects(attacker, defender, destroyAtk);
        var dfdEffects = ProcessPostBattleEffects(defender, attacker, destroyDfd);

        destroyAtk = atkEffects.destroy;
        destroyDfd = dfdEffects.destroy;

        if (atkEffects.battleDone || dfdEffects.battleDone) {
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

    private (bool destroy, bool battleDone) ProcessPostBattleEffects(Piece self, Piece opponent, bool isMarkedForDeath) {
        StatusHost host = self.GetComponent<StatusHost>();
        bool battleDone = false;

        if (host == null) return (isMarkedForDeath, battleDone);

        for (int i = host.getEffects().Count - 1; i >= 0; i--) {
            StatusEffect effect = host.getEffects()[i];
            switch (effect.id) {
                case CardId.AB_SRVBTL: // Survive battle
                    if (isMarkedForDeath) {
                        Debug.Log($"{self.name} is supposed to die, but activated Survive Battle");
                        host.RemoveEffect(effect);
                        self.currentHealth = 1;
                        battleDone = true;
                        isMarkedForDeath = false;
                    }
                    break;

                case CardId.AB_STLRMN: // Still remains (gain health from opponent's negative health)
                    if (opponent.currentHealth < 0) {
                        host.RemoveEffect(effect);
                        float bonusHealth = Mathf.Abs(opponent.currentHealth);
                        self.currentHealth += bonusHealth;
                        Debug.Log($"{self.name} activated Still Remains, gained {bonusHealth} health!");
                    }
                    break;
            }
        }

        return (isMarkedForDeath, battleDone);
    }

    private void UpdateDiceDisplayText(Piece attacker, Piece defender, int[] atkDice, int[] dfdDice, bool isSneakAttack) {
        string atkString = $"({string.Join(" + ", atkDice)})";
        string dfdString = $"({string.Join(" + ", dfdDice)})";

        if (isSneakAttack) {
            atkDiceText.text = $"{atkString} * 1.5 = {(float)atkDice.Sum() * 1.5f}";
        }
        else {
            atkDiceText.text = $"{atkString} = {atkDice.Sum()}";
        }

        dfdDiceText.text = $"{dfdString} = {dfdDice.Sum()}";
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
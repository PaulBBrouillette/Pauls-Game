using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance;
    private enum CombatPhase { Start, WaitForInput, RollingDice, NoInputAllowed, End, Inactive }
    private CombatPhase phase;

    [SerializeField] private TextMeshProUGUI atkDiceText;
    [SerializeField] private TextMeshProUGUI dfdDiceText;
    [SerializeField] private TextMeshProUGUI atkHealthText;
    [SerializeField] private TextMeshProUGUI dfdHealthText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private GameObject UIDie;
    private Piece attacker, defender;
    private bool isSneakAttack;
    private StatusHost aHost, dHost, thisHost, otherHost;
    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        phase = CombatPhase.Inactive;
    }

    public void BeginCombat(Piece attacker, Piece defender, bool isSneakAttack) {
        this.attacker = attacker;
        this.defender = defender;
        this.isSneakAttack = isSneakAttack;
        aHost = attacker.GetComponent<StatusHost>();
        dHost = defender.GetComponent<StatusHost>();
        phase = CombatPhase.Start;
    }

    void Update() {
        switch (phase) {
            case CombatPhase.Start:
                ExecutePreBattlePhase(attacker, defender);
                // Probably also do some sort of animations regarding pieces' cards and their relevant effects
                break;

            // Wait for players to do stuff
            case CombatPhase.WaitForInput:
                if (InputManager.Controls.Player.BattleAdvance.WasPressedThisFrame()) {
                    phase = CombatPhase.RollingDice;
                }
                break;

            case CombatPhase.End:
                break;

            case CombatPhase.RollingDice:
                int[] atkDice = attacker.RollTheDice();
                int[] dfdDice = defender.RollTheDice();
                phase = CombatPhase.NoInputAllowed;
                StartCoroutine(ResolveCombatSequence(attacker, defender, atkDice, dfdDice, isSneakAttack));
                break;

            case CombatPhase.NoInputAllowed:

                break;
            
            case CombatPhase.Inactive:
                break;
        }
    }

    private void ExecutePreBattlePhase(Piece attacker, Piece defender) {
        Debug.Log("Pre-Battle Phase: Checking initialization cards...");
        resultText.text = "Press Space to roll!";
        UpdateUI(attacker, defender);
        phase = CombatPhase.WaitForInput;
    }

    private float EvaluateMidBattleEffects(Piece piece, float baseDamage) {
        if (piece == attacker) {
            thisHost = aHost;
            otherHost = dHost;
        }
        else {
            thisHost = dHost;
            otherHost = aHost;
        }

        // Look at the piece who is rolling right now
        float modifiedDamage = baseDamage;
        foreach (StatusEffect effect in thisHost.getEffects()) {
            if (effect.id == CardId.N_CONFLP) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    modifiedDamage *= 2.0f;
                }
                else {
                    modifiedDamage *= .25f;
                }
            }
        }

        // Now look at the piece getting attacked and see if they will modify damage
        foreach (StatusEffect effect in otherHost.getEffects()) {

        }
        return Mathf.CeilToInt(modifiedDamage);
    }

    public IEnumerator ResolveCombatSequence(Piece attacker, Piece defender, int[] atkDice, int[] dfdDice, bool isSneakAttack) {
        RectTransform dfdHealthTxt = dfdHealthText.GetComponent<RectTransform>();
        TargetShaker dfdShaker = dfdHealthText.GetComponent<TargetShaker>();
        RectTransform atkHealthTxt = atkHealthText.GetComponent<RectTransform>();
        TargetShaker atkShaker = atkHealthText.GetComponent<TargetShaker>();

        foreach (float rolledValue in atkDice) {
            GameObject die = Instantiate(UIDie, new Vector2(100f, 100f), Quaternion.identity);
            die.transform.SetParent(atkDiceText.transform.parent, false);
            DiceUI ds = die.GetComponent<DiceUI>();
            int original = Mathf.CeilToInt(rolledValue);
            int final = original;
            
            //total = EvaluateMidBattleEffects(attacker, total);
            List<CardEffectData> effects = GetCardEffectsForDie(attacker);
            if (isSneakAttack) {
                effects.Add(new CardEffectData { cardName = "Sneak Attack", multiplier = 1.5f, displayColor = Color.green });
            }
            if (effects.Count > 0) {
                foreach (CardEffectData effect in effects) {
                    final = Mathf.CeilToInt(final * effect.multiplier);
                }
            }

            Debug.Log($"Attacker: Rolled dam was {original} but after multipliers is {final}");

            // Start the roll -> fly -> hit sequence and PAUSE the loop until it finishes
            yield return StartCoroutine(ds.AnimateDice(
                Mathf.CeilToInt(original),
                Mathf.CeilToInt(final),
                dfdHealthTxt,
                effects,
                () => {
                    if (dfdShaker != null) {
                        dfdShaker.StartCoroutine(dfdShaker.ShakeUI());
                    }
                }
            ));

            // 5. INCREMENTAL UPDATE: Happens immediately after the die hits and shakes
            defender.currentHealth -= final;
            UpdateUI(attacker, defender); // Instantly updates the health text on screen
        }

        foreach (int rolledValue in dfdDice) {
            GameObject die = Instantiate(UIDie, new Vector2(100f, 100f), Quaternion.identity);
            die.transform.SetParent(atkDiceText.transform.parent, false);
            DiceUI ds = die.GetComponent<DiceUI>();

            int original = Mathf.CeilToInt(rolledValue);
            int final = original;
            //if (isSneakAttack) { total *= 1.5f; }
            //total = EvaluateMidBattleEffects(attacker, total);
            List<CardEffectData> effects = GetCardEffectsForDie(defender);

            if (effects.Count > 0) {
                foreach (CardEffectData effect in effects) {
                    final = Mathf.CeilToInt(final * effect.multiplier);
                }
            }

            //float total = rolledValue;
            //total = EvaluateMidBattleEffects(defender, total);
            Debug.Log($"Attacker: Rolled dam was {original} but after multipliers is {final}");

            // Start the roll -> fly -> hit sequence and PAUSE the loop until it finishes
            yield return StartCoroutine(ds.AnimateDice(
                Mathf.CeilToInt(original),
                Mathf.CeilToInt(final),
                atkHealthTxt,
                effects,
                () => {
                    // This triggers the exact millisecond the die hits the target
                    if (atkShaker != null) {
                        atkShaker.StartCoroutine(atkShaker.ShakeUI());
                    }
                }
            ));

            attacker.currentHealth -= final;
            UpdateUI(attacker, defender);
        }
        
        bool battleDone = ExecutePostBattlePhase(attacker, defender);
        if (battleDone) {
            ShowResult(attacker, defender);
            GameplayManager.Instance.SetPhase(TurnPhase.BattleEnd);
            phase = CombatPhase.End;
        }
        else {
            phase = CombatPhase.WaitForInput;
        } 
    }

    private List<CardEffectData> GetCardEffectsForDie(Piece piece) {
        List<CardEffectData> results = new List<CardEffectData>();
        StatusHost host = piece.GetComponent<StatusHost>();
        if (host == null) return results;

        foreach (StatusEffect effect in host.getEffects()) {
            if (effect.id == CardId.N_CONFLP) {
                if (UnityEngine.Random.Range(0, 2) == 0) {
                    results.Add(new CardEffectData { cardName = "Coin Flip", multiplier = 2.0f, displayColor = Color.green });
                }
                else {
                    results.Add(new CardEffectData { cardName = "Coin Flip", multiplier = 0.25f, displayColor = Color.yellow });
                }
            }
        }
        return results;
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
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class PieceHoverHandler : MonoBehaviour {
    private Piece piece;
    private bool isHovered = false;
    Color origColor = Color.white;
    Vector3 origScale = Vector3.one;
    private Coroutine hoverCoroutine;
    bool endCoroutine = false;
    public float popupDelay = .25f;

    void Awake() {
        piece = GetComponent<Piece>();
        origColor = piece.GetComponent<SpriteRenderer>().material.color;
        origScale = piece.transform.localScale;
    }

    void Update() {
        // Cast a ray from the mouse position into the 2D world
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        bool hitThisPiece = (hit.collider != null && hit.collider.gameObject == gameObject);

        if (endCoroutine) {
            endCoroutine = false;
            OnMouseEnterCustom();
        }

        if (hitThisPiece && !isHovered) {
            isHovered = true;
            ColorTest();
            hoverCoroutine = StartCoroutine(ActivateAfterDelay());
        }
        else if (!hitThisPiece && isHovered) {
            ColorTestEnd();
            if (hoverCoroutine != null) {
                StopCoroutine(hoverCoroutine);
                hoverCoroutine = null;
            }
            isHovered = false;
            OnMouseExitCustom();
        }
    }
    IEnumerator ActivateAfterDelay() {
        yield return new WaitForSeconds(popupDelay);
        endCoroutine = true;
        if (!isHovered) {
            hoverCoroutine = null;
            yield break;
        }
        hoverCoroutine = null;
    }

    void OnMouseEnterCustom() {
        Debug.Log($"Hovered over: {name}");
        if (TooltipManager.Instance != null) {
            TooltipManager.Instance.ShowTooltip(buildPieceContent(piece), Mouse.current.position.value);
        }
    }

    void OnMouseExitCustom() {
        Debug.Log($"Left: {name}");
        if (TooltipManager.Instance != null) {
            TooltipManager.Instance.HideTooltip();
        }
        endCoroutine = false;
    }

    void ColorTest() {
        piece.GetComponent<SpriteRenderer>().material.color = Color.red;
        piece.transform.localScale = new Vector3(origScale.x*1.5f, origScale.y*1.5f, 1.0f);
    }

    void ColorTestEnd() {
        piece.GetComponent<SpriteRenderer>().material.color = origColor;
        piece.transform.localScale = origScale;
    }


    string buildPieceContent(Piece piece) {
        string s = "";
        s += piece.data.name + "\n\n";
        s += "Health: " + piece.currentHealth + "/" + piece.data.initMaxHealth;
        s = BuildDiceRange(piece, s);
        s = BuildEffectList(piece, s);
        return s;
    }

    string BuildEffectList(Piece piece, string str) {
        StatusHost host = piece.GetComponent<StatusHost>();
        if (host == null || host.getEffects() == null || host.getEffects().Count == 0) {
            return str;
        }
        else {
            str += "\n\nEffects:";
            foreach (StatusEffect effect in host.getEffects()) {
                str += "\n" + effect.effectName;
            }
            return str;
        }
    }

    string BuildDiceRange(Piece piece, string str) {
        int low = 0;
        int high = 0;
        foreach (int i in piece.data.dice) {
            low++;
            high += i;
        }
        return str + $"\n\nDice per roll:{low}-{high}";
    }
}
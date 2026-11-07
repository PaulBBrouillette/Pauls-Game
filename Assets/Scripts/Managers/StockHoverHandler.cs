using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StockHoverHandler : MonoBehaviour {
    public UIRaycastHelper raycastHelper;
    PieceDisplay pieceDisplay;
    CardDisplay cardDisplay;
    public bool isHovered = false;
    Color origColor = Color.white;
    Vector3 origScale = Vector3.one;
    private Coroutine hoverCoroutine;
    bool endCoroutine = false;
    public float popupDelay = .25f;
    public string displayType;

    private void Awake() {
        try {
            cardDisplay = GetComponent<CardDisplay>();
            displayType = "Card";
            origColor = cardDisplay.GetComponent<Color>();
            origScale = cardDisplay.transform.localScale;
        }
        catch {
            Debug.Log("Not a card");
        }

        try {
            pieceDisplay = GetComponent<PieceDisplay>();
            displayType = "Piece";
            origColor = pieceDisplay.GetComponent<Color>();
            origScale = pieceDisplay.transform.localScale;
        }
        catch {
            Debug.Log("Not a card");
        }
    }

    private void Start() {
        raycastHelper = GetComponentInParent<UIRaycastHelper>();
    }

    void Update() {
        if (endCoroutine) {
            endCoroutine = false;
            OnMouseEnterCustom();
        }

        bool hit = false;
        if (raycastHelper.hoverCardDisplay && cardDisplay == raycastHelper.cDisplay) {
            hit = true;
        }
        else if (raycastHelper.hoverPieceDisplay && pieceDisplay == raycastHelper.pDisplay) {
            hit = true;
        }

        if (hit && !isHovered) {
            isHovered = true;
            StartHoverEffect();
            hoverCoroutine = StartCoroutine(ActivateAfterDelay());
        }
        else if (!hit && isHovered) {
            EndHoverEffect();
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
        if (TooltipManager.Instance != null) {
            if (cardDisplay != null) {
               TooltipManager.Instance.ShowTooltip(BuildTooltipText(cardDisplay), Mouse.current.position.value);
            }
            else if (pieceDisplay != null) {
                TooltipManager.Instance.ShowTooltip(BuildTooltipText(pieceDisplay), Mouse.current.position.value);
            }
        }
    }

    void OnMouseExitCustom() {
        if (TooltipManager.Instance != null) {
            TooltipManager.Instance.HideTooltip();
        }
        endCoroutine = false;
    }

    string BuildTooltipText(PieceDisplay piece) {
        string s = "";
        s += piece.data.name + "\n\n";
        s += "Health: " + piece.data.initMaxHealth;
        //s = BuildDiceRange(piece, s);
        //s = BuildEffectList(piece, s);
        return s;
    }

    string BuildTooltipText(CardDisplay card) {
        string s = "";
        s += card.cardData.description;
        return s;
    }

    void StartHoverEffect() {
        if (cardDisplay != null) {
            cardDisplay.transform.localScale = origScale * 1.2f;
        }
        else if (pieceDisplay != null) {
            pieceDisplay.transform.localScale = origScale * 1.2f;
        }
    }

    void EndHoverEffect() {
        if (cardDisplay != null) {
            cardDisplay.transform.localScale = origScale;
        }
        else if (pieceDisplay != null) {
            pieceDisplay.transform.localScale = origScale;
        }
    }
}

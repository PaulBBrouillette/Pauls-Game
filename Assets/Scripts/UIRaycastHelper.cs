using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIRaycastHelper : MonoBehaviour {
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    public bool hoverPieceDisplay;
    public bool hoverCardDisplay;
    public PieceDisplay pDisplay;
    public CardDisplay cDisplay;

    void Update() {
        PointerEventData pointerData = new PointerEventData(eventSystem);
        pointerData.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        bool isClicked = InputManager.Controls.Player.Select.triggered;

        foreach (RaycastResult result in results) {
            if (result.gameObject.CompareTag("PieceDisplay")) {
                PieceDisplay p = result.gameObject.GetComponent<PieceDisplay>();
                pDisplay = p;
                if (p != null) {
                    if (isClicked) {
                        GameplayManager.Instance.ChoosePieceToPlace(p.data, p.id);
                    }
                    else {
                        hoverPieceDisplay = true;
                    }
                }
                Debug.Log("Hovering/Hit UI Element: " + result.gameObject.name);
                break;
            }
            else if (result.gameObject.CompareTag("CardDisplay")) {
                CardDisplay c = result.gameObject.GetComponent<CardDisplay>();
                cDisplay = c;
                if (c != null) {
                    if (isClicked) {
                        GameplayManager.Instance.ChooseCardToPlace(c.cardData, c.id);
                    }
                    else {
                        hoverCardDisplay = true;
                    }
                }
                Debug.Log("Hovering/Hit UI Element: " + result.gameObject.name);
                break;
            }
            else {
                pDisplay = null;
                cDisplay = null;
                hoverPieceDisplay = false;
                hoverCardDisplay = false;
            }
        }
    }
}
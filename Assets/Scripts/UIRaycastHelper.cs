using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIRaycastHelper : MonoBehaviour {
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    void Update() {
        if (InputManager.Controls.Player.Select.triggered) {
            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = Mouse.current.position.ReadValue();

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results) {
                if (result.gameObject.CompareTag("PieceDisplay")) {
                    PieceDisplay p = result.gameObject.GetComponent<PieceDisplay>();
                    if (p != null) {
                        GameplayManager.Instance.ChoosePieceToPlace(p.data, p.id);
                    }
                    Debug.Log("Hit UI Element: " + result.gameObject.name);
                }
                else if (result.gameObject.CompareTag("CardDisplay")) {
                    CardDisplay c = result.gameObject.GetComponent<CardDisplay>();
                    if (c != null) {
                        GameplayManager.Instance.ChooseCardToPlace(c.cardData, c.id);
                    }
                    Debug.Log("Hit UI Element: " + result.gameObject.name);
                }
            }
        }
    }
}
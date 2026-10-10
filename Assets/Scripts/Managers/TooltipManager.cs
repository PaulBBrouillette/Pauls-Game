using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TooltipManager : MonoBehaviour {
    public static TooltipManager Instance;
    private Coroutine fadeCoroutine;

    [Header("UI References")]
    [SerializeField] private Image panelImage;
    [SerializeField] private GameObject panelObject;
    [SerializeField] private TextMeshProUGUI panelText;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float targetAlpha = .55f;
    [SerializeField] private float fadeTime = .5f;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(25f, 25f); // Offset away from the piece

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        HideTooltip();
    }

    public void ShowTooltip(string content, Vector2 mousePosition) {
        panelObject.SetActive(true);
        panelText.text = content;

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        PositionPanel(mousePosition);

        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeInTooltip(fadeTime));
    }

    public void HideTooltip() {
        if (fadeCoroutine != null) {
            StopCoroutine(fadeCoroutine);
        }
        ResetAlpha(0f);
        panelObject.SetActive(false);
    }

    private IEnumerator FadeInTooltip(float duration) {
        float counter = 0f;
        ResetAlpha(0f);

        Color startImageColor = panelImage != null ? panelImage.color : Color.white;
        Color startTextColor = panelText != null ? panelText.color : Color.white;

        while (counter < duration) {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, targetAlpha, counter / duration);

            if (panelImage != null) {
                panelImage.color = new Color(startImageColor.r, startImageColor.g, startImageColor.b, alpha);
            }
            if (panelText != null) {
                panelText.color = new Color(startTextColor.r, startTextColor.g, startTextColor.b, alpha);
            }

            yield return null;
        }
        ResetAlpha(targetAlpha);
    }

    private void ResetAlpha(float targetAlpha) {
        if (panelImage != null) {
            Color c = panelImage.color;
            panelImage.color = new Color(c.r, c.g, c.b, targetAlpha);
        }
        if (panelText != null) {
            Color c = panelText.color;
            panelText.color = new Color(c.r, c.g, c.b, targetAlpha);
        }
    }

    /// <summary>
    /// Fit the panel so that it's on screen, and shift it if it will appear off screen
    /// </summary>
    private void PositionPanel(Vector2 mousePosition) {
        Vector2 targetPosition = mousePosition + offset;

        float minLeft = 0f;
        float maxRight = Screen.width - panelRect.rect.width;

        if (targetPosition.x < minLeft) {
            targetPosition.x = mousePosition.x + offset.x;
        }
        else if (targetPosition.x > maxRight) {
            targetPosition.x = mousePosition.x - panelRect.rect.width - offset.x;
        }

        float minBottom = panelRect.rect.height;
        float maxTop = Screen.height;

        if (targetPosition.y < minBottom) {
            targetPosition.y = mousePosition.y + panelRect.rect.height - offset.y;
        }
        else if (targetPosition.y > maxTop) {
            targetPosition.y = mousePosition.y - offset.y;
        }
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0, Screen.width - panelRect.rect.width);
        targetPosition.y = Mathf.Clamp(targetPosition.y, panelRect.rect.height, Screen.height);

        panelRect.position = targetPosition;
    }
}
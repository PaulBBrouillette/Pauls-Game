using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingTextUI : MonoBehaviour {
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private float floatSpeed = 60f;
    [SerializeField] private float duration = 1.0f; 

    public IEnumerator Float(string message, Color color) {
        textComponent = GetComponent<TMP_Text>();
        textComponent.text = message;
        textComponent.color = color;

        RectTransform rect = GetComponent<RectTransform>();
        Color originalColor = color;
        float elapsed = 0f;

        while (elapsed < duration) {
            rect.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
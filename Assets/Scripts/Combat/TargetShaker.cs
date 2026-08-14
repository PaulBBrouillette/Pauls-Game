using System.Collections;
using UnityEngine;
public class TargetShaker : MonoBehaviour {
    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        originalAnchoredPos = rectTransform.anchoredPosition;
    }

    public IEnumerator ShakeUI(float duration = 0.2f, float magnitude = 10f) {
        float elapsed = 0f;

        while (elapsed < duration) {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            rectTransform.anchoredPosition = originalAnchoredPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = originalAnchoredPos;
    }
}
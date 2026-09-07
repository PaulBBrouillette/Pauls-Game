using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour {
    [SerializeField] private Sprite[] diceFaces;
    [SerializeField] private float rollDuration = 0.3f;
    [SerializeField] private float faceChangeSpeed = 0.1f;
    [SerializeField] private float aftRollStop = 0.5f;
    [SerializeField] private float effectStop = 0.75f;
    [SerializeField] private float flySpeed = 1500f; // Speed at which it flies to the target
    [SerializeField] private GameObject floatingText;
    int max;
    int min = 0;
    private Image image;
    private RectTransform rectTransform;

    void Awake() {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        max = diceFaces.Length - 1;
    }

    public IEnumerator AnimateDice(float original, float final, RectTransform targetRect, List<CardEffectData> effects, Action onArrival) {
        int index = Mathf.Clamp((int)original, min, max);
        original = Mathf.Clamp(original, min, max);
        final = Mathf.Clamp(final, min, max);
        float elapsed = 0f;
        while (elapsed < rollDuration) {
            int randomFace = UnityEngine.Random.Range(0, diceFaces.Length);
            image.sprite = diceFaces[randomFace];
            elapsed += faceChangeSpeed;
            yield return new WaitForSeconds(faceChangeSpeed);
        }

        image.sprite = diceFaces[index];
        yield return new WaitForSeconds(aftRollStop); // Pause to read the number

        if (effects.Count > 0) {
            int listIndex = 0;
            foreach (CardEffectData effect in effects) {
                GameObject textObj = Instantiate(floatingText, new Vector2(100f, 100f), Quaternion.identity);
                textObj.transform.SetParent(transform, false);
                FloatingTextUI ft = textObj.GetComponent<FloatingTextUI>();

                yield return StartCoroutine(ft.Float($"{effect.cardName}: x{effect.multiplier}", effect.displayColor));

                index = Mathf.CeilToInt(original *= effect.multiplier);
                if (effects.Count - 1 == listIndex) {
                    image.sprite = diceFaces[(int)final];
                    yield return new WaitForSeconds(effectStop);
                }
                else {
                    image.sprite = diceFaces[index];
                }
                listIndex++;
            }
        }
        
        while (targetRect != null && Vector3.Distance(rectTransform.position, targetRect.position) > 5f) {
            rectTransform.position = Vector3.MoveTowards(rectTransform.position, targetRect.position, flySpeed * Time.deltaTime);
            rectTransform.Rotate(new Vector3(0, 0, 360f * Time.deltaTime));
            yield return null;
        }
        // Play sound
        SoundManager.Instance.PlaySoundByType(SoundType.BATTLE_HIT);
        onArrival?.Invoke();
        Destroy(gameObject);
    }

    public IEnumerator ShakeUI(float duration = 0.2f, float magnitude = 10f) {
        RectTransform targetRect = GetComponent<RectTransform>();
        Vector2 originalPos = targetRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration) {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            targetRect.anchoredPosition = originalPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        targetRect.anchoredPosition = originalPos;
    }
}
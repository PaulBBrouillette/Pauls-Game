using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DiceUI : MonoBehaviour {
    [SerializeField] private Sprite[] diceFaces;
    [SerializeField] private float rollDuration = 0.8f;
    [SerializeField] private float faceChangeSpeed = 0.04f;
    private Image image;

    public IEnumerator Roll(int finalValue) {
        Debug.Log($"Dice rolling for {finalValue}...");
        image = GetComponent<Image>();
        float elapsed = 0f;

        while (elapsed < rollDuration) {
            int randomFace = Random.Range(0, diceFaces.Length);
            if (image.sprite == diceFaces[randomFace]) randomFace = Random.Range(0, diceFaces.Length);
            image.sprite = diceFaces[randomFace];

            elapsed += faceChangeSpeed;

            yield return new WaitForSeconds(faceChangeSpeed);
        }
        Debug.Log("Rolling stop");
        // Make absolutely sure the final face is correct.
        image.sprite = diceFaces[finalValue - 1];
    }
}
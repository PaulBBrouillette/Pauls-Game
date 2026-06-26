using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour {
    public CardData cardData;
    public int id;
    public static int rollingInt = 0;
    public int turnsRemaining;

    public void Initialize(CardData data) {
        cardData = data;
        Image img = GetComponent<Image>();
        img.sprite = data.shopIcon;
        id = rollingInt;
        rollingInt++;
        turnsRemaining = data.turnDuration;
    }
}
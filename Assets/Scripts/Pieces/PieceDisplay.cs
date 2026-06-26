using UnityEngine;
using UnityEngine.UI;

public class PieceDisplay : MonoBehaviour {
    public PieceData data;
    public int id;
    public static int rollingInt = 0;

    public void Initialize(PieceData data) {
        this.data = data;
        Image img = GetComponent<Image>();
        img.sprite = data.shopIcon;
        id = rollingInt;
        rollingInt++;
    }
}
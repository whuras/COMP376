using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Text ButtonText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ButtonText.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ButtonText.color = Color.white;
    }
}
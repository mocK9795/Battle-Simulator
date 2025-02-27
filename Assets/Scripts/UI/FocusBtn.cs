using UnityEngine;
using UnityEngine.UI;

public class FocusBtn : MonoBehaviour
{
    public string focusName;
    public FocusDisplay display;
    public Image image;
    public Slider slider;

    public void OnClick()
    {
        display.OnFocusClick(focusName);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
public class LabledText : MonoBehaviour
{
    public Sprite image { set { imageRenderer.sprite = value; } get { return imageRenderer.sprite; } }
	public Image imageRenderer;
    public TMP_Text text;

	private void Start()
	{
		imageRenderer = GetComponentInChildren<Image>();
		text = GetComponentInChildren<TMP_Text>();
	}
}

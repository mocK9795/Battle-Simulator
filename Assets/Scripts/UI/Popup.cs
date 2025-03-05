using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class Popup : MonoBehaviour
{
    public enum Style {Report, Popup}
    public Style style {set { style = value; OnStyleChange(); } get { return style; } }
    public Color backgroundColor;
    public Image background;
	public Vector2 padding;

	int constraintCount { get { return grid.constraintCount; } set { grid.constraintCount = value; } }
	Vector2 size 
	{
	get 
		{ return rt.sizeDelta - padding; } 
	set 
		{ rt.sizeDelta = value; }
	}

    List<GameObject> UiElements = new();
	GridLayoutGroup grid = null;
	RectTransform rt;
	RectTransform gridRt;

	public void Activate()
	{
		UiElements = new();
		grid = GetComponentInChildren<GridLayoutGroup>();
		rt = GetComponent<RectTransform>();
		gridRt = GetComponent<RectTransform>();
	}

	public void Clear()
	{
		foreach (GameObject uiElement in UiElements)
		{
			Destroy(uiElement);
		}

		UiElements.Clear();
	}

	public void SetContraint(GridLayoutGroup.Constraint constraint, int count)
	{
		grid.constraint = constraint;
		constraintCount = count;
	}

	public void Element(GameObject element)
	{
		element.transform.SetParent(grid.transform, false);
		UiElements.Add(element);
	}

	public void Message(string message) {Message(message, Color.white); }

	public void Message(string message, Color color)
	{
		GameObject element = new GameObject("Message");
		element.transform.SetParent(grid.transform, false);
		var messageElement = element.AddComponent<TextMeshProUGUI>();
		messageElement.text = message;
		messageElement.enableAutoSizing = true;
		messageElement.fontSizeMax = 999;
		messageElement.color = color;
		UiElements.Add(element);
	}

	public void Image(Sprite sprite)
	{
		Image(sprite, Color.white);
	}

	public void Image(Sprite sprite, Color color)
	{
		GameObject element = new GameObject("Image");
		element.transform.SetParent(grid.transform, false);
		var image = element.AddComponent<Image>();
		image.sprite = sprite;
		image.color = color;
		UiElements.Add(element);
	}

	public void Button(string buttonText, System.Action buttonCallback)
	{
		print("Feature not implemented completely");
		GameObject element = new GameObject("Button");
		element.transform.SetParent(grid.transform, false);
		var button = element.AddComponent<Button>();
		button.onClick.AddListener(() => buttonCallback());
		UiElements.Add(element);
	}

	public void FinalizeLayout()
	{
		if (constraintCount == 1)
		{
			if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
				grid.cellSize = new Vector2(size.x, size.y / UiElements.Count);
			else if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
				grid.cellSize = new Vector2(size.x / UiElements.Count, size.y);
		}
	}

	public void OnClose()
	{
		Destroy(gameObject);
	}

	void OnStyleChange() { }

	private void OnValidate()
	{
		if (background != null)
		background.color = backgroundColor;
	}
}

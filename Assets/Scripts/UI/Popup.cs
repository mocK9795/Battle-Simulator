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

    List<GameObject> UiElements = new();
	GridLayoutGroup grid = null;

	public void Activate()
	{
		UiElements = new();
		grid = GetComponentInChildren<GridLayoutGroup>();
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
		grid.constraintCount = count;
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
		element.transform.SetParent(element.transform, false);
		var messageElement = element.AddComponent<TMP_Text>();
		messageElement.text = message;
		messageElement.enableAutoSizing = true;
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
		element.transform.SetParent(element.transform, false);
		var image = element.AddComponent<Image>();
		image.sprite = sprite;
		image.color = color;
		UiElements.Add(element);
	}

	public void Button(string buttonText, System.Action buttonCallback)
	{
		throw (new NotImplementedException());
		GameObject element = new GameObject("Button");
		element.transform.SetParent(element.transform, false);
		var button = element.AddComponent<Button>();
	}

	void OnStyleChange() { }
}

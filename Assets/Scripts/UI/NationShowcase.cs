using TMPro;
using UnityEngine;

public class NationShowcase : MonoBehaviour
{
	public float updateSpeed;
	public int valuePrecision;

	public Vector2 offScreenPosition;
	public Vector2 onScreenPosition;
	public float animationTime;

	public FocusDisplay focusMenu;
	public VisualEffects effects;
    public GameObject menuContainer;
	public RectTransform menuTransform;
    public TMP_Text nationName;

    public LabledText wealthDisplay;
	float shownWealth;

	Nation nation = null;

	private void Start()
	{
		effects = FindFirstObjectByType<VisualEffects>();
	}

	public void ShowcaseNation(Nation nation)
    {
        this.nation = nation;
		menuTransform.anchoredPosition = offScreenPosition;
        menuContainer.SetActive(true);
		nationName.text = nation.nation;
    }

	private void Update()
	{
        if (!menuContainer.activeSelf) return;
        if (nation == null) return;

		menuTransform.anchoredPosition = Vector2.Lerp(menuTransform.anchoredPosition, onScreenPosition, Mathf.Clamp01(Time.deltaTime * animationTime));

		shownWealth = GlobalData.SoftLerp(shownWealth, nation.wealth, updateSpeed);
        wealthDisplay.text.text = GlobalData.SetPrecision(shownWealth, valuePrecision).ToString() + " " + effects.symbol;
	}

	public void OnFocusTreeButtonClick()
	{
		focusMenu.PlaceFocusUI(nation.focusTree);
	}
}

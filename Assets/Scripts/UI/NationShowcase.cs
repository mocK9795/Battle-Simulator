using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
	public Image nationNameBackground;
	public float backgroundTransparency;

    public LabledText wealthDisplay;
	float shownWealth;

	public LabledText politicalPowerDisplay;
	float shownPoliticalPower;

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
		Color color = GlobalData.Copy(nation.nationColor);
		color.a = backgroundTransparency;
		nationNameBackground.color = nation.nationColor;
		nationNameBackground.color = color;
    }

	private void Update()
	{
        if (!menuContainer.activeSelf) return;
        if (nation == null) return;

		menuTransform.anchoredPosition = Vector2.Lerp(menuTransform.anchoredPosition, onScreenPosition, Mathf.Clamp01(Time.deltaTime * animationTime));

		shownWealth = GlobalData.SoftLerp(shownWealth, nation.wealth, updateSpeed);
        wealthDisplay.text.text = GlobalData.SetPrecision(shownWealth, valuePrecision).ToString() + " " + effects.symbol;
	
		shownPoliticalPower = GlobalData.SoftLerp(shownPoliticalPower, nation.politicalPower, updateSpeed);
		politicalPowerDisplay.text.text = GlobalData.SetPrecision(shownPoliticalPower, valuePrecision).ToString();
	}

	public void OnFocusTreeButtonClick()
	{
		focusMenu.ActivateFocusUI(nation);
	}
}

using UnityEngine;
using TMPro;

public class ContructionMenu : MonoBehaviour
{
    public GameObject menu;
    public RectTransform menuTransform;
    public Site site;

    public Vector2 offScreenPosition;
    public Vector2 onScreenPosition;
    public float animationTime;

    public TMP_Text valueText;
    public TMP_Text timeText;
    public TMP_Text costText;
    EconomyManager economyManager;

	private void Start()
	{
		economyManager = FindFirstObjectByType<EconomyManager>();
	}

	private void Update()
	{
		menuTransform.anchoredPosition = Vector2.Lerp(menuTransform.anchoredPosition, onScreenPosition, Mathf.Clamp01(Time.deltaTime * animationTime));

        site = EconomyManager.NormalizeSiteData(site);
		valueText.text = site.efficency.ToString();
		timeText.text = site.capacity.ToString();
		costText.text = economyManager.SiteCost(site).ToString() + " " + economyManager.symbol;
	}

    public void Activate()
    {
        menu.SetActive(true);
        menuTransform.anchoredPosition = offScreenPosition;
    }

	public void OnValueIncrease()
    {
        site.efficency += 0.1f;
    }

    public void OnTimeIncrease()
    {
        site.capacity += 3;
    }

    public void OnValueDecrease()
    {
        site.efficency -= 0.1f;
    }

    public void OnTimeDecrease()
    {
        site.capacity -= 3;
    }
}

using TMPro;
using UnityEngine;

[RequireComponent (typeof(Announcement))]
public class VisualEffects : MonoBehaviour
{
	public string battleName;
	public string battleStats;
	[Header("Drawn Shapes")]
	[Tooltip("Requires 2 LineRenderers In Child Object")]
	public float arrowWidth;
	public float selectionWidth;
	public Color arrowColor = Color.white;
	public Color selectionColor = Color.white;
	[Header("Statistics Bar")]
	public TMP_Text wealthCounter;
	public float wealthUpdateSpeed;
	public float wealthShowcasePrecision;
	float wealth = 0;
	float shownWealth = 0;
	public string symbol = "";
	Announcement announcement = null;
	LineRenderer arrowRenderer = null;
	LineRenderer selectionRenderer = null;

	private void Start()
	{
		var allLineRenderers= GetComponentsInChildren<LineRenderer>();
		arrowRenderer = allLineRenderers[0];
		selectionRenderer = allLineRenderers[1];
		SetArrowRendererSettings();
		SetSelectionRendererSettings();

		announcement = GetComponent<Announcement>();
		announcement.announcementComplete = OnAnnouncementComplete;
		announcement.Announce(battleName);
	}

	private void Update()
	{
		if (shownWealth < wealth)
		{
			shownWealth += 1 * wealthUpdateSpeed * Time.deltaTime;
			shownWealth = Mathf.Min(shownWealth, wealth);
			UpdateWealthCounter();
		}
		if (shownWealth > wealth + 1)
		{
			shownWealth -= 1 * wealthUpdateSpeed * Time.deltaTime;
			shownWealth = Mathf.Max(shownWealth, wealth);
			UpdateWealthCounter();
		}
	}

	void UpdateWealthCounter() {wealthCounter.text = (Mathf.RoundToInt(shownWealth * Mathf.Pow(10, wealthShowcasePrecision)) / Mathf.Pow(10, wealthShowcasePrecision)).ToString() + symbol;}

	public void DrawArrow(Vector2 start, Vector2 end)
	{
		arrowRenderer.positionCount = 2;
		arrowRenderer.SetPosition(0, GlobalData.vector3(start));
		arrowRenderer.SetPosition(1, GlobalData.vector3(end));
	}

	public void ClearArrow()
	{
		arrowRenderer.positionCount = 0;
	}

	private void OnValidate()
	{
		if (arrowRenderer == null) return;
		SetArrowRendererSettings();
		SetSelectionRendererSettings();
	}

	void SetSelectionRendererSettings()
	{
		selectionRenderer.startColor = selectionColor;
		selectionRenderer.endColor = selectionColor;
		selectionRenderer.startWidth = selectionWidth;
		selectionRenderer.endWidth = selectionWidth;
		selectionRenderer.loop = true;
	}

	void SetArrowRendererSettings()
	{
		arrowRenderer.startWidth = arrowWidth;
		arrowRenderer.endWidth = arrowWidth;
		arrowRenderer.startColor = arrowColor;
		arrowRenderer.endColor = arrowColor;
	}

	public void OnAnnouncementComplete()
	{
		announcement.Announce(battleStats);
		announcement.announcementComplete = null;
	}

	public void DrawBox(Vector2 center, Vector2 size)
	{
		Vector2 halfSize = size / 2;
		Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
		Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
		Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);
		Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);

		selectionRenderer.positionCount = 4;
		selectionRenderer.SetPosition(0, topLeft);
		selectionRenderer.SetPosition(1, topRight);
		selectionRenderer.SetPosition(2, bottomRight);
		selectionRenderer.SetPosition(3, bottomLeft);
	}

	public void ClearBox()
	{
		selectionRenderer.positionCount = 0;
	}

	public void SetWealthCount(float wealth) 
	{
		this.wealth = wealth;
	}
}

using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
[RequireComponent (typeof(Announcement))]
public class VisualEffects : MonoBehaviour
{
	public string battleName;
	public string battleStats;
	public float width;
	public Color arrowColor = Color.white;
    LineRenderer lineRenderer = null;
	Announcement announcement = null;
	[Space(1)]
	public string errorName;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		SetLineRendererSettings();

		announcement = GetComponent<Announcement>();
		announcement.announcementComplete = OnAnnouncementComplete;
		announcement.Announce(battleName);
	}

	public void DrawArrow(Vector2 start, Vector2 end)
	{
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, GlobalData.vector3(start));
		lineRenderer.SetPosition(1, GlobalData.vector3(end));
	}

	public void ClearArrow()
	{
		lineRenderer.positionCount = 0;
	}

	private void OnValidate()
	{
		if (lineRenderer == null) return;
		SetLineRendererSettings();
	}

	void SetLineRendererSettings()
	{
		lineRenderer.startWidth = width;
		lineRenderer.endWidth = width;
		lineRenderer.startColor = arrowColor;
		lineRenderer.endColor = arrowColor;
	}

	public void OnAnnouncementComplete()
	{
		announcement.Announce(battleStats);
		announcement.announcementComplete = null;
	}

	[ContextMenu("Show Error")]
	public void ShowError()
	{
		throw new System.Exception(errorName);
	}
}

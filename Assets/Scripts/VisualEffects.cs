using UnityEngine;

[RequireComponent (typeof(LineRenderer))]
[RequireComponent (typeof(Announcement))]
public class VisualEffects : MonoBehaviour
{
	public string battleName;
	public float width;
	public Color arrowColor = Color.white;
    LineRenderer lineRenderer = null;
	Announcement announcement = null;

	private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		SetLineRendererSettings();

		announcement = GetComponent<Announcement>();
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
}

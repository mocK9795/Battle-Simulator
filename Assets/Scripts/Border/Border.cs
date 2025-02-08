using UnityEngine;

public class Border : MonoBehaviour
{
	public Color color {
		get
		{
			LineRenderer lineRenderer = GetComponent<LineRenderer>();
			return Color.Lerp(lineRenderer.startColor, lineRenderer.endColor, 0.5f);
		}
		set
		{
			LineRenderer lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.startColor = value;
			lineRenderer.endColor = value;
		}
	}
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class ShowcaseItem : MonoBehaviour
{
    public new RectTransform transform;

	private void Update()
	{
		transform.rotation = Quaternion.Euler(0, 0, GlobalData.Angle(transform.position, GlobalData.mousePosition));
	}

}

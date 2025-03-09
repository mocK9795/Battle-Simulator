using UnityEngine;
using System.Collections.Generic;

public class MobileControls : MonoBehaviour
{
    Player player;
	public float zoomScale;
	public bool useMobileControls;
	public List<GameObject> mobileControls;
	private void Start()
	{
		player = GetComponent<Player>();
		useMobileControls = Application.isMobilePlatform || useMobileControls;
		foreach (var obj in mobileControls) { obj.SetActive(useMobileControls); }
	}
	public void ZoomIn()
    {
		player.OnZoom(-1 * zoomScale);
    }
	public void ZoomOut()
	{
		player.OnZoom(1 * zoomScale);
	}
}

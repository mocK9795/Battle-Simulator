using TMPro;
using UnityEngine;

public class Announcement : MonoBehaviour
{
    public TMP_Text message;
    public float showTime;
	public float fadeOut;
	public float fadeIn;
    public float shake;
	public Color messageColor;
	[HideInInspector] public bool isAnnouncing;
	[HideInInspector] public System.Action announcementComplete = null;

	float showTimer;

	private void Update()
	{
		if (showTimer < showTime)
		{
			showTimer += Time.deltaTime;
			if (showTimer < fadeIn) messageColor.a = (showTimer / fadeIn);
			else if (showTimer > fadeOut) messageColor.a = 1 - ((showTimer - showTime) / fadeOut);
			message.color = messageColor;
			isAnnouncing = true;
		}
		else if (isAnnouncing)
		{
			message.gameObject.SetActive(false);
			isAnnouncing = false;
			if (announcementComplete != null) announcementComplete();
		}
	}

	public void Announce(string announcement)
    {
		showTimer = 0;
		message.text = announcement;
		message.gameObject.SetActive(true);
    }

	[ContextMenu("Test Announce")]
	public void DefaultAnnounce()
	{
		Announce(message.text);
	}
}

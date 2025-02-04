using TMPro;
using UnityEngine;

public class Announcement : MonoBehaviour
{
    public TMP_Text message;
    public float showTime;
    public float shake;

	float showTimer;

	private void Update()
	{
        if (showTimer < showTime)
        {
			showTimer += Time.deltaTime;
		}
		else
		{
			message.gameObject.SetActive(false);
		}
	}

	public void Announce(string announcement)
    {
		showTimer = 0;
		message.text = announcement;
		message.gameObject.SetActive(true);
    }
}

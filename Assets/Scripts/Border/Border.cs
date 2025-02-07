using UnityEngine;

public class Border : MonoBehaviour
{
	public string nation;
    Announcement announcer;

	private void Start()
	{
		announcer = FindFirstObjectByType<Announcement>();
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		announcer.Announce(nation);
		print("Entering " + nation);
	}
}

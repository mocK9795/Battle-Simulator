using System.Collections.Generic;
using UnityEngine;

public class Nation : MonoBehaviour
{
    public string nation;
	public Color nationColor;
	public float health;
	List<Warrior> warriors = new List<Warrior>();
	Announcement announcer;

	private void Start()
	{
		announcer = FindFirstObjectByType<Announcement>();
		gameObject.name = nation;
		SetArmy();
		SetArmyColor();
	}

	private void Update()
	{
		health = GetArmyHealth();
	}

	[ContextMenu("Set Army")]
	public void SetArmy()
	{
		Warrior[] allWarriors = BattleManager.GetAllWarriors();
		foreach (Warrior warrior in allWarriors)
		{
			if (warrior.nation != nation) continue;
			warriors.Add(warrior);
        }
	}


	[ContextMenu("Set Army Color")]
	public void SetArmyColor()
	{
		foreach (Warrior warrior in warriors)
		{
			SpriteRenderer sprite = warrior.GetComponent<SpriteRenderer>();
			sprite.color = nationColor;
		}
	}

	[ContextMenu("Get Color")] 
	public Color GetColorFromBorder()
	{
		nationColor = GetComponentInChildren<Border>().color;
		return nationColor;
	}

	public float GetArmyHealth()
	{
		float totalHealth = 0;
		foreach (Warrior warrior in warriors)
		{
			totalHealth += warrior.health;
		}
		return totalHealth;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		announcer.Announce(nation);
		print("Entering " + nation);
	}
}

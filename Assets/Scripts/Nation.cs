using UnityEngine;
using UnityEngine.Rendering;

public class Nation : MonoBehaviour
{
    public string nation;
	public Color nationColor;
	public float health;

	private void Start()
	{
		SetArmyAsChild();
		SetChildColor();
	}

	private void Update()
	{
		health = GetArmyHealth();
	}

	[ContextMenu("Set Army As Child")]
	public void SetArmyAsChild()
	{
		Warrior[] warriors = BattleManager.GetAllWarriors();
		foreach (Warrior warrior in warriors)
		{
			if (warrior.nation != nation) continue;
			warrior.transform.parent = transform;
        }
	}


	[ContextMenu("Set Child Color")]
	public void SetChildColor()
	{
		foreach (Warrior warrior in GetComponentsInChildren<Warrior>())
		{
			SpriteRenderer sprite = warrior.GetComponent<SpriteRenderer>();
			sprite.color = nationColor;
		}
	}

	public float GetArmyHealth()
	{
		float totalHealth = 0;
		foreach (Warrior warrior in GetComponentsInChildren<Warrior>())
		{
			totalHealth += warrior.health;
		}
		return totalHealth;
	}
}

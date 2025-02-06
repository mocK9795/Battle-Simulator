using JetBrains.Annotations;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	public Vector2 warriorSpread;
	public Sprite warriorSprite;
	public NationData[] nations;
	public WarriorData[] warriors;

	[ContextMenu("Create Nations From Data")]
	public void CreateNations ()
	{
		foreach (NationData nationData in nations) {
			GameObject nationObject = new GameObject(nationData.name);
			Nation nation = nationObject.AddComponent<Nation>();
			nation.nation = nationData.name;

			Color nationColor = nationData.color; nationColor.a = 1;
			nation.nationColor = nationColor;
		}
	}

	[ContextMenu("Create Warriors From Data")]
	public void CreateArmies()
	{
		foreach (WarriorData warriorData in warriors)
		{
			for (int i = 0; i < warriorData.count; i++)
			{
				GameObject warriorObject = new GameObject(warriorData.nation);
				Warrior warrior = warriorObject.AddComponent<Warrior>();
				warrior.nation = warriorData.nation; 
				warrior.health = warriorData.health;
				warrior.damage = warriorData.damage;
				warrior.speed = warriorData.speed;

				SpriteRenderer renderer = warriorObject.AddComponent<SpriteRenderer>();
				renderer.sprite = warriorSprite;

				warriorObject.AddComponent<BoxCollider2D>();
			}	
		}
	}

	[ContextMenu("Sync Nations And Warriors")]
	public void SetWarriorNationData()
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			nation.SetArmyAsChild();
			nation.SetChildColor();
		}
	}

	[ContextMenu("Spread Warriors")]
	public void SpreadWarriors()
	{
		Warrior[] allWarriors = GetAllWarriors();
		foreach (Warrior warrior in allWarriors)
		{
			warrior.transform.position += new Vector3(
				Random.Range(-warriorSpread.x, warriorSpread.x),
				Random.Range(-warriorSpread.y, warriorSpread.y)
				);
		}
	}

	[ContextMenu("Delete Nations")]
	public void RemoveNations()
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			DestroyImmediate(nation.gameObject);
		}
	}

	[ContextMenu("Delete Warriors")]
	public void RemoveWarriors()
	{
		Warrior[] allWarriors = GetAllWarriors();
		foreach (Warrior warrior in allWarriors)
		{
			DestroyImmediate(warrior.gameObject);
		}
	}

	public static Nation[] GetAllNations()
	{
		return FindObjectsByType<Nation>(FindObjectsSortMode.None);
	}
	public static Warrior[] GetAllWarriors()
	{
		return FindObjectsByType<Warrior>(FindObjectsSortMode.None);
	}
}

[System.Serializable]
public struct NationData
{
	public string name;
	public Color color;
}

[System.Serializable]
public struct WarriorData
{
	public string nation;
	public float health;
	public float speed;
	public float damage;
	public int count;
}

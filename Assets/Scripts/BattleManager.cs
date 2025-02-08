using UnityEngine;

public class BattleManager : MonoBehaviour
{
	public enum GroupMode { Nation, Dump };
	public GroupMode groupMode;

	public Vector2 warriorSpread;
	public Sprite warriorSprite;
	public NationData[] nations;
	public WarriorData[] warriors;

	[ContextMenu("Create Nations From Data")]
	public void CreateNations ()
	{
		Border[] borders = FindObjectsByType<Border>(FindObjectsSortMode.None);

		for (int i = 0; i<nations.Length; i++) {
			NationData nationData = nations[i];

			GameObject nationObject;
			if (i >= borders.Length) nationObject = new GameObject(nationData.name);
			else { nationObject = borders[i].gameObject; borders[i].gameObject.name = nationData.name; }

			Nation nation = nationObject.AddComponent<Nation>();
			nation.nation = nationData.name;

			Color nationColor; 
			if (i < borders.Length) nationColor = borders[i].color;
            else nationColor = Color.white;
            nationColor.a = 1;
			
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
			nation.SetArmy();
			nation.SetArmyColor();
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

	[ContextMenu("Group Warriors")]
	public void GroupWarriors()
	{
		Warrior[] warriors = GetAllWarriors();
		Nation[] nations = GetAllNations();
		
		foreach (Warrior warrior in warriors)
		{
			if (groupMode == GroupMode.Dump)
			{
				warrior.transform.parent = transform;
				continue;
			}

			foreach (Nation nation in nations)
			{
				if (warrior.nation != nation.nation) continue;
				warrior.transform.parent = nation.transform;
			}
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

	public static Color GetNationColor()
	{
		return Color.white;
	}
}

[System.Serializable]
public struct NationData
{
	public string name;
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

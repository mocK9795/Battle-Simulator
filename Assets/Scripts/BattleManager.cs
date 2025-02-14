using UnityEngine;

public class BattleManager : MonoBehaviour
{
	public enum WarriorGroupMode { Nation, Dump };
	public enum NationGroupMode { Manager, Border, Map};
	[Header("Grouping")]
	public WarriorGroupMode warriorGroupMode;
	public NationGroupMode nationGroupMode;

	[Header("Misc Data")]
	public Vector2 warriorSpread;
	public Sprite warriorSprite;
	public Sprite capitalSprite;
	[Header("Capital Scaling")]
	public float capitalScale;
	public float capitalColliderScale;
	public float capitalControllScale;
	public bool autoScaleCapitals;

	[Header("Creation Data")]
	public NationData[] nations;
	public WarriorData[] warriors;
	public CapitalData capitalData;

	public MapBorderRenderer borderRenderer;
	public GameObject map;

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
            else nationColor = nationData.color;
            nationColor.a = 1;
			
			nation.nationColor = nationColor;
		}
	}

	[ContextMenu("Create Warriors From Data")]
	public void CreateArmies()
	{
		Nation[] allNations = GetAllNations();
		string[] nationNames = new string[allNations.Length];
		for (int i = 0; i<allNations.Length; i++) { nationNames[i] = allNations[i].nation; }

		foreach (WarriorData warriorData in warriors)
		{
			for (int i = 0; i < warriorData.count; i++)
			{
				foreach (string nationName in nationNames)
				{
					GameObject warriorObject = new GameObject(nationName + " " + i.ToString());
					Warrior warrior = warriorObject.AddComponent<Warrior>();
					warrior = SetWarriorData(warrior, warriorData, nationName, true);
				}
			}	
		}
	}

	public Warrior SetWarriorData(Warrior warrior, WarriorData data, string nation , bool createColliders = false)
	{
		warrior.nation = nation;
		warrior.health = data.health;
		warrior.damage = data.damage;
		warrior.speed = data.speed;

		SpriteRenderer renderer = warrior.gameObject.AddComponent<SpriteRenderer>();
		renderer.sprite = warriorSprite;

		if (createColliders ) { warrior.gameObject.AddComponent<BoxCollider2D>(); }

		return warrior;
	}

	[ContextMenu("Create Capitals From Data")]
	public void CreateCapitals() 
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			GameObject capitalObject = new GameObject(nation.nation + " Capital");
			Capital capital = capitalObject.AddComponent<Capital>();
			capital = (Capital) SetWarriorData(capital, capitalData.data, nation.nation);
			var collider = capitalObject.AddComponent<CircleCollider2D>();
			var sizeControll = capitalObject.AddComponent<CircleCollider2D>();
			sizeControll.isTrigger = true;
		}

		SetWarriorNationData();
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

	[ContextMenu("Delete Capitals")]
	public void RemoveCapitals()
	{
		var capitals = GetAllCapitals();
		foreach (var cap in capitals) {DestroyImmediate(cap.gameObject);}
	}

	[ContextMenu("Group Warriors")]
	public void GroupWarriors()
	{
		Warrior[] warriors = GetAllWarriors();
		Nation[] nations = GetAllNations();
		
		foreach (Warrior warrior in warriors)
		{
			if (warriorGroupMode == WarriorGroupMode.Dump)
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

	[ContextMenu("Group Nations")]
	public void GroupNations()
	{
		var allNations = GetAllNations();
		foreach (var nation in allNations)
		{
			if (nationGroupMode == NationGroupMode.Border)
			{
				nation.transform.parent = borderRenderer.transform;
			}
			if (nationGroupMode == NationGroupMode.Map)
			{
				nation.transform.parent = map.transform;
			}
			if (nationGroupMode == NationGroupMode.Manager)
			{
				nation.transform.parent = transform;
			}
		}
	}

	[ContextMenu("Set Warrior Sprite")]
	public void SetWarriorSprite()
	{
		var allWarriors = GetAllWarriors();
		foreach (var warrior in allWarriors) { warrior.GetComponent<SpriteRenderer>().sprite = warriorSprite; }
		SetCapitalSprite();
	}

	[ContextMenu("Set Capital Sprite")]
	public void SetCapitalSprite()
	{
		var capitals = GetAllCapitals();
		foreach (var capital in capitals) {capital.GetComponent<SpriteRenderer>().sprite = capitalSprite;}
	}

	public void ScaleCapital()
	{
		var capitals = GetAllCapitals();
		foreach (var capital in capitals) {
			capital.transform.localScale = Vector3.one * capitalScale;
			capital.GetCollider().radius = capitalColliderScale;
			capital.GetController().radius = capitalControllScale;
		}
	}

	private void OnValidate()
	{
		if (autoScaleCapitals) ScaleCapital();
	}

	public static Nation[] GetAllNations()
	{
		return FindObjectsByType<Nation>(FindObjectsSortMode.None);
	}
	public static Warrior[] GetAllWarriors()
	{
		return FindObjectsByType<Warrior>(FindObjectsSortMode.None);
	}

	public static Capital[] GetAllCapitals()
	{
		return FindObjectsByType<Capital>(FindObjectsSortMode.None);
	}

	public static Nation GetNation(string nationName)
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			if (nation.nation == nationName) return nation;
		}

		return null;
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
	public float health;
	public float speed;
	public float damage;
	public int count;

	public WarriorData(float health, float speed, float damage, int count) : this()
	{
		this.health = health;
		this.speed = speed;
		this.damage = damage;
		this.count = count;
	}
}

[System.Serializable]
public struct CapitalData
{
	public float health;
	public WarriorData data
	{
		get { return new WarriorData(health, 0, 0, 1); }
		set { health = value.health; }
	}
}

using UnityEngine;

public class BattleManager : MonoBehaviour
{
	public enum WarriorGroupMode { Nation, Dump };
	public enum NationGroupMode { Manager, Border, Map};
	[Header("Grouping")]
	public WarriorGroupMode warriorGroupMode;
	public NationGroupMode nationGroupMode;

	[Header("Object Data")]
	public Vector2 warriorSpread;
	public Vector2 warriorBoxSize;

	[Header("Sprite Data")]
	public Sprite warriorSprite;
	public Sprite capitalSprite;
	
	[Header("Capital Scaling")]
	public float capitalScale;
	public float capitalColliderScale;
	public float capitalControllScale;
	public bool autoScaleCapitals;

	[Header("Territory Change")]
	public float changeRate;
	float changeTimer;

	[Header("Creation Data")]
	public NationData[] nations;
	public int warriorCount;
	public WarriorData[] warriors;
	public CapitalData capitalData;

	public MapRenderer mapRenderer;
	public MapBorderRenderer borderRenderer;

	[Header("Apply Models")]
	public WarObject.ModelType warriorModel;
	public WarObject.ModelType capitalModel;

	private void Update()
	{
		changeTimer += Time.deltaTime;
		if (changeTimer > changeRate)
		{
			changeTimer = 0;
			var allWarrior = GetAllWarriors();
			Color[] colors = new Color[allWarrior.Length];
			Vector2Int[] positions = new Vector2Int[allWarrior.Length];
			for (int i = 0; i < allWarrior.Length; i++)
			{
				positions[i] = mapRenderer.MapPosition(allWarrior[i].transform.position);
				colors[i] = allWarrior[i].color;
			}

			mapRenderer.SetColors(colors, positions);
		}
	}

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
			foreach (string nationName in nationNames)
			{
				CreateArmiesFromData(warriorData, nationName);
			}
		}
	}

	public void CreateArmiesFromData(WarriorData data, string nationName)
	{
		Vector2 spawn = Vector2.zero;
		Nation nation = GetNation(nationName);
		Capital[] capitals = nation.GetCapitals();
		if (capitals != null) if (capitals.Length > 0) spawn = capitals[0].transform.position;
		for (int i = 0; i < warriorCount; i++)
		{
			GameObject warriorObject = new GameObject(nationName + " " + i.ToString());
			Warrior warrior = warriorObject.AddComponent<Warrior>();
			SetWarriorData(warrior, data, nationName);
			warrior.transform.position = spawn;
			warrior.modelType = warriorModel;
		}
	}

	public Warrior SetWarriorData(Warrior warrior, WarriorData data, string nation)
	{
		warrior = (Warrior)SetWarObjectData(warrior, new(nation, data.health, data.damage));
		warrior.speed = data.speed;
		warrior.gameObject.AddComponent<BoxCollider2D>();

		return warrior;
	}
	public WarObject SetWarObjectData(WarObject obj, WarObjectData data)
	{
		obj.nation = data.nation;
		obj.health = data.health;
		obj.damage = data.damage;

		SpriteRenderer renderer = obj.gameObject.AddComponent<SpriteRenderer>();
		renderer.sprite = warriorSprite;

		return obj;
	}


	[ContextMenu("Create Capitals From Data")]
	public void CreateCapitals() 
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			GameObject capitalObject = new GameObject(nation.nation + " Capital");
			Capital capital = capitalObject.AddComponent<Capital>();
			capital = (Capital) SetWarObjectData(capital, new(nation.nation, capitalData.health, 0));
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
			nation.SetWarAssets();
			nation.SetWarAssetsColor();
		}
	}

	public void SetWarObjectColor(WarObject target)
	{
		GetNation(target.nation).SetWarAssetColor(target);
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

	public static void RemoveNations()
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			Destroy(nation.gameObject);
		}
	}

	public static void RemoveWarriors()
	{
		Warrior[] allWarriors = GetAllWarriors();
		foreach (Warrior warrior in allWarriors)
		{
			Destroy(warrior.gameObject);
		}
	}

	public static void RemoveCapitals()
	{
		var capitals = GetAllCapitals();
		foreach (var cap in capitals) {Destroy(cap.gameObject);}
	}

	[ContextMenu("Group War Objects")]
	public void GroupWarObjects()
	{
		WarObject[] warObjects = GetAllWarObjects();
		Nation[] nations = GetAllNations();
		
		foreach (WarObject warObj in warObjects)
		{
			if (warriorGroupMode == WarriorGroupMode.Dump)
			{
				warObj.transform.parent = transform;
				continue;
			}

			foreach (Nation nation in nations)
			{
				if (warObj.nation != nation.nation) continue;
				warObj.transform.parent = nation.transform;
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
				nation.transform.parent = mapRenderer.transform;
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
		foreach (var warrior in allWarriors) { warrior.sprite = warriorSprite; }
		SetCapitalSprite();
	}

	[ContextMenu("Set Capital Sprite")]
	public void SetCapitalSprite()
	{
		var capitals = GetAllCapitals();
		foreach (var capital in capitals) {capital.sprite = capitalSprite;}
	}

	[ContextMenu("Set Object Data")]
	public void SetObjectData()
	{
		var allWarriors = GetAllWarriors();
		var capitals = GetAllCapitals();
		foreach (var warrior in allWarriors) {
			var collider = warrior.GetComponent<BoxCollider2D>();
			if (collider == null) continue;
			collider.size = warriorBoxSize;
		}

		foreach (var capital in capitals)
		{
			var body = capital.GetComponent<Rigidbody2D>();
			if (body == null) continue;
			body.constraints = RigidbodyConstraints2D.FreezeAll;
		}
	}

	[ContextMenu("Attach AI To All")]
	public void AttachAI()
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			var ai = nation.gameObject.AddComponent<AI>();
			ai.nation = nation;
		}
	}

	[ContextMenu("Set Warrior Model")]
	public void SetWarriorModel()
	{
		var allWarriors = GetAllWarriors();
		SetModel(warriorModel, allWarriors);
	}

	[ContextMenu("Set Capital Model")]
	public void SetCapitalModel()
	{
		SetModel(capitalModel, GetAllCapitals());
	}

	public void SetModel(WarObject.ModelType model, WarObject[] applyObjects)
	{
		UnitModelData modelData = GlobalData.FindModel(model);
		foreach (var unit in applyObjects)
		{
			unit.modelType = model;
		}
	}

	public static T GetNearest<T>(Vector2 position, WarObject self, float maxRadius) where T : WarObject
	{
		var nearbyObjects = Physics2D.OverlapCircleAll(position, maxRadius);
		foreach (var obj in nearbyObjects)
		{
			if (self != null && obj.gameObject.GetInstanceID() == self.gameObject.GetInstanceID()) continue;
			var component = obj.GetComponent<T>();
			if (component == null) continue;
			return component;
		}

		return null;
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
	
	[ContextMenu("Normalize Scales")]
	public void NormalizeScales()
	{
		var warObjects = GetAllWarObjects();
		foreach (var warObject in warObjects)
		{
			Vector3 scale = warObject.transform.localScale;
			warObject.transform.localScale = new Vector3(scale.x, scale.y, (scale.x + scale.y) / 2);
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
	public static WarObject[] GetAllWarObjects()
	{
		return FindObjectsByType<WarObject>(FindObjectsSortMode.None);
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

	public static T[] GetAll<T>() where T : Component
	{
		return FindObjectsByType<T>(FindObjectsSortMode.None);
	}

	public static Nation GetNation(Color color)
	{
		Nation[] allNations = GetAllNations();
		foreach (Nation nation in allNations)
		{
			if (nation.nationColor == color) return nation;
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
public class WarriorData : WarObjectData
{
	public float speed;

	public WarriorData(string nation, float health, float damage, float speed) : base(nation, health, damage)
	{
		this.speed = speed;
	}
}

[System.Serializable]
public struct CapitalData
{
	public float health;
}

[System.Serializable]
public class WarObjectData
{
	public string nation;
	public float health;
	public float damage;

	public WarObjectData(string nation, float health, float damage)
	{
		this.nation = nation;
		this.health = health;
		this.damage = damage;
	}
}
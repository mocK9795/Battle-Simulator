using System.Collections.Generic;
using UnityEngine;

public class Nation : MonoBehaviour
{
    public string nation;
	public Color nationColor;
	public float health;

	public float wealth;
	public float politicalPower;

	public FocusTree focusTree;

	List<WarObject> warAssets = new List<WarObject>();
	Announcement announcer;

	private void OnValidate()
	{
		name = nation;
	}

	private void Start()
	{
		announcer = FindFirstObjectByType<Announcement>();
		gameObject.name = nation;
		SetWarAssets();
		SetWarAssetsColor();

		if (focusTree == null || focusTree.tree.Count < 1) focusTree = GlobalData.GetGenericFocusTree();
	}

	private void Update()
	{
		health = GetNationHealth();
	}

	public void ApplyDevlopmentBonus()
	{

	}

	[ContextMenu("Set War Assets")]
	public void SetWarAssets()
	{
		warAssets.Clear();
		WarObject[] allWarriors = BattleManager.GetAllWarObjects();
		foreach (WarObject warrior in allWarriors)
		{
			if (warrior.nation != nation) continue;
			warAssets.Add(warrior);
        }
	}

	[ContextMenu("Set War Assets Color")]
	public void SetWarAssetsColor()
	{
		foreach (WarObject warAsset in warAssets)
		{
			SetWarAssetColor(warAsset);
		}
	}

	public void SetWarAssetColor(WarObject warAsset)
	{
		warAsset.color = nationColor;
	}

	public Warrior[] GetArmy()
	{
		SetWarAssets();
		List<Warrior> army = new();
		foreach (var asset in warAssets)
		{
			if (asset is Warrior) army.Add((Warrior) asset);
		}

		return army.ToArray();
	}

	public WarObject[] GetWarAssets()
	{
		SetWarAssets();
		return warAssets.ToArray();
	}

	public float GetNationHealth()
	{
		float totalHealth = 0;
		foreach (WarObject warrior in warAssets)
		{
			totalHealth += warrior.health;
		}
		return totalHealth;
	}

	public Capital[] GetCapitals()
	{
		List<Capital> capitals = new List<Capital>();
		
		foreach (Capital capital in BattleManager.GetAllCapitals())
		{
			if (capital.nation == nation) capitals.Add(capital);
		}
		
		return capitals.ToArray();
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		announcer.Announce(nation);
		print("Entering " + nation);
	}
}

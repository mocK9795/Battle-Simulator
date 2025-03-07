using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil.Cil;

public class AI : MonoBehaviour
{
	public Nation nation;
	float timer;

	public void Update()
	{
		timer += Time.deltaTime;
		if (timer < GlobalData.aiThinkSpeed) return;
		timer = 0;

		nation.SetWarAssets();
		if (nation.GetWarAssets().Length < 1) Destroy(gameObject);

		PositionWarriors();
		SendReEnforcements();
		GaurdCapital();
		RecruitWarriors();
	}

	void PositionWarriors() 
	{
		var army = nation.GetArmy();
		if (army.Length < 4) return;
		int skip = GlobalData.mapRenderer.mapData.Length / army.Length;
		int skipCount = 0;
		int armyIndex = 0;
		bool complete = false;
		for (int y = 0; y < army.Length; y++)
		{
			for (int x = 0; x < army.Length; x++)
			{
				Color color = GlobalData.mapRenderer.mapData[x, y];
				if (color == nation.nationColor)
				{
					skipCount++;
					if (skipCount < skip) continue;
					army[armyIndex].target = GlobalData.mapRenderer.WorldPosition(new(x, y));
					armyIndex++;
					if (armyIndex >= army.Length)
					{
						complete = true;
						break;
					}
				}
			}
			if (complete) break;
		}
	}

	void SendReEnforcements()
	{
		var warriors = nation.GetArmy();
		if (warriors.Length == 2) return;
		for (int i = 1; i < warriors.Length; i++)
		{
			if (warriors[i].isAttacking)
			{
				warriors[i - 1].SetTarget(warriors[i].target);
			}
		}
	}

	void GaurdCapital()
	{
		var warriors = nation.GetArmy();

		var capitals = nation.GetCapitals();
		capitals = capitals.OrderBy(obj => obj.controllRadius).ToArray();
		foreach (var capital in capitals)
		{
			if (!capital.underSeige) continue;
			foreach (var warrior in warriors) { warrior.SetTarget(capital.transform.position); }
		}
	}

	void RecruitWarriors()
	{
		var worldData = GlobalData.worldInformation;
		var budgetData = CalculateDivisionBudget(worldData);
		float budget = budgetData.y;
		float buyCount = budgetData.x;
		var buyData = CaculateBestAttributes(budget, worldData);
		if (IsBelowAverage(buyData, worldData)) return;
		var cost = GlobalData.recruiter.GetCost(buyData);
		for (int i = 0; i < buyCount; i++)
		{
			GlobalData.recruiter.RecruitArmy(nation, buyData);
		}
	}

	Vector2 CalculateDivisionBudget(WorldInformation data)
	{
		int divisionLack = nation.GetArmy().Length - data.largetArmySize;
		divisionLack = Mathf.Max(divisionLack, 1);

		return new(divisionLack, nation.wealth / divisionLack);
	}
	public bool IsBelowAverage(WarriorData data, WorldInformation worldData)
	{
		return GlobalData.recruiter.GetCost(data) 
			> GlobalData.recruiter.GetCost(
				new(worldData.averageHealth, 
				worldData.averageSpeed, 
				worldData.averageDamage, 
				data.count
		));
	}
	WarriorData CaculateBestAttributes(float budget, WorldInformation worldData)
	{
		WarriorData bestData = new WarriorData(0, worldData.averageSpeed, 0, 1);

		for (int health = 1; health <= budget / GlobalData.recruiter.health; health++)
		{
			for (int damage = 1; damage <= budget / GlobalData.recruiter.damage; damage++)
			{
				WarriorData data = new(health, worldData.averageSpeed, damage, 1);
				float cost = GlobalData.recruiter.GetCost(data);
				if (cost <= budget && (health > bestData.health || damage > bestData.damage))
				{
					bestData = data;
				}
			}
		}

		return bestData;
	}
}

public class WorldInformation
{
	public int largetArmySize;
	public float averageSpeed;
	public float maxSpeed;
	public float averageHealth;
	public float maxHealth;
	public float averageDamage;
	public float maxDamage;

	public WorldInformation(int largetArmySize = 0, float averageSpeed = 0, float maxSpeed = 0, float averageHealth = 0, float maxHealth = 0, float averageDamage = 0, float maxDamage = 0)
	{
		this.largetArmySize = largetArmySize;
		this.averageSpeed = averageSpeed;
		this.maxSpeed = maxSpeed;
		this.averageHealth = averageHealth;
		this.maxHealth = maxHealth;
		this.averageDamage = averageDamage;
		this.maxDamage = maxDamage;
	}
}

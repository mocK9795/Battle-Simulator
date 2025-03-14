using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class AI : MonoBehaviour
{
	public Nation nation;
	float timer;
	bool updatePositions = true;

	public void Update()
	{
		timer += Time.deltaTime;
		if (timer < GlobalData.aiThinkSpeed) return;
		timer = 0;

		if (updatePositions)
		{
			StartCoroutine(PositionWarriors());
		}
		SendReEnforcements();
		GaurdCapital();
		RecruitWarriors();
	}

	public static Vector2Int[] GetNationArea(Color color)
	{
		int width = GlobalData.mapRenderer.map.width;
		int height = GlobalData.mapRenderer.map.height;

		List<Vector2Int> area = new();

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (GlobalData.mapRenderer.mapData[x, y]  == color) area.Add(new Vector2Int(x, y));
			}
		}

		return area.ToArray();
	}

	IEnumerator PositionWarriors()
	{
		var army = nation.GetArmy();
		if (army.Length < 1) {
			updatePositions = true;
			yield break; 
		}

		List<Vector2> posiblePositions = new();
		for (int y = 0; y < GlobalData.mapRenderer.map.height; y++) {
			yield return new WaitForEndOfFrame();
			for (int x = 0; x < GlobalData.mapRenderer.map.width; x++)
			{
				var color = GlobalData.mapRenderer.mapData[x, y];
				if (color != nation.nationColor) continue;
				posiblePositions.Add(
					GlobalData.mapRenderer.WorldPosition(
						new Vector2Int(x, y)
						));
			}
		}
		
		if (posiblePositions.Count < 1)
		{
			updatePositions = true;
			yield break;
		}

		army = nation.GetArmy();
		if (army.Length < 1)
		{
			updatePositions = true;
			yield break;
		}

		List<Vector2> scatteredPositions = GlobalData.SelectScatteredPoints(posiblePositions, army.Length);
		foreach (var warrior in army)
		{
			float leastDistance = float.MaxValue;
			Vector2 closestPosition = Vector2.one;
			foreach (var position in scatteredPositions)
			{
				if (warrior == null) break; // Somehow the value of warrior can be null
				float distance = Vector2.Distance(position, warrior.transform.position);
				if (distance > leastDistance) continue;
				closestPosition = position;
				leastDistance = distance;
			}
			warrior.SetTarget(closestPosition);
			scatteredPositions.Remove(closestPosition);
		}
		updatePositions = true;
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
	public bool IsBelowAverage(RecruitmentData data, WorldInformation worldData)
	{
		return GlobalData.recruiter.GetCost(data) 
			> GlobalData.recruiter.GetCost(
				new(worldData.averageHealth, 
				worldData.averageSpeed, 
				worldData.averageDamage, 
				data.count
		));
	}
	RecruitmentData CaculateBestAttributes(float budget, WorldInformation worldData)
	{
		RecruitmentData bestData = new RecruitmentData(0, 0, worldData.averageSpeed, 1);

		for (int health = 1; health <= budget / GlobalData.recruiter.health; health++)
		{
			for (int damage = 1; damage <= budget / GlobalData.recruiter.damage; damage++)
			{
				RecruitmentData data = new(health, damage, worldData.averageSpeed, 1);
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

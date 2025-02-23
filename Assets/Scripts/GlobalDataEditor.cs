using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System;

public class GlobalDataEditor : MonoBehaviour
{
	public LayerMask warriorMask;
    public float warriorAttackRange;
	public float moveAccuracy;
	public float rotateAccuracy;
	public float minScale;
	public float friction;
	public float healthToScaleRatio;
	public float knockbackRatio;
	public float capitalChangeHealth = 10;
	public float aiWarriorAttackRange;
	public float aiThinkSpeed;
	public float damageScale;
	public UnitModelData[] unitModelData;

	private void OnValidate()
	{
		SetGlobalData();
	}

	[ContextMenu("Set Data")]
	public void SetGlobalData()
	{
		GlobalData.warriorAttackRange = warriorAttackRange;
		GlobalData.warriorMask = warriorMask;
		GlobalData.moveAccuracy = moveAccuracy;
		GlobalData.rotateAccuracy = rotateAccuracy;
		GlobalData.minScale = minScale;
		GlobalData.friction = friction;
		GlobalData.healthToScaleRatio = healthToScaleRatio;
		GlobalData.knockbackRatio = knockbackRatio;
		GlobalData.capitalChangeHealth = capitalChangeHealth;
		GlobalData.aiWarriorAttackRange = aiWarriorAttackRange;
		GlobalData.aiThinkSpeed = aiThinkSpeed;
		GlobalData.damageScale = damageScale;
		GlobalData.unitModelData = unitModelData;
	}

	[ContextMenu("Apply Data From Model")]
	public void ApplyDataFromModel()
	{
		foreach (var item in unitModelData) 
		{
			item.rotation = item.model.transform.eulerAngles;
			item.scale = item.model.transform.localScale;
			item.name = item.model.name;
		}
	}

	public void GetMousePosition(InputAction.CallbackContext value)
	{
		GlobalData.mousePosition = value.ReadValue<Vector2>();
	}

	private void Start()
	{
		SetGlobalData();
		GlobalData.recruiter = FindFirstObjectByType<RecruitmentManager>();
		GlobalData.mapRenderer = FindFirstObjectByType<MapRenderer>();
	}

	private void Update()
	{
		GlobalData.worldInformation = GlobalData.GetWorldInformation();
	}
}

public static class GlobalData
{
	public static LayerMask warriorMask;
	public static float warriorAttackRange;
	public static float rotateAccuracy;
	public static float moveAccuracy;
	public static Vector2 mousePosition;
	public static Vector2 mouseClickStartPoint;
	public static Vector2 mouseClickEndPoint;
	public static List<Vector2> mousePath = new();
	public static Warrior selectedWarrior;
	public static bool mouseDown;
	public static float minScale;
	public static float friction;
	public static float healthToScaleRatio;
	public static float knockbackRatio;
	public static float capitalChangeHealth = 10;
	public static float aiWarriorAttackRange;
	public static float aiThinkSpeed;
	public static float damageScale;

	public static RecruitmentManager recruiter;
	public static MapRenderer mapRenderer;
	public static WorldInformation worldInformation;
	public static UnitModelData[] unitModelData;

	public static Vector3 vector3(Vector2 vector2) { return new Vector3(vector2.x, vector2.y); }
	public static Vector3[] vector3(Vector2[] points)
	{
		Vector3[] pointsV3 = new Vector3[points.Length];
		for (int i = 0; i < points.Length; i++) { pointsV3[i] = vector3(points[i]); }
		return pointsV3;
	}
	public static Vector2 vector2(Vector3 vector3) { return new Vector2(vector3.x, vector3.y); }
	public static Vector2[] vector2(Vector3[] points)
	{
		Vector2[] pointsV2 = new Vector2[points.Length];
		for (int i = 0; i < points.Length; i++) { pointsV2[i] = points[i]; }
		return pointsV2;
	}
	public static Vector2Int vector2Int(Vector2 vector2) { return new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y)); }
	public static Vector2 vector2(Vector2Int vector2Int) { return new Vector2(vector2Int.x, vector2Int.y); }
	public static Vector3 Inverse(Vector3 value)
	{
		value.x = -value.x;
		value.y = -value.y;
		value.z = -value.z;
		return value;
	}
	public static Vector3 Inverse(Vector2 value)
	{
		return Inverse(vector3(value));
	}
	public static List<Vector2> listVector2(List<Vector2Int> points)
	{
		List<Vector2> nonIntPoints = new();
		foreach (Vector2Int point in points) { nonIntPoints.Add(new Vector2(point.x, point.y)); }
		return nonIntPoints;
	}
	public static List<Vector2Int> listVector2Int(List<Vector2> points)
	{
		List<Vector2Int> intPoints = new();
		foreach (Vector2 point in points) { intPoints.Add(new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y))); }
		return intPoints;
	}
	public static Color Closest(Color c1, Color c2, Color target)
	{
		float distance1 = Distance(c1, target);
		float distance2 = Distance(c2, target);

		return distance1 < distance2 ? c1 : c2;
	}
	public static float Distance(Color c1, Color c2)
	{
		float rDiff = c1.r - c2.r;
		float gDiff = c1.g - c2.g;
		float bDiff = c1.b - c2.b;

		return Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
	}
	public static float Angle(Vector2 position, Vector2 target)
	{
		float x = target.x - position.x;
		float y = target.y - position.y;
		return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}
	public static WorldInformation GetWorldInformation()
	{
		Nation[] nations = BattleManager.GetAllNations();

		if (nations.Length < 1) return null;

		WorldInformation data = new(0);

		foreach (var state in nations)
		{
			var army = state.GetArmy();
			if (army.Length > data.largetArmySize) data.largetArmySize = army.Length;

			foreach (var armed in army) { data = FactorWarriorInData(armed, data); }
		}

		return data;
	}
	public static WorldInformation FactorWarriorInData(Warrior warrior, WorldInformation data)
	{
		data.maxDamage = Mathf.Max(warrior.damage, data.maxDamage);
		data.maxHealth = Mathf.Max(warrior.health, data.maxHealth);
		data.maxSpeed = Mathf.Max(warrior.speed, data.maxSpeed);

		data.averageDamage = (data.averageDamage + warrior.damage) / 2;
		data.averageHealth = (data.averageHealth + warrior.health) / 2;
		data.averageSpeed = (data.averageSpeed + warrior.speed) / 2;

		return data;
	}
	public static float DistanceFromOrigin(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}
	public static float DistanceFromOrigin(Vector2 vector2)
	{
		return DistanceFromOrigin(vector2.x, vector2.y);
	}
	public static List<Vector2> SelectScatteredPoints(List<Vector2> points, int numberOfPointsToSelect)
	{
		if (numberOfPointsToSelect <= 0 || points == null || points.Count == 0)
		{
			throw new ArgumentException("Invalid number of points to select or empty points list.");
		}

		// Sort points by distance from the origin (0, 0)
		points = points.OrderBy(p => DistanceFromOrigin(p)).ToList();

		// Select points in such a way that they are scattered as evenly as possible
		List<Vector2> selectedPoints = new List<Vector2>();
		int step = points.Count / numberOfPointsToSelect;
		for (int i = 0; i < numberOfPointsToSelect; i++)
		{
			selectedPoints.Add(points[i * step]);
		}

		return selectedPoints;
	}
	public static UnitModelData FindModel(string name)
	{
		foreach (var data in unitModelData) {if (data.name == name) return data; }
		return null;
	}
}


[System.Serializable]
public class UnitModelData
{
	public string name;
	public GameObject model;
	public Vector3 rotation;
	public Vector3 scale;
	public Vector2 box;
	public Vector2 boxOffset;
}
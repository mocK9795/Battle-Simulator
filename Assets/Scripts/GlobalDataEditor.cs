using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Rendering;

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
	}

	public void GetMousePosition(InputAction.CallbackContext value)
	{
		GlobalData.mousePosition = value.ReadValue<Vector2>();
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
	public static Warrior selectedWarrior;
	public static bool mouseDown;
	public static float minScale;
	public static float friction;
	public static float healthToScaleRatio;
	public static float knockbackRatio;
	public static float capitalChangeHealth = 10;
	public static float aiWarriorAttackRange;
	public static float aiThinkSpeed;


	public static Vector3 vector3(Vector2 vector2) { return new Vector3(vector2.x, vector2.y); }
	public static Vector2 vector2(Vector3 vector3) { return new Vector2(vector3.x, vector3.y); }
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
}

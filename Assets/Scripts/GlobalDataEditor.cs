using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GlobalDataEditor : MonoBehaviour
{
	public LayerMask warriorMask;
    public float warriorAttackRange;
	public float moveAccuracy;
	public float rotateAccuracy;
	public float minScale;
	public float friction;
	public float healthToScaleRatio;

	private void OnValidate()
	{
		GlobalData.warriorAttackRange = warriorAttackRange;
		GlobalData.warriorMask = warriorMask;
		GlobalData.moveAccuracy = moveAccuracy;
		GlobalData.rotateAccuracy = rotateAccuracy;
		GlobalData.minScale = minScale;
		GlobalData.friction = friction;
		GlobalData.healthToScaleRatio = healthToScaleRatio;
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

	public static Vector3 vector3(Vector2 vector2) { return new Vector3(vector2.x, vector2.y); }
}

using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class Player : MonoBehaviour
{
    public string nation;
    float lookSpeed;
    public float zoomSpeed;
    public float minZoom;
    public float maxZoom;
    Camera mainCamera;
    VisualEffects effects;
    BattleManager battle;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        effects = FindFirstObjectByType<VisualEffects>();
        battle = FindFirstObjectByType<BattleManager>();
        lookSpeed = mainCamera.orthographicSize;

        Nation playerNation = BattleManager.GetNation(nation);
        foreach (var warrior in playerNation.GetArmy())
        {
            warrior.useAi = false;
        }
    }

    public Warrior GetSelectedWarrior()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(GlobalData.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Warrior>();
        }

        return null;
    }

    public void OnClick(InputAction.CallbackContext value)
    {
        GlobalData.mouseDown = !value.canceled;
        if (value.started) {
            GlobalData.mouseClickStartPoint = GlobalData.mousePosition;
            GlobalData.selectedWarrior = GetSelectedWarrior();
        }

        if (value.canceled)
        {
            effects.ClearArrow();
            GlobalData.mouseClickEndPoint = GlobalData.mousePosition;

            if (GlobalData.selectedWarrior == null) return;

            Vector2 clickStart = WorldPosition(GlobalData.mouseClickStartPoint);
            Vector2 clickEnd = WorldPosition(GlobalData.mouseClickEndPoint);
            GlobalData.selectedWarrior.SetTargetFromOffset(clickEnd - clickStart);

            GlobalData.selectedWarrior = null;
        }
    }

    public Vector2 WorldPosition(Vector2 position)
    {
        return mainCamera.ScreenToWorldPoint(position);
    }

    private void Update()
    {
        if (GlobalData.mouseDown && GlobalData.selectedWarrior != null)
        {
            effects.DrawArrow(WorldPosition(GlobalData.mouseClickStartPoint), WorldPosition(GlobalData.mousePosition));
        }
    }

    public void OnLook(InputAction.CallbackContext value)
    {
        if (!GlobalData.mouseDown || GlobalData.selectedWarrior != null) return;
        transform.position += GlobalData.Inverse( value.ReadValue<Vector2>() ) * Time.deltaTime * lookSpeed;
    }

    public void OnZoom(InputAction.CallbackContext value)
    {
        OnZoom(-value.ReadValue<Vector2>().y);
    }
	public void OnZoom(float zoomValue)
    {
		mainCamera.orthographicSize = mainCamera.orthographicSize + zoomValue * zoomSpeed;
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
        lookSpeed = mainCamera.orthographicSize;
	}

    public void MarchArmy() 
    {
        var playerNation = BattleManager.GetNation(nation);
        foreach (var warrior in playerNation.GetArmy())
        {
			Vector2 start = warrior.transform.position;
			Vector2 mousePosition = WorldPosition(GlobalData.mousePosition);
            warrior.SetTargetFromOffset(mousePosition - start);
        }
    }

    public void MarchArmy(InputAction.CallbackContext value) { if (value.canceled) MarchArmy(); }
}

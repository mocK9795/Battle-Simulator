using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public string nation;
    public float lookSpeed;
    Camera mainCamera;
    VisualEffects effects;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        effects = FindFirstObjectByType<VisualEffects>();
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

            Vector2 clickStart = mainCamera.ScreenToWorldPoint(GlobalData.mouseClickStartPoint);
            Vector2 clickEnd = mainCamera.ScreenToWorldPoint(GlobalData.mouseClickEndPoint);
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
}

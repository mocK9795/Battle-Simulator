using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public string nation;
    Camera mainCamera;

	private void Start()
	{
		mainCamera = GetComponent<Camera>();
	}

	public Warrior GetSelectedWarrior()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(GlobalData.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        print(hit.point);

        if (hit.collider != null)
        {
            Debug.Log("Object Selected: " + hit.collider.name);
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
            GlobalData.mouseClickEndPoint = GlobalData.mousePosition;

            if (GlobalData.selectedWarrior == null) return;

            Vector2 clickStart = mainCamera.ScreenToWorldPoint(GlobalData.mouseClickStartPoint);
            Vector2 clickEnd = mainCamera.ScreenToWorldPoint(GlobalData.mouseClickEndPoint);
            GlobalData.selectedWarrior.SetTargetFromOffset(clickEnd - clickStart);

            GlobalData.selectedWarrior = null;
        }
    }
}

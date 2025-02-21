using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;

public class Player : MonoBehaviour
{
    public string nation;
    float lookSpeed;
    public float lookSpeedMultiplyier;
    public float zoomSpeed;
    public float minZoom;
    public float maxZoom;
    public enum InputMode {Direct, Raycast};
    public InputMode inputMode = InputMode.Direct;
    public float step;


    [Header("Selection Mode")]
    public Image selectModeButton;
    public Sprite selectModeOn;
    public Sprite selectModeOff;
    public Sprite warriorSelectedSprite;
    public Sprite warriorSprite;

    [Header("Enable Unit AI")]
    public Image enableUnitAiButton;
    public Sprite unitAiOn;
    public Sprite unitAiOff;
    public Sprite aiControlledUnitImage;
    bool unitAi = false;

    List<Warrior> selectedWarriors = new();
    List<Warrior> lastSelected = new();
    Camera mainCamera;
    VisualEffects effects;
    BattleManager battle;
    RecruitmentManager recruiter;
    Nation playerNation;
    bool selectMode = false;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        effects = FindFirstObjectByType<VisualEffects>();
        battle = FindFirstObjectByType<BattleManager>();
        recruiter = FindFirstObjectByType<RecruitmentManager>();
        lookSpeed = mainCamera.orthographicSize;

        playerNation = BattleManager.GetNation(nation);
        foreach (var warrior in playerNation.GetArmy())
        {
            warrior.useAi = false;
        }

    }

    public Warrior GetSelectedWarrior()
    {
        Vector2 mousePosition = WorldPosition(GlobalData.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            var warrior = hit.collider.GetComponent<Warrior>();
            if (warrior.nation == nation) return warrior;
            else return null;
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

            if (GlobalData.selectedWarrior == null && selectMode) OnSelect();
            if (GlobalData.selectedWarrior == null) { GlobalData.mousePath.Clear(); return; }

            var path = WorldPosition(GlobalData.mousePath.ToArray());

			if (!selectMode)
            {
                GlobalData.selectedWarrior.SetTarget(path);
                GlobalData.selectedWarrior = null;
            }
            if (selectMode)
            {
                foreach (var warrior in selectedWarriors) {warrior.SetTarget(path);}
                ClearSelection();
            }
            GlobalData.mousePath.Clear();
        }
    }

    public void OnSelect()
    {
        Vector2 startPoint = WorldPosition(GlobalData.mouseClickStartPoint);
        Vector2 endPoint = WorldPosition(GlobalData.mouseClickEndPoint);
		Vector2 boxCenter = (startPoint + endPoint) / 2;
		Vector2 boxSize = new Vector2(Mathf.Abs(endPoint.x - startPoint.x), Mathf.Abs(endPoint.y - startPoint.y));

        lastSelected = new(selectedWarriors);
        ClearSelection();

        Collider2D[] selectedObjects = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
        foreach (var obj in selectedObjects) { 
            Warrior warrior = obj.GetComponent<Warrior>();
            if (warrior == null) continue;
            if (warrior.nation != nation) continue;
            selectedWarriors.Add(warrior);
            warrior.sprite = warriorSelectedSprite;
        }
    }

    public Vector2 WorldPosition(Vector2 position)
    {
        if (inputMode == InputMode.Direct) return mainCamera.ScreenToWorldPoint(position);
        else
        {
            Ray ray = mainCamera.ScreenPointToRay(position);
            Vector3 closestPoint = new Vector3(0, 0, float.MinValue);
            float currentDistance = 0;
            while (true) {
                var newPoint = ray.GetPoint(currentDistance);
                if (Mathf.Abs(newPoint.z) > Mathf.Abs(closestPoint.z)) break;
                closestPoint = newPoint;
                currentDistance += step;
            }
            return closestPoint;
        }
    }
    public Vector2[] WorldPosition(Vector2[] positions)
    {
        for (int i = 0; i < positions.Length; i++) { positions[i] = WorldPosition(positions[i]);}
        return positions;
    }

    private void Update()
    {
        FixSelection();

        if (GlobalData.mouseDown && GlobalData.selectedWarrior != null)
        {
            effects.DrawArrow(WorldPosition(GlobalData.mousePath.ToArray()));
        }

        if (GlobalData.mouseDown && GlobalData.selectedWarrior == null & selectMode)
        {
			Vector2 startPoint = WorldPosition(GlobalData.mouseClickStartPoint);
			Vector2 endPoint = WorldPosition(GlobalData.mousePosition);
			Vector2 boxCenter = (startPoint + endPoint) / 2;
			Vector2 boxSize = new Vector2(Mathf.Abs(endPoint.x - startPoint.x), Mathf.Abs(endPoint.y - startPoint.y));

            effects.DrawBox(boxCenter, boxSize);
		}
        else
        {
            effects.ClearBox();
        }

		effects.SetWealthCount(playerNation.wealth);
	}

	public void OnLook(InputAction.CallbackContext value)
    {
        if (GlobalData.mouseDown) GlobalData.mousePath.Add(GlobalData.mousePosition);
        if (!GlobalData.mouseDown || GlobalData.selectedWarrior != null || selectMode) return;
        transform.position += GlobalData.Inverse( value.ReadValue<Vector2>() ) * Time.deltaTime * lookSpeed;
    }

    public void OnZoom(InputAction.CallbackContext value)
    {
        OnZoom(-value.ReadValue<Vector2>().y);
    }
	public void OnZoom(float zoomValue)
    {
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = mainCamera.orthographicSize + zoomValue * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
            lookSpeed = mainCamera.orthographicSize * lookSpeedMultiplyier;
            return;
        }

        transform.position += Vector3.forward * zoomValue * zoomSpeed;
        transform.position = new Vector3(
            transform.position.x, 
            transform.position.y, 
            Mathf.Clamp(transform.position.z, minZoom, maxZoom
            ));
        lookSpeed = -transform.position.z * lookSpeedMultiplyier;
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

    public void OnSelectModeClick()
    {
        selectMode = !selectMode;
        if (selectMode) selectModeButton.sprite = selectModeOn;
        else selectModeButton.sprite = selectModeOff;
    }

    public void OnRecruitMenuOpen()
    {
        recruiter.ShowMenu(true);
        recruiter.ShowDetails(recruiter.testData);
    }

    public void OnRecruitConfirm()
    {
        recruiter.RecruitArmy(playerNation, recruiter.data);
    }

    public void OnUnitAiClick()
    {
        unitAi = !unitAi;
        if (unitAi) enableUnitAiButton.sprite = unitAiOn;
        else enableUnitAiButton.sprite = unitAiOff;

        if (selectMode)
        {
            foreach (var selected in lastSelected)
            {
                selected.useAi = unitAi;
                if (unitAi) selected.sprite = aiControlledUnitImage;
                else selected.sprite = warriorSprite;
			}
            return;
		}

        var warriors = playerNation.GetArmy();
        foreach (Warrior warrior in warriors) {
            warrior.useAi = unitAi;
            if (unitAi) warrior.sprite = aiControlledUnitImage;
            else warrior.sprite = warriorSprite;
		}
	}

    void ClearSelection()
    {
        foreach (var warrior in selectedWarriors) { warrior.sprite = warriorSprite; }
        selectedWarriors.Clear();
    }

    void FixSelection()
    {
        foreach (var warrior in selectedWarriors) { if (warrior == null) Destroy(warrior); }
        foreach (var warrior in lastSelected) { if (warrior == null) Destroy(warrior); }
	}
}

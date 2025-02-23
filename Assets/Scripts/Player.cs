using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

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
    public enum PathDrawMode {Direct, Optimize};
    public PathDrawMode pathDrawMode = PathDrawMode.Direct;
    [Tooltip("How precise the raycast is")]
    public float step;
    public Vector3 minZoomRotation;
    public Vector3 maxZoomRotation;

    [Header("Selection Mode")]
    public float pathOptimizationThreshold;
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
            if (warrior == null) return null;
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
            if (pathDrawMode == PathDrawMode.Optimize) path = GlobalData.vector2(BorderPointOrdering.OptimizePath(GlobalData.vector3(path)));

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
        SetRotation();

        if (GlobalData.mouseDown && GlobalData.selectedWarrior != null)
		{
            var path = WorldPosition(GlobalData.mousePath.ToArray());
            if (pathDrawMode == PathDrawMode.Optimize)
            {
                path = GlobalData.vector2(BorderPointOrdering.OptimizePath(GlobalData.vector3(path)));
                //if (GlobalData.mousePath.Count > path.Length) print("Real " + GlobalData.mousePath.Count + " Optimized " + path.Length);
                if (GlobalData.mousePath.Count > pathOptimizationThreshold)
                {
                    Vector2 start = GlobalData.mousePath[0];
                    Vector2 end = GlobalData.mousePath[GlobalData.mousePath.Count - 1];
                    GlobalData.mousePath = new(path);
                    GlobalData.mousePath[0] = start;
                    GlobalData.mousePath.Add(end);
                }
            }
			effects.DrawArrow(path);
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

    void SetRotation()
    {
		Quaternion newRotation = Quaternion.Lerp(
            Quaternion.Euler(minZoomRotation),
            Quaternion.Euler(maxZoomRotation),
            (-transform.position.z) / Mathf.Abs(minZoom)
        );
		transform.rotation = newRotation;
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
			}
            return;
		}

        var warriors = playerNation.GetArmy();
        foreach (Warrior warrior in warriors) {
            warrior.useAi = unitAi;
		}
	}

    void ClearSelection()
    {
        selectedWarriors.Clear();
    }

    void FixSelection()
    {
        foreach (var warrior in selectedWarriors) { if (warrior == null) Destroy(warrior); }
        foreach (var warrior in lastSelected) { if (warrior == null) Destroy(warrior); }
	}
}

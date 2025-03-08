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
    public enum InputMode { Direct, Raycast };
    public InputMode inputMode = InputMode.Direct;
    public enum PathDrawMode { Direct, Optimize };
    public PathDrawMode pathDrawMode = PathDrawMode.Direct;
    [Tooltip("How precise the raycast is")]
    public float step;
    public Vector3 minZoomRotation;
    public Vector3 maxZoomRotation;
    public float dragThresshold;
    float dragAmount;
    bool isDraging;
    [Tooltip("From how far away a model can be seen")]
    public float modelRenderDist;
    public float cameraLagFlickCancelThresshold;
    //public Vector4 cameraLimit;

    [Header("Selection Mode")]
    public float pathOptimizationThreshold;
    public Image selectModeButton;
    public Sprite selectModeOn;
    public Sprite selectModeOff;
    public Sprite warriorSelectedSprite;
    public Sprite warriorSprite;
    [HideInInspector]
	public bool selectMode = false;

	[Header("Enable Unit AI")]
    public Image enableUnitAiButton;
    public Sprite unitAiOn;
    public Sprite unitAiOff;
    public Sprite aiControlledUnitImage;
    bool unitAi = false;

    [Header("Inspect Mode")]
    public NationShowcase showcase;
    public Image inspectImage;
    public Sprite inspectModeOff;
    public Sprite inspectModeOn;
    bool inspect = false;

    [Header("Contruct Mode")]
    public ContructionMenu contructionMenu;
    public Image contructImage;
    public Sprite contructModeOn;
    public Sprite contructModeOff;
    bool contruction = false;

    [HideInInspector]
    public List<Warrior> selectedWarriors = new();
    List<Warrior> lastSelected = new();
    Camera mainCamera;
    VisualEffects effects;
    BattleManager battle;
    EconomyManager economy;
    RecruitmentManager recruiter;
    MapRenderer mapRenderer;
    MapBorderRenderer mapBorderRenderer;
    ArmyManagement armyManagement;
    Nation playerNation;

    private void Start()
    {
        mainCamera = GetComponent<Camera>();
        effects = FindFirstObjectByType<VisualEffects>();
        battle = FindFirstObjectByType<BattleManager>();
        recruiter = FindFirstObjectByType<RecruitmentManager>();
        mapBorderRenderer = FindFirstObjectByType<MapBorderRenderer>();
        mapRenderer = FindFirstObjectByType<MapRenderer>();
        economy = FindFirstObjectByType<EconomyManager>();
        armyManagement = FindFirstObjectByType<ArmyManagement>();
        lookSpeed = mainCamera.orthographicSize;

        playerNation = BattleManager.GetNation(nation);
        foreach (var warrior in playerNation.GetArmy())
        {
            warrior.useAi = false;
        }
        var ai = playerNation.GetComponent<AI>();
        if (ai != null) ai.enabled = false;

    }

    public Warrior GetSelectedWarrior()
    {
        var selection = GetClickedObject();
        if (selection == null) return null;
        var warrior = selection.GetComponent<Warrior>();
        if (warrior == null) return null;
        if (warrior.nation == nation) return warrior;
        return null;
    }

    public Nation GetSelectedNation()
    {
        var selectionPosition = mapRenderer.MapPosition(WorldPosition(GlobalData.mousePosition));
        var selectionColor = mapRenderer.mapData[selectionPosition.x, selectionPosition.y];
        var selectionNation = BattleManager.GetNation(selectionColor);
        return selectionNation;
    }

    Collider2D GetClickedObject()
    {
        Vector2 mousePosition = WorldPosition(GlobalData.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        return hit.collider;
    }

    public void OnClick(InputAction.CallbackContext value)
    {
        GlobalData.mouseDown = !value.canceled;
        if (value.started) {
            GlobalData.mouseClickStartPoint = GlobalData.mousePosition;
            GlobalData.selectedWarrior = GetSelectedWarrior();
            dragAmount = 0;
            isDraging = false;
        }

        if (value.canceled)
        {
            armyManagement.OnClick();
            effects.ClearArrow();
            GlobalData.mouseClickEndPoint = GlobalData.mousePosition;

            if (contruction && !isDraging) { OnContruct(); return; }
            if (inspect && !isDraging) OnInspect();
            if (GlobalData.selectedWarrior == null && selectMode && !inspect) OnSelect();
            if (GlobalData.selectedWarrior == null) { GlobalData.mousePath.Clear(); return; }
            if (!isDraging) { OpenWarriorPopup(GlobalData.selectedWarrior); return; }

            var path = WorldPosition(GlobalData.mousePath.ToArray());
            if (pathDrawMode == PathDrawMode.Optimize) path = GlobalData.vector2(BorderPointOrdering.OptimizePath(GlobalData.vector3(path)));

            if (!selectMode)
            {
                GlobalData.selectedWarrior.SetTarget(path);
                GlobalData.selectedWarrior = null;
            }
            if (selectMode)
            {
                foreach (var warrior in selectedWarriors) { warrior.SetTarget(path[path.Length - 1]); }
                ClearSelection();
            }
            GlobalData.mousePath.Clear();
        }
    }


    public void OpenWarriorPopup(Warrior warrior)
    {
        if (GlobalData.popupPrefab == null) return;
        var popupObj = Instantiate(GlobalData.popupPrefab, FindFirstObjectByType<Canvas>().transform);
        var popup = popupObj.GetComponent<Popup>();

        popup.Activate();
        popup.SetContraint(GridLayoutGroup.Constraint.FixedColumnCount, 1);
        popup.Message("Manpower " + warrior.health.ToString());
        popup.Message("Damage " + warrior.damage.ToString());
        popup.Message("Speed " + warrior.speed.ToString());
        popup.FinalizeLayout();
    }

    void OnSelect()
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
            warrior.outline = true;
            selectedWarriors.Add(warrior);
        }
    }

    public void OnInspect()
    {
        var nation = GetSelectedNation();
        if (nation == null) return;
        showcase.ShowcaseNation(nation);
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
        for (int i = 0; i < positions.Length; i++) { positions[i] = WorldPosition(positions[i]); }
        return positions;
    }

    private void Update()
    {
        FixSelection();
        SetRotation();

        if (GlobalData.mouseDown && GlobalData.selectedWarrior != null && !inspect)
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

        if (GlobalData.mouseDown && GlobalData.selectedWarrior == null && selectMode && !inspect)
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

		var dislocation = GlobalData.Inverse(value.ReadValue<Vector2>()) * Time.deltaTime * lookSpeed;
		if (dragAmount > dragThresshold) isDraging = true;
		dragAmount += dislocation.magnitude;

		if (!GlobalData.mouseDown || GlobalData.selectedWarrior != null || selectMode) return;

        if (dislocation.magnitude > cameraLagFlickCancelThresshold) return;
		transform.position += dislocation;

		//transform.position = new Vector3(
	     //   Mathf.Clamp(transform.position.x, cameraLimit.x, cameraLimit.y),
	     //   Mathf.Clamp(transform.position.y, cameraLimit.z, cameraLimit.w),
	     //   transform.position.z
	    //);
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
            SetModelRendering();
            return;
        }

        transform.position += Vector3.forward * zoomValue * zoomSpeed;
        transform.position = new Vector3(
            transform.position.x, 
            transform.position.y, 
            Mathf.Clamp(transform.position.z, minZoom, maxZoom
            ));
        lookSpeed = -transform.position.z * lookSpeedMultiplyier;
        SetModelRendering();
	}

    void SetModelRendering()
    {
        float dist = Mathf.Abs(transform.position.z);
        WarObject[] allObjects = BattleManager.GetAllWarObjects();
        GlobalData.showModel = dist > modelRenderDist;
        if (dist > modelRenderDist) foreach (var obj in allObjects) { obj.SetModel(WarObject.ModelType.Sprite); }
        else foreach (var obj in allObjects) { obj.SetModel(); }
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
        recruiter.ShowDetails(recruiter.data);
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

    public void OnSearchClick()
	{
		inspect = !inspect;
		if (inspect) inspectImage.sprite = inspectModeOn;
		if (!inspect) inspectImage.sprite = inspectModeOff;
    }

    public void OnConstructClick()
    {
        contruction = !contruction;
        if (contruction) { contructImage.sprite = contructModeOn; contructionMenu.Activate(); }
        else {contructImage.sprite = contructModeOff; contructionMenu.menu.SetActive(false); }
    }

    public void OnContruct()
    {
        var position = WorldPosition(GlobalData.mousePosition);
        var mapPosition = mapRenderer.MapPositionInOriginContext(position);
        Site site = contructionMenu.site.Copy();
        site.position = mapPosition;
        economy.ContructSite(site, playerNation, mapPosition);
    }

    void ClearSelection()
    {
        foreach (var warrior in selectedWarriors) warrior.outline = false;
        selectedWarriors.Clear();
    }

    void FixSelection()
    {
        foreach (var warrior in selectedWarriors) { if (warrior == null) Destroy(warrior); }
        foreach (var warrior in lastSelected) { if (warrior == null) Destroy(warrior); }
	}
}

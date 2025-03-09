using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ArmyManagement : MonoBehaviour
{
    [Tooltip("Must have a text component (army number)")]
    public GameObject preset;
	public GameObject activeObject;
    public GameObject container;
	public GameObject taskbar;
    public Player player;
	public int armyGroupCount;
    List<List<Warrior>> armyContainers;
	int selectedArmy = 0;
	float containerCloseDelay = 0.1f;
	float timeSinceContainerClosed;
	public List<Warrior> selectedWarriors { get { return armyContainers[selectedArmy]; } }

	private void Start()
	{
		armyContainers = new();
		var btns = new List<ArmyButton>(BattleManager.GetAll<ArmyButton>());
		for (int i = 0; i < armyGroupCount; i++) 
		{
			if (i > btns.Count - 1) btns.Add(Instantiate(preset, container.transform).GetComponent<ArmyButton>());
			btns[i].id = i; armyContainers.Add(new()); 
		}
	}

	public void OnArmyAdd() 
	{
		CleanArmyRefrences();
		OnRemoveFromArmy();
		armyContainers[selectedArmy].AddRange(player.selectedWarriors);
		player.ClearSelection();
		OutlineSelected(true);
	}

	void OutlineSelected(bool outline) 
	{ foreach (var warrior in armyContainers[selectedArmy]) { warrior.outline = outline; } }

	public void OnArmyClick(int index)
	{
		CleanArmyRefrences();
		if (index == selectedArmy) return;
		OutlineSelected(false);
		timeSinceContainerClosed = 0;
		selectedArmy = index;
		OutlineSelected(true);
		activeObject.transform.SetParent(container.transform, false);
		var selectedButton = FindButton(selectedArmy);
		selectedButton.transform.SetParent(taskbar.transform, false);
		activeObject = selectedButton.gameObject;
	}

	ArmyButton FindButton(int index)
	{
		var btns = BattleManager.GetAll<ArmyButton>();
		foreach (var button in btns)
		{
			if (button.id == index) return button;
		}
		return null;
	}

	public void OnRemoveFromArmy()
	{
		CleanArmyRefrences();
		if (player.selectMode)
		foreach (var army in player.selectedWarriors) {RemoveWarrior(army);}
	}

	void RemoveWarrior(Warrior warrior)
	{
		foreach (var armyContainer in armyContainers)
		{
			for (int i = 0; i < armyContainer.Count; i++)
			{
				if (armyContainer[i].gameObject.GetInstanceID() == warrior.gameObject.GetInstanceID())
				{ armyContainer[i].outline = false; armyContainer.RemoveAt(i); return; }
			}
		}
	}

	void CleanArmyRefrences()
	{
		foreach (var armyContainer in armyContainers)
		{
			for (int i = 0; i < armyContainer.Count; i++)
			{
				if (armyContainer[i] == null) { armyContainer.RemoveAt(i); i--; }
			}
		}
	}

	public void OnGridOpen()
	{
		timeSinceContainerClosed = 0;
		container.SetActive(true);
	}
 
	private void Update()
	{
		if (timeSinceContainerClosed < containerCloseDelay) timeSinceContainerClosed += Time.deltaTime;
	}

	public void OnClick()
	{
		if (timeSinceContainerClosed < containerCloseDelay) return;
		StartCoroutine(DeactivateGrid());
	}

	IEnumerator DeactivateGrid()
	{
		yield return new WaitForSeconds(containerCloseDelay * .5f);
		if (timeSinceContainerClosed < containerCloseDelay) yield break;
		container.SetActive(false);
		yield break;
	}
}

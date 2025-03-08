using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class ArmyManagement : MonoBehaviour
{
    [Tooltip("Must have a text component (army number)")]
    public GameObject preset;
	public GameObject activeObject;
    public GameObject container;
	public GameObject taskbar;
    public Player player;
    List<List<Warrior>> armyContainers;
	int selectedArmy = 0;
	float gridCloseDelay = 0.1f;
	float gridCloseTimer;
	public List<Warrior> selectedWarriors { get { return armyContainers[selectedArmy]; } }

	private void Start()
	{
		armyContainers = new();
	}

	public void OnArmyAdd() 
	{
		CleanArmyRefrences();
		OnRemoveFromArmy();
		armyContainers[selectedArmy].AddRange(player.selectedWarriors);
	}

	public void OnArmyClick(int index)
	{
		CleanArmyRefrences();
		if (player.selectMode)
		{
			return;
		}
		selectedArmy = index;
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
				if (armyContainer[i].gameObject.GetInstanceID() == warrior.GetInstanceID())
				{ armyContainer.RemoveAt(i); return; }
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
		gridCloseTimer = 0;
		container.SetActive(true);
	}
 
	private void Update()
	{
		if (gridCloseTimer < gridCloseDelay) gridCloseTimer += Time.deltaTime;
	}

	public void OnClick()
	{
		if (gridCloseTimer < gridCloseDelay) return;
		container.SetActive(false);
	}
}

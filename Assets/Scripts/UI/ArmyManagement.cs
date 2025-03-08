using UnityEngine;
using System.Collections.Generic;

public class ArmyManagement : MonoBehaviour
{
    [Tooltip("Must have a text component (army number)")]
    public GameObject preset;
    public GameObject container;
    public Player player;
    List<List<Warrior>> armyContainers;
	int selectedArmy = 0;

	private void Start()
	{
		armyContainers = new();
	}

	public void OnArmyAdd() 
	{
		CleanArmyRefrences();

	}

	public void OnArmyClick(int index)
	{
		selectedArmy = index;
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
}

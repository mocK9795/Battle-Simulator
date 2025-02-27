using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FocusDisplay : MonoBehaviour
{
	public GameObject menu;
	public GameObject contianer;
	public GameObject focusUiPreset;
	public float scale;

	FocusTree activeTree;
	Nation activeNation;
	List<FocusBtn> buttons = new();

	public static Dictionary<string, (int, int)> GenerateFocusPosition(List<Focus> focusTree)
	{
		Dictionary<string, (int, int)> positions = new Dictionary<string, (int, int)>();
		Dictionary<string, int> levels = new Dictionary<string, int>();
		Dictionary<int, List<string>> levelSkills = new Dictionary<int, List<string>>();

		foreach (var skill in focusTree)
		{
			int level = GetFocusLevel(skill, levels, focusTree);
			if (!levelSkills.ContainsKey(level))
			{
				levelSkills[level] = new List<string>();
			}
			levelSkills[level].Add(skill.name);
		}

		foreach (var kvp in levelSkills)
		{
			int level = kvp.Key;
			int index = 0;
			foreach (var skillName in kvp.Value)
			{
				positions[skillName] = (level, index);
				index++;
			}
		}
		return positions;
	}
	private static int GetFocusLevel(Focus skill, Dictionary<string, int> levels, List<Focus> tree)
	{
		if (levels.ContainsKey(skill.name))
		{
			return levels[skill.name];
		}

		int maxDependencyLevel = 0;
		foreach (var dependency in skill.requirements)
		{
			int dependencyLevel = GetFocusLevel(GetFocus(dependency, tree.ToArray()), levels, tree);
			if (dependencyLevel + 1 > maxDependencyLevel)
			{
				maxDependencyLevel = dependencyLevel + 1;
			}
		}

		levels[skill.name] = maxDependencyLevel;
		return maxDependencyLevel;
	}

	public static Focus GetFocus(string focus, Focus[] tree) { foreach (var treeElement in tree) { if (treeElement.name == focus) return treeElement; } return null; }

	public void PlaceFocusUI(FocusTree focusTree, Nation nation)
	{
		menu.SetActive(true);
		var positions = GenerateFocusPosition(focusTree.tree);
		activeTree = focusTree;
		activeNation = nation;
		
		for (int i = 0; i<focusTree.tree.Count; i++)
		{
			var focus = focusTree.tree[i];
			var ui = Instantiate(focusUiPreset, contianer.transform);
			
			Vector2 position = GlobalData.vector3(positions[focus.name]) * scale;
			ui.GetComponent<RectTransform>().anchoredPosition = position;
			ui.GetComponent<Image>().sprite = focus.image;

			var button = ui.GetComponent<FocusBtn>();
			button.display = this;
			button.focusName = focus.name;
			buttons.Add(button);
		}
	}

	public void OnFocusClick(string focusName)
	{
		var focus = activeTree.GetFocus(focusName);
		var btn = GetButton(focus);
		StartCoroutine(activeTree.AttemptCompletion(focus, activeNation, btn));
	}

	public FocusBtn GetButton(Focus focus)
	{
		foreach (var button in buttons) { if (button.focusName == focus.name) return button; } return null;
	}
}

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Focus
{
    public string name;
    public Sprite image;
    public List<string> requirements;
    public float politicalCost;
    public float economicCost;
    public float timeCost;
    public bool underWork;
    public bool isComplete;
}

[System.Serializable]
public class FocusTree
{
	public List<Focus> tree = null;

	public FocusTree(List<Focus> tree)
	{
		this.tree = tree;
	}

    public Focus GetFocus(string name)
    {
        foreach (Focus focus in tree) { if (focus.name == name) return focus; }
        return null;
    }

	public IEnumerator AttemptCompletion(Focus focus, Nation nation, FocusBtn focusBtn)
    {
        if (focus.underWork || focus.isComplete) yield break;
        focus.underWork = true;
        
        if (focus.economicCost > nation.wealth || focus.politicalCost > nation.politicalPower) yield break;
        nation.wealth -= focus.economicCost;
        nation.politicalPower -= focus.politicalCost;

        float timer = 0;
        float step = 1f / 64f;
     
        while (timer < focus.timeCost)
        {
			focusBtn.slider.value = 1 - (timer / focus.timeCost);

			yield return new WaitForSeconds(step);
            timer += step;
		}

        focus.underWork = false;
        focus.isComplete = true;
	}
}

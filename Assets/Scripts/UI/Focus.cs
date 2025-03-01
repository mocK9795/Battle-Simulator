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
    public Reward[] rewards;
    [HideInInspector]
    public bool underWork;
    [HideInInspector]
    public bool isComplete;

    public Focus Copy()
    {
        Focus result = new Focus();
        result.name = name;
        result.image = image;
        result.requirements = new(requirements);
        result.timeCost = timeCost;
        result.underWork = underWork;
        result.isComplete = isComplete;
        return result;
    }

    public void GrantRewards(Nation nation)
    {
        foreach (var reward in rewards)
        {
            reward.ApplyReward(nation);
        }
    }
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

    public IEnumerator AttemptCompletion(Focus focus, Nation nation, FocusBtn focusBtn, System.Action<Focus, Nation> callback, ErrorDisplay display)
    {
        foreach (var otherFocus in tree) { if (otherFocus.underWork) {display.ShowError("You Can Only Complete One Focus At A Time") ; yield break; } }
        if (focus.isComplete) {display.ShowError("Focus is Complete"); yield break; }
        focus.underWork = true;

        if (focus.economicCost > nation.wealth || focus.politicalCost > nation.politicalPower) {display.ShowError("Requirements Not Met"); yield break; }
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

        callback(focus, nation);
	}
}

[System.Serializable]
public class Reward
{
    public enum RewardType
    {
        Experience
    }
    public RewardType rewardType;

    public float bonus;
    [Tooltip("Amount of units to give the experience (-1 for all)")]
    public int count;
    public void ApplyReward(Nation nation)
    {
        if (rewardType == RewardType.Experience)
        {
            var army = nation.GetArmy();
            var maxCount = count;
            if (count < 0) maxCount = army.Length;
            for (int i = 0; i < army.Length; i++)
            {
                army[i].experience += bonus;
                if (i >= maxCount - 1) break;
            }
        }


    }
}
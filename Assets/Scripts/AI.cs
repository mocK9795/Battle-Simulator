using UnityEngine;

public class AI : MonoBehaviour
{
    public Nation nation;

	public void Update()
	{
		var warriors = nation.GetArmy();
		if (warriors.Length == 2) return;
		for (int i = 1;  i < warriors.Length;  i++)
		{
			if (warriors[i].isAttacking)
			{
				warriors[i - 1].SetTarget(warriors[i].target);
			}
		}
	}
}

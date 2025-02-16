using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AI : MonoBehaviour
{
    public Nation nation;
	float timer;

	public void Update()
	{
		timer += Time.deltaTime;
		if (timer < GlobalData.aiThinkSpeed) return;
		timer = 0;

		var warriors = nation.GetArmy();
		if (warriors.Length == 2) return;
		for (int i = 1;  i < warriors.Length;  i++)
		{
			if (warriors[i].isAttacking)
			{
				warriors[i - 1].SetTarget(warriors[i].target);
			}
		}

		var capitals = nation.GetCapitals();
		capitals = capitals.OrderBy(obj => obj.controllRadius).ToArray();
		foreach (var capital in capitals)
		{
			if (!capital.underSeige) continue;
			if (capital.attacker == null) continue;
			foreach (var warrior in warriors) { warrior.SetTarget(capital.attacker.transform.position); }
		}
	}
}

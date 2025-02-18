using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RecruitmentManager : MonoBehaviour
{
	public BattleManager battle;

	public GameObject recruitmentMenu;
	public GameObject detailPreset;
	public GameObject detailsContainer;
	List<TMP_Text> details = new();

	[Header("Warrior Statistic Cost Per 1 Unit")]
	public float health;
	public float damage;
	public float speed;

	[Header("Test Data")]
	public WarriorData testData;
	[HideInInspector]
	public WarriorData currentData;

	[ContextMenu("Create Test Details")]
	public void RunTest()
	{
		ShowDetails(testData);
	}

	public void RecruitArmy(Nation nation, WarriorData data)
	{
		battle.CreateArmiesFromData(data, nation.nation);
	}

	public float GetCost(WarriorData data)
	{
		return (data.health * health + data.damage * damage + data.speed * speed) * data.count; 
	}

	public void ShowDetails(WarriorData data)
	{
		currentData = data;
		ClearDetails();

		details.Add(CreateDetail("Health " + data.health.ToString()));
		details.Add(CreateDetail("Damage " + data.damage.ToString()));
		details.Add(CreateDetail("Speed " + data.speed.ToString()));
		details.Add(CreateDetail(""));
		details.Add(CreateDetail("Count " + data.count.ToString()));
		details.Add(CreateDetail(""));
		details.Add(CreateDetail("Cost " + GetCost(data)));
	}

	public void ShowMenu(bool show)
	{
		recruitmentMenu.SetActive(show);
	}

	public TMP_Text CreateDetail(string detailInfo) {
		GameObject createdObj = Instantiate(detailPreset, detailsContainer.transform);
		var createdDetail = createdObj.GetComponent<TMP_Text>();
		createdDetail.text = detailInfo;
		return createdDetail;
	}

	[ContextMenu("Clear Details")]
	void ClearDetails()
	{
		try {
			foreach (var item in details) { DestroyImmediate(item.gameObject); }
			details.Clear();
		}
		catch (MissingReferenceException) { details = new(); throw (new System.Exception("Details Not Assigned")); }
	}
}

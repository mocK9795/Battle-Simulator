using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class RecruitmentManager : MonoBehaviour
{
	public BattleManager battle;

	public GameObject recruitmentMenu;
	public GameObject actionsContainer;
	public float actionTextSize = 80;
	public GameObject detailPreset;
	public GameObject detailsContainer;
	public Sprite AddButton;
	public Sprite SubtractButton;
	List<Component> details = new();

	[Header("Warrior Statistic Cost Per 1 Unit")]
	public float health;
	public float damage;
	public float speed;

	[Header("Test Data")]
	public WarriorData testData;
	[HideInInspector]
	public WarriorData data;

	[ContextMenu("Create Test Details")]
	public void RunTest()
	{
		ShowDetails(testData);
	}

	public WarriorData NormalizeData(WarriorData data)
	{
		data.health = Mathf.Max(data.health, 1);
		data.damage = Mathf.Max(data.damage, 1);
		data.speed = Mathf.Max(data.speed, 1);
		return data;
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
		this.data = NormalizeData(data);
		data = this.data;
		ClearDetails();

		CreateDetail("Health " + data.health.ToString());
		SetModifierAction(CreateModifiers(), AddHealth, SubtractHealth);
		CreateDetail("Damage " + data.damage.ToString());
		SetModifierAction(CreateModifiers(), AddDamage, SubtractDamage);
		CreateDetail("Speed " + data.speed.ToString());
		SetModifierAction(CreateModifiers(), AddSpeed, SubtractSpeed);
		AddEmptyRow();
		CreateDetail("Count " + data.count.ToString());
		SetModifierAction(CreateModifiers(), AddCount, SubtractCount);
		SetCost(GetCost(data));
	}

	void AddEmptyRow(int collumnCount = 3)
	{
		for (int i = 0; i < collumnCount; i++)
		{
			var obj = new GameObject("Empty Row");
			obj.transform.parent = detailsContainer.transform;
			var img = obj.AddComponent<Image>();
			img.color = recruitmentMenu.GetComponent<RawImage>().color;
			details.Add(img);
		}
	}

	public void ShowMenu(bool show)
	{
		recruitmentMenu.SetActive(show);
	}

	Pair<Button> CreateModifiers()
	{
		GameObject buttonA = new GameObject("Add");
		buttonA.transform.parent = detailsContainer.transform;
		var buttonComponentA = buttonA.AddComponent<Button>();
		var imageA = buttonA.AddComponent<Image>();
		imageA.sprite = AddButton;

		GameObject buttonB = new GameObject("Subtract");
		buttonB.transform.parent = detailsContainer.transform;
		var buttonComponentB = buttonB.AddComponent<Button>();
		var imageB = buttonB.AddComponent<Image>();
		imageB.sprite = SubtractButton;

		details.Add(buttonComponentA);
		details.Add(buttonComponentB);

		return new Pair<Button>(buttonComponentA, buttonComponentB);
	}

	void SetCost(float cost)
	{
		GameObject createdObj = Instantiate(detailPreset, actionsContainer.transform);
		var createdDetail = createdObj.GetComponent<TMP_Text>();
		createdDetail.text = "Cost " + cost.ToString();
		createdDetail.textWrappingMode = TextWrappingModes.Normal;
		createdDetail.fontSize = actionTextSize;
		details.Add(createdDetail);
	}

	public static void SetModifierAction(Pair<Button> pair, System.Action a, System.Action b)
	{
		pair.a.onClick.AddListener( () => { a(); } );
		pair.b.onClick.AddListener( () => { b(); } );
	}

	TMP_Text CreateDetail(string detailInfo) {
		GameObject createdObj = Instantiate(detailPreset, detailsContainer.transform);
		var createdDetail = createdObj.GetComponent<TMP_Text>();
		createdDetail.text = detailInfo;
		details.Add(createdDetail);
		return createdDetail;
	}

	public void SubtractDamage() { data.damage -= 1; ShowDetails(data); }
	public void AddDamage() { data.damage += 1; ShowDetails(data); }
	public void SubtractHealth() { data.health -= 1; ShowDetails(data); }
	public void AddHealth() { data.health += 1; ShowDetails(data); }
	public void SubtractSpeed() { data.speed -= 1; ShowDetails(data); }
	public void AddSpeed() { data.speed += 1; ShowDetails(data); }
	public void SubtractCount() { data.count -= 1; ShowDetails(data); }
	public void AddCount() { data.count += 1; ShowDetails(data); }

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

public struct Pair<T>
{
	public T x;
	public T y;

	public Pair(T x, T y) : this()
	{
		this.x = x;
		this.y = y;
	}

	public T a {
		get { return x; } 
		set { x = value; } 
	} 
	public T b {
	get { return y; } 
	set { y  = value; } 
	} 


}

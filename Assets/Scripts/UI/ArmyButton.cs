using TMPro;
using UnityEngine;

public class ArmyButton : MonoBehaviour
{
    public int id {get { return privateId; } set { privateId = value; idText.text = privateId.ToString(); } }
	int privateId;
    public TMP_Text idText;

	private void Start()
	{
		idText = GetComponentInChildren<TMP_Text>();
	}

	public void OnArmySelect()
    {
        FindFirstObjectByType<ArmyManagement>().OnArmyClick(id);
    }
}

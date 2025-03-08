using TMPro;
using UnityEngine;

public class ArmyButton : MonoBehaviour
{
    public int id {get { return id; } set { id = value; idText.text = id.ToString(); } }
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

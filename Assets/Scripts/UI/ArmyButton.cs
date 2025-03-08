using UnityEngine;

public class ArmyButton : MonoBehaviour
{
    public int id;

    public void OnArmySelect()
    {
        FindFirstObjectByType<ArmyManagement>().OnArmyClick(id);
    }
}

using UnityEngine;

public class Factory : WarObject
{
    [Tooltip("Amount of money a factory produces")]
    public float production;
    [Tooltip("Amount of Energy the factory uses per unit of production")]
    public float efficiency;
    float output;

	new void Update()
    {
        base.Update();
        output += production * Time.deltaTime * health * efficiency;
    }

    public float TakeOuput()
    {
        var result = output;
        output = 0;
        return result;
    }
}

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Focus
{
    public string name;
    public Sprite image;
    public List<string> requirements;
    public float politicalCost;
    public float economicCost;
    public float timeCost;
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
}

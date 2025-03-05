using JetBrains.Annotations;
using UnityEngine;

public class WarObject : MonoBehaviour
{
    [Header("War Statistics")]
    public string nation;
    public float health;
    [HideInInspector] public float maxHealth;
    public float damage;

	public enum ModelType {Car, City, Warrior, Truck, Tank, Ship, Plane, Sprite, Factory};
	[Header("Model Type")]
	public ModelType modelType;

	public enum ObjectType {Warrior, Structure}
	[HideInInspector] public ObjectType type;

	[HideInInspector]
	public Sprite sprite
	{
		get { return GetComponent<SpriteRenderer>().sprite; }
		set { GetComponent<SpriteRenderer>().sprite = value; }
	}

	[HideInInspector]
	public GameObject model
	{
		get { return modelObj; }
	}
	GameObject modelObj;

	[HideInInspector]
	public BoxCollider2D box
	{
		get { return GetComponent<BoxCollider2D>(); }
	}

	[HideInInspector]
	public CircleCollider2D sphere;

	[HideInInspector]
	public SpriteRenderer spriteRenderer
	{
		get { return GetComponent<SpriteRenderer>(); }
	}

	[HideInInspector]
	public Rigidbody2D body
	{
		get {return GetComponent<Rigidbody2D>();}
	}

	[HideInInspector]
	public Color color
	{
		set
		{
			if (spriteRenderer != null) {spriteRenderer.color =  value;}
			if (model == null) return;
			Renderer[] renderers = model.GetComponentsInChildren<MeshRenderer>();
			foreach (Renderer renderer in renderers)
			{
				renderer.material.color = value;
			}
		}
	}

	public void Start()
	{
		maxHealth = health;
		SetModel();
	}

	public void Update()
	{
		if(type == ObjectType.Warrior && health < 0 ) Destroy(gameObject);
	}

	public void Capture(string captureNation)
	{
		if (type == ObjectType.Structure && health > 0) return;
		health = GlobalData.capitalChangeHealth;
		nation = captureNation;
		SetModel();
	}

	public void SetModel(UnitModelData modelData)
	{
		if (modelData == null) { RemoveModel(); 
			if(GlobalData.battle) GlobalData.battle.SetWarObjectColor(this); 
			return; }
		if (modelObj == null)
		{
			var children = GetComponentsInChildren<Transform>();
			foreach (var child in children)
			{
				if (child == null) continue; // Somehow child value can be null
				if (child.GetComponent<WarObject>() == null)
				{
					DestroyImmediate(child.gameObject);
				}
			}
		}
		else DestroyImmediate(modelObj);
		
		modelObj = Instantiate(modelData.model);
		modelObj.transform.parent = transform;
		
		modelObj.transform.localPosition = Vector3.zero;
		modelObj.transform.rotation = Quaternion.Euler(modelData.rotation);
		modelObj.transform.localScale = modelData.scale;

		if (box != null)
		{
			box.size = modelData.box;
			box.offset = modelData.boxOffset;
		}
		if (sphere != null)
		{
			sphere.radius = modelData.box.x;
			sphere.offset = modelData.boxOffset;
		}

		spriteRenderer.enabled = false;

		if (GlobalData.battle) GlobalData.battle.SetWarObjectColor(this);
	}

	public void SetModel(ModelType modelVariant)
	{
		SetModel(GlobalData.FindModel(modelVariant));
	}

	public void SetModel()
	{
		SetModel(modelType);
	}

	void RemoveModel()
	{
		DestroyImmediate(modelObj);
		spriteRenderer.enabled = true;
	}
}

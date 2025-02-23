using UnityEngine;

public class WarObject : MonoBehaviour
{
    [Header("War Statistics")]
    public string nation;
    public float health;
    [HideInInspector] public float maxHealth;
    public float damage;

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
	public SpriteRenderer spriteRenderer
	{
		get { return GetComponent<SpriteRenderer>(); }
	}

	public void Start()
	{
		maxHealth = health;
	}

	public void Update()
	{
		if (health < 0) Destroy(gameObject);
	}

	public void SetModel(UnitModelData modelData)
	{
		if (modelData == null) { RemoveModel(); return; }
		DestroyImmediate(modelObj);
		
		modelObj = Instantiate(modelData.model);
		modelObj.name = modelData.name;
		modelObj.transform.parent = transform;
		
		modelObj.transform.localPosition = Vector3.zero;
		modelObj.transform.rotation = Quaternion.Euler(modelData.rotation);
		modelObj.transform.localScale = modelData.scale;

		box.size = modelData.box;
		box.offset = modelData.boxOffset;

		spriteRenderer.enabled = false;
	}

	void RemoveModel()
	{
		DestroyImmediate(modelObj);
		spriteRenderer.enabled = true;
	}
}

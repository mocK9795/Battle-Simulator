using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Warrior : MonoBehaviour
{
	[Header("Warrior Statistics")]
	public string nation;
	public float health;
	public float speed;
	public float damage;
	public float agility;

	public Vector2 target;
	float maxHealth;
	Vector3 maxScale;
	bool hasAttacked = false;
	Rigidbody2D body;

	Vector2 position { get { return new Vector2(transform.position.x, transform.position.y); } }
	Vector2 forward { get { return new Vector2(transform.right.x, transform.right.y); } }
	float targetAngle;
	float currentAngle;

	private void Start()
	{
		body = GetComponent<Rigidbody2D>();
		body.gravityScale = 0;
		body.linearDamping = GlobalData.friction;
		
		maxHealth = health;
		maxScale = transform.localScale;
	}

	public void Update()
	{
		//if (Mathf.RoundToInt(currentAngle / GlobalData.rotateAccuracy) == Mathf.RoundToInt(targetAngle / GlobalData.rotateAccuracy))
		//{
		//	RotateTowardsTarget();
		//	body.linearVelocity = Vector2.zero;
		//}
		UpdateTargetAngle();
		RotateTowardsTarget();
		if (!(Vector2.Distance(position, target) < GlobalData.moveAccuracy))
		{
			body.linearVelocity += speed * forward;
		}

		hasAttacked = false;

		if (health < 0)
		{
			Destroy(gameObject);
		}

		body.linearVelocity = Vector2.ClampMagnitude(body.linearVelocity, speed);
		transform.localScale = Vector3.ClampMagnitude(transform.localScale, Mathf.Max(health / GlobalData.healthToScaleRatio, GlobalData.minScale));
	}

	public void SetTargetFromOffset(Vector2 offset)
	{
		target = position + offset;
		UpdateTargetAngle();
	}

	public void UpdateTargetAngle()
	{
		float x = target.x - position.x;
		float y = target.y - position.y;
		targetAngle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
	}

	public void RotateTowardsTarget()
	{
		//currentAngle = Mathf.Lerp(transform.eulerAngles.z, targetAngle, Mathf.Clamp01(Time.deltaTime * agility));
		//transform.rotation = Quaternion.Euler(0, 0, currentAngle);
		transform.rotation = Quaternion.Euler(0, 0, targetAngle);
	}

	[ContextMenu("Print Data")]
	public void PrintData()
	{
		print(forward);
		print(position);
		print(targetAngle);
		print(currentAngle);
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (hasAttacked) return;
		if (collision.collider == null) return;
		Warrior enemy = collision.collider.GetComponent<Warrior>();
		if (enemy == null) return;
		if (enemy.nation == nation) return;

		enemy.health -= damage * Time.deltaTime;
		hasAttacked = true;
		body.linearVelocity -= speed * forward * -1;
	}
}

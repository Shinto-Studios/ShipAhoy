using Unity.Collections;
using UnityEngine;

public abstract class IShip : MonoBehaviour
{
	// ----- Essentielle variabler ----- \\

	private GameManager gameManager = null;
	[SerializeField] private Rigidbody rigid = null;

	// ----- Generelle variabler ----- \\

	[SerializeField] protected int shipID = -1;

	[SerializeField] protected int shipHealth = 100;
	[SerializeField] protected float shipFuel = 100.0f;
	[SerializeField] private int shipCannonDamage = 15;
	[SerializeField] private float shipCannonNextFire = 0.5f;

	[SerializeField] protected Cannon leftCannon = null;
	[SerializeField] protected Cannon rightCannon = null;

	protected float moveSpeed = 8.0f;
	protected float moveSpeedMax = 8.0f;
	protected float rotationSpeed = 90.0f;

	private bool networkUpdated = false;

	// ----- API funktioner ----- \\

	///<summary>Bruges til at håndtere spiller input og andre ting</summary>
	protected abstract void ProcessShip();
	
	///<summary>Bruges til at lave physics beregninger på vores skib</summary>
	protected abstract void ProcessShipFixed();
	
	///<summary>Bliver kaldt når vi rammer sammen med et andet skib</summary>
	protected abstract void OnShipCollision();

	///<summary>Bliver kaldt når vi tager skade</summary>
	protected abstract void OnDamageTaken(int damage);

	///<summary>Bliver kaldt når vores skib dør</summary>
	protected abstract void OnShipDestroyed();

	// ----- Engine funktioner ----- \\

	private void OnCollisionEnter(Collision collision)
	{
		ShipCollisionHandler(collision);
	}

	// ----- Custom funktioner ----- \\

	///<summary>Bliver kaldt når vores skib bliver oprettet</summary>
	public void ShipCreatedHandler(GameManager gm, bool physics, int ID)
	{
		gameManager = gm;
		shipID = ID;

		if(physics == false)
        {
			Destroy(rigid);
		}
	}

	///<summary>Bliver kaldt hver frame</summary>
	public void OnUpdate()
	{
		leftCannon.OnUpdate();
		rightCannon.OnUpdate();

		ProcessShip();

		transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
	}

	///<summary>Bliver kaldt consistently hver physics update</summary>
	public void OnFixedUpdate()
	{
		ProcessShipFixed();

		Utils.ApplyRigidSettings(rigid, moveSpeedMax);
	}

	///<summary>Bliver kaldt når vi rammer sammen med et andet objekt</summary>
	private void ShipCollisionHandler(Collision collision)
    {
		OnShipCollision();

		IShip ship = collision.gameObject.GetComponent<IShip>();

		if (ship != null)
		{
			ship.ApplyDamage(10 * (int)rigid.velocity.magnitude);
		}
	}

	///<summary>Bevæger skibet mod en bestemt retning</summary>
	protected void MoveShip(Vector3 direction)
	{
		rigid.AddForce(direction);
	}

	///<summary>Giver skader til skibet</summary>
	public void ApplyDamage(int damage)
	{
		shipHealth -= damage;

		GetGameManager().GetUIManager().ShowDamageText(gameObject, damage, 2);

		OnDamageTaken(damage);

		if (shipHealth <= 0)
		{
			GetGameManager().GetUIManager().ShowDamageText(gameObject, damage, 2);
			GetGameManager().GetUIManager().GetHealthBar().SetHealth(shipHealth);

			OnDamageTaken(damage);

			OnShipDestroyed();

			gameManager.OnShipDestroyed(this);
		}
	}

	public void SetShipHealth(int health)
    {
		shipHealth = health;
    }

	public int GetShipHealth()
	{
		return shipHealth;
	}

	public void SetShipFuel(float fuel)
	{
		shipFuel = fuel;
	}

	public float GetShipFuel()
	{
		return shipFuel;
	}

	///<summary>Skaffer skibets hastighed</summary>
	public float GetShipSpeed()
    {
		return rigid.velocity.magnitude;
	}

	///<summary>Rotere skibet rundt baseret på hastigheden angivet</summary>
	public void RotateShip(float speed)
	{
		transform.Rotate(Vector3.up, speed * (GetShipSpeed() / moveSpeedMax) * Time.deltaTime);
	}

	///<summary>Rotere venstre kanon rundt baseret på hastigheden angivet</summary>
	public void RotateLeftCannon(float speed)
	{
		leftCannon.GetCannonMotor().transform.Rotate(Vector3.up, speed * Time.deltaTime);

		float newAngle = Utils.ClampAngle(leftCannon.GetCannonMotor().transform.localEulerAngles.y, -220, -140);

		leftCannon.GetCannonMotor().transform.localEulerAngles = new Vector3(leftCannon.GetCannonMotor().transform.localEulerAngles.x, newAngle, leftCannon.GetCannonMotor().transform.localEulerAngles.z);
	}

	///<summary>Rotere højre kanon rundt baseret på hastigheden angivet</summary>
	public void RotateRightCannon(float speed)
    {
		rightCannon.GetCannonMotor().transform.Rotate(Vector3.up, speed * Time.deltaTime);

		float newAngle = Utils.ClampAngle(rightCannon.GetCannonMotor().transform.localEulerAngles.y, -40, 40);

		rightCannon.GetCannonMotor().transform.localEulerAngles = new Vector3(rightCannon.GetCannonMotor().transform.localEulerAngles.x, newAngle, rightCannon.GetCannonMotor().transform.localEulerAngles.z);
	}

	///<summary>Skaffer den skade kanonerne gør på skibe</summary>
	public int GetCannonDamage()
	{
		return shipCannonDamage;
	}

	///<summary>Skaffer den tid det tager at gøre kanonerne klar igen</summary>
	public float GetCannonNextFire()
	{
		return shipCannonNextFire;
	}

	public int GetShipID()
    {
		return shipID;
    }

	public GameManager GetGameManager()
    {
		return gameManager;
    }

	public void UpdateNetworkState()
	{
		networkUpdated = true;
	}

	public void ResetNetworkState()
    {
		networkUpdated = false;
    }

	public bool GetNetworkState()
	{
		return networkUpdated;
	}
}
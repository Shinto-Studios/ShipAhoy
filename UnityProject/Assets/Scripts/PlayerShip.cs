using UnityEngine;

public class PlayerShip : IShip
{
	// ----- API funktioner ----- \\

	protected override void ProcessShip()
    {
		if (Input.GetKey(KeyCode.A))
		{
			RotateShip(-rotationSpeed);
		}
		else if (Input.GetKey(KeyCode.D))
		{
			RotateShip(rotationSpeed);
		}

		if(Input.GetButton("Fire1"))
        {
			leftCannon.ShootCannon();
        }

		if (Input.GetButton("Fire2"))
		{
			rightCannon.ShootCannon();
		}

		GetGameManager().GetUIManager().GetCompass().transform.eulerAngles = new Vector3(GetGameManager().GetUIManager().GetCompass().transform.eulerAngles.x, GetGameManager().GetUIManager().GetCompass().transform.eulerAngles.y, transform.eulerAngles.y);
		GetGameManager().GetUIManager().GetFuelBar().SetFuel((int)shipFuel);
		GetGameManager().GetUIManager().GetHealthBar().SetHealth(shipHealth);
	}

	protected override void ProcessShipFixed()
    {
		if (Input.GetKey(KeyCode.W))
		{
			MoveShip(transform.forward * moveSpeed);
		}
		else if (Input.GetKey(KeyCode.S))
		{
			MoveShip(-transform.forward * moveSpeed);
		}
	}

	protected override void OnShipCollision()
    {
		
	}

	protected override void OnDamageTaken(int damage)
    {
        
    }

	protected override void OnShipDestroyed()
    {
        
    }
}

using UnityEngine;

public class EnemyShip : IShip
{
    // ----- Generelle variabler ----- \\

    [SerializeField] private RectTransform healthBarRect = null;
    [SerializeField] private Healthbar healthBar = null;

    private bool targetLockon = false;

    // ----- API funktioner ----- \\

    protected override void ProcessShip()
    {
        if (GetGameManager().GetPlayerShip() != null)
        {
            float distance = Vector3.Distance(transform.position, GetGameManager().GetPlayerShip().transform.position);

            if (targetLockon == true)
            {
                if (distance > 64.0f)
                {
                    targetLockon = false;
                }
                else
                {
                    Vector3 relativePoint = transform.InverseTransformPoint(GetGameManager().GetPlayerShip().transform.position).normalized;
                    float dotPoint = Vector3.Dot((GetGameManager().GetPlayerShip().transform.position - transform.position).normalized, transform.position);

                    if (dotPoint >= 0.0f)
                    {
                        if (relativePoint.x < 0.0f)
                        {
                            RotateShip(-45.0f);
                        }
                        else if (relativePoint.x >= 0.0f)
                        {
                            RotateShip(45.0f);

                            if (relativePoint.z >= -0.5f && relativePoint.z <= 0.5f)
                            {
                                rightCannon.ShootCannon();
                            }
                        }
                    }
                    else if (dotPoint < 0.0f)
                    {
                        if (relativePoint.x >= 0.0f)
                        {
                            RotateShip(45.0f);
                        }
                        else if (relativePoint.x < 0.0f)
                        {
                            RotateShip(-45.0f);

                            if (relativePoint.z >= -0.5f && relativePoint.z <= 0.5f)
                            {
                                leftCannon.ShootCannon();
                            }
                        }
                    }
                }
            }
            else
            {
                if (distance < 24.0f)
                {
                    targetLockon = true;
                }
            }
        }
        else
        {
            targetLockon = false;
        }

        healthBar.SetHealth(shipHealth);
        healthBarRect.eulerAngles = new Vector3(90, 0, 0);
    }

    protected override void ProcessShipFixed()
    {
        if(targetLockon == true)
        {
            MoveShip(transform.forward * moveSpeed);
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

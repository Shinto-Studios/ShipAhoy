using UnityEngine;

public class Utils
{
    // ----- Essentielle variabler ----- \\

    const string glyphs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    // ----- API funktioner ----- \\

    ///<summary>Bliver brugt til og enforce vores maximum hastighed</summary>
	public static void ApplyRigidSettings(Rigidbody rigid, float maxSpeed)
    {
        float xNew = Mathf.Lerp(Mathf.Clamp(rigid.velocity.x, -maxSpeed, maxSpeed), 0, Time.fixedDeltaTime);
        float yNew = Mathf.Lerp(Mathf.Clamp(rigid.velocity.y, -maxSpeed, maxSpeed), 0, Time.fixedDeltaTime);
        float zNew = Mathf.Lerp(Mathf.Clamp(rigid.velocity.z, -maxSpeed, maxSpeed), 0, Time.fixedDeltaTime);

        rigid.velocity = new Vector3(xNew, yNew, zNew);
    }

    ///<summary>Hjælpefunktion til at holde vores rotation inden for en given value</summary>
    public static float ClampAngle(float angle, float min, float max)
    {
        angle = Mathf.Repeat(angle, 360);
        min = Mathf.Repeat(min, 360);
        max = Mathf.Repeat(max, 360);

        bool inverse = false;

        float tmin = min;
        float tangle = angle;

        if (min > 180)
        {
            inverse = !inverse;
            tmin -= 180;
        }

        if (angle > 180)
        {
            inverse = !inverse;
            tangle -= 180;
        }

        bool result = !inverse ? tangle > tmin : tangle < tmin;

        if (!result)
        {
            angle = min;
        }

        inverse = false;
        tangle = angle;

        float tmax = max;

        if (angle > 180)
        {
            inverse = !inverse;
            tangle -= 180;
        }

        if (max > 180)
        {
            inverse = !inverse;
            tmax -= 180;
        }

        result = !inverse ? tangle < tmax : tangle > tmax;

        if (!result)
        {
            angle = max;
        }

        return angle;
    }

    public static string RandomString(int length)
    {
        string randomString = string.Empty;

        for (int i = 0; i < length; i++)
        {
            randomString += glyphs[Random.Range(0, glyphs.Length)];
        }

        return randomString;
    }
}

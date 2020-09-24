using UnityEngine;

public class DamageUI : MonoBehaviour
{
    // ----- Essentielle variabler ----- \\

    private Camera cam;
    private Transform lookAt = null;

    [SerializeField] private Vector3 offset = new Vector3();

    // ----- Engine funktioner ----- \\

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if(lookAt != null)
        {
            Vector3 pos = cam.WorldToScreenPoint(lookAt.position + offset);

            if (transform.position != pos)
            {
                transform.position = pos;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----- API funktioner ----- \\

    public void SetFollowTransform(Transform transf)
    {
        if(lookAt == null)
        {
            lookAt = transf;
        }
    }
}

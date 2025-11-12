using UnityEngine;

public class Knifes : MonoBehaviour
{
    [Header("Knife Attributes")]
    public float speed;
    public GameObject knife;
    private Rigidbody knifeRigid;

    public Transform hookTransform;
    public Transform player;

    private bool throwing = false;
    private bool canThrow = true;
    private Vector3 throwDirection;


    public void Start()
    {
        knifeRigid = knife.GetComponent<Rigidbody>();
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0) && canThrow)
        {
            canThrow = false;
            Throw();
        }

        if (throwing)
        {
            knifeRigid.constraints = RigidbodyConstraints.None;
            knifeRigid.velocity = throwDirection * speed;
        }
    }

    public void StopThrowing()
    {
        throwing = false;
        knifeRigid.velocity = Vector3.zero;
        knifeRigid.constraints = RigidbodyConstraints.FreezePosition;
    }

    public bool CanThrowKnife()
    {
        return canThrow;
    }

    public void TeleportToKnife()
    {
        Vector3 teleportPos = knife.transform.position - throwDirection.normalized * 2;
        player.transform.position = teleportPos;    
        knife.GetComponent<ThrowingKnife>().Holding();
        knife.transform.parent = this.transform;     
        knife.transform.localPosition = new Vector3(0.543f, -0.12f, -0.04400015f);
        knife.transform.localRotation = Quaternion.Euler(new Vector3(-15, 90, -11.57f));
        knife.transform.localScale = new Vector3(5, -5, 5);
        canThrow = true;
    }

    private void Throw()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane p = new Plane(Vector3.up, player.position);

        if (p.Raycast(screenRay, out float hitDist))
        {
            Vector3 myPoint = new Vector3(screenRay.GetPoint(hitDist).x, hookTransform.position.y, screenRay.GetPoint(hitDist).z);
            throwDirection = -(knife.transform.position - myPoint).normalized;
            knife.transform.parent = null;
            throwing = true;
        }
    }
}

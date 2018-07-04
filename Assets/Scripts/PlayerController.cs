using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed = 0.5f;

    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero;

    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Rigidbody rb;

    private Vector2 inputAxes;
    private bool mouse0Down;

    private Vector3 lastPos;
    private Vector3 playerVel;

    private bool primaryFireOnCooldown;

    private void OnMouseEnter()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }

    private void Awake()
    {
        primaryFireOnCooldown = false;
        rb = GetComponent<Rigidbody>();

        lastPos = transform.position;
    }

    private void Update()
    {
        handleUserInput();

        if (mouse0Down && !primaryFireOnCooldown)
        { // cast ray to fire weapon
            Ray ray = cameraController.cam.ScreenPointToRay(Input.mousePosition);
            StartCoroutine(doPrimaryFire(ray));
            StartCoroutine(doPrimaryFireCooldown(0.2f));
        }
    }

    private void FixedUpdate()
    {
        playerVel = transform.position - lastPos;
        lastPos = transform.position;

        Vector3 movement = moveSpeed * Time.deltaTime * new Vector3(inputAxes.x, 0, inputAxes.y);
        transform.Translate(movement);

    }

    private void handleUserInput()
    {
        inputAxes = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        mouse0Down = Input.GetKeyDown(KeyCode.Mouse0);
    }

    IEnumerator doPrimaryFire(Ray ray)
    {
        RaycastHit hit;
        int layerMask = 1 << 9; // playerRaycast layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("RayCast hit in PlayerRaycast LayerMask");
            Vector3 targetPos = new Vector3(hit.point.x, 0.25f, hit.point.z);
            var projectile = GameObject.Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
            Vector3 inheritedVelocity = playerVel; // projectile inherits player's velocity
            Vector3 displacement = targetPos - transform.position;
            float distance = displacement.magnitude;
            //* okay, this one kind of works, but it buggy. sometimes the player rigidbody's velocity gets set to something weird
            int nSteps = Mathf.CeilToInt(distance / (projectile.moveSpeed * Time.fixedDeltaTime));
            for (int t = 0; t < nSteps; t++)
            {
                projectile.transform.Translate(displacement.normalized * projectile.moveSpeed * Time.fixedDeltaTime);
                projectile.transform.Translate(inheritedVelocity * 0.5f); // only inherit half the player's velocity
                yield return new WaitForFixedUpdate();
            }
            //*/

            /* I thought this would work more simply, but for some reason, it doesn't.
            projectile.rb.velocity = displacement.normalized * projectile.moveSpeed * Time.fixedDeltaTime;
            yield return new WaitForSeconds(projectile.lifeTime);
            //*/

            // add explosion
            var explosion = GameObject.Instantiate(explosionPrefab, targetPos, Quaternion.identity);
            // delete projectile
            GameObject.Destroy(projectile.gameObject);

            // add explosion force to nearby objects tagged with "Explodable"
            Vector3 explosionCenter = targetPos;
            float explosionRadius = explosion.GetComponent<SphereCollider>().radius;
            float explosionForce = 250.0f;
            Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius);
            foreach(Collider hitCollider in colliders)
            {
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, 5.0f);
                }
            }
            yield return new WaitForSeconds(0.1f);
            GameObject.Destroy(explosion);
        }
        yield return null;   
    }

    IEnumerator doPrimaryFireCooldown(float seconds)
    {
        primaryFireOnCooldown = true;
        yield return new WaitForSeconds(seconds);
        primaryFireOnCooldown = false;
        yield return null;
    }


}

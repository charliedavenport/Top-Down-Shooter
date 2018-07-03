using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Camera cam { get; private set; }

    public float followSpeed = 0.5f;

    [SerializeField] private Transform target;

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, followSpeed);
    }

}

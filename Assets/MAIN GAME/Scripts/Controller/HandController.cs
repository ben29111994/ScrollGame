using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    private static HandController instance;
    public static HandController Instance { get { return instance; } }

    public GameObject handObject;
    public LayerMask layerHand;

    private void Awake()
    {
        instance = (instance == null) ? this : instance;
    }

    public void Update()
    {
        SetHandPosition(Vector3.zero, false);
        if (!Input.GetMouseButton(0) || !GameController.Instance.isPlaying) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit, 100.0f, layerHand)) SetHandPosition(hit.point, true);
    }

    public void SetHandPosition(Vector3 pos,bool isActive)
    {
        handObject.transform.position = pos;
        handObject.SetActive(isActive);
    }
}

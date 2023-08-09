using MegaFiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.SceneManagement;
using System.Net;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ScrollControl : MonoBehaviour
{
    public MegaBend bend;
    public float fullScrollLengthOffset;
    float baseOffset;
    bool isReleasing;
    public bool isReleased;
    bool isTouchWall;
    public LayerMask layerMask;
    // Start is called before the first frame update

    void Start()
    {
        baseOffset = bend.Offset.x;
    }

    public void ScrollRelease(float moveY)
    {
        Debug.LogError("Release");
        DOTween.KillAll();
        transform.DOLocalMoveY(0.05f + moveY, 0);
        isReleasing = true;
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, fullScrollLengthOffset, 0.5f).OnComplete(() => { 
            //Destroy(GetComponent<MeshCollider>()); 
            //var addCollider = gameObject.AddComponent<MeshCollider>();
            //addCollider.convex = true;
            //addCollider.isTrigger = true;
            isReleasing = false;
            isReleased = true;
            if(isTouchWall)
            {
                ScrollReap();
                isTouchWall = false;
            }
        });      
    }

    public void ScrollReap()
    {
        Debug.LogError("Reap");
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => { 
            transform.DOLocalMoveY(0.05f, 0);
            //Destroy(GetComponent<MeshCollider>());
            //var addCollider = gameObject.AddComponent<MeshCollider>();
            //addCollider.convex = true;
            //addCollider.isTrigger = true;
            isReleased = false;
        });
    }

    public void ScrollMove()
    {
        Debug.LogError("Move");
        DOTween.KillAll();
        Vector3 targetHitPos = Vector3.zero;
        var parentControl = transform.parent.transform;
        RaycastHit hit;
        if (Physics.Raycast(parentControl.position, transform.TransformDirection(Vector3.left), out hit, Mathf.Infinity, layerMask))
        {
            if (hit.transform.CompareTag("Wall"))
            {
                targetHitPos = hit.transform.localPosition;
            }
            if(hit.transform.CompareTag("Scroll"))
            {
                targetHitPos = hit.transform.parent.transform.localPosition;
            }
        }
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset + 10, 0.5f).OnComplete(() => {
            isReleased = false;
        });
        if (parentControl.localEulerAngles.y == 0 || parentControl.localEulerAngles.y == 180)
        {
            if(targetHitPos.x < 0)
            {
                parentControl.DOLocalMoveX(targetHitPos.x + 1, 0.5f)
                    .OnComplete(() => {
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
            else
            {
                parentControl.DOLocalMoveX(targetHitPos.x - 1, 0.5f)
                    .OnComplete(() => {
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
        }
        else
        {
            if (targetHitPos.z < 0)
            {
                parentControl.DOLocalMoveZ(targetHitPos.z + 1, 0.5f)
                    .OnComplete(() => {
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
            else
            {
                parentControl.DOLocalMoveZ(targetHitPos.z - 1, 0.5f)
                    .OnComplete(() => {
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DOTween.KillAll();
            DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, 5, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            DOTween.KillAll();
            DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, -10, 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Scroll"))
        {
            if(isReleasing)
            {
                DOTween.KillAll();
                ScrollReap();
            }
        }
        if(other.CompareTag("Wall"))
        {
            if (isReleasing)
            {
                isTouchWall = true;       
            }
        }
    }
}

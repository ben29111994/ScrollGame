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
    bool isMoving;
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
            if (isTouchWall)
            {
                ScrollReap();
                isTouchWall = false;
            }
        });      
    }

    public void ScrollReap()
    {
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
        DOTween.KillAll();
        Vector3 targetHitPos = Vector3.zero;
        var parentControl = transform.parent.transform;
        RaycastHit hit;
        Vector3 dir = Vector3.left;
        //if (transform.localScale.x < 0)
        //    dir = -Vector3.left;
        //gameObject.layer = default;
        if (Physics.Raycast(parentControl.position, parentControl.transform.TransformDirection(dir), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(parentControl.position, parentControl.transform.TransformDirection(dir), Color.red, 1);
            if (hit.transform.CompareTag("Wall"))
            {
                Debug.LogError("RayWall");
                targetHitPos = hit.transform.parent.localPosition;
            }
            if(hit.transform.CompareTag("Scroll"))
            {
                Debug.LogError("RayScroll " + hit.transform.name);
                targetHitPos = hit.transform.parent.localPosition;
            }
        }
        //gameObject.layer = layerMask;
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset + 10, 0.5f).OnComplete(() => {
            isReleased = false;
        });
        isMoving = true;
        if (parentControl.localEulerAngles.y == 0 || parentControl.localEulerAngles.y == 180)
        {
            if(targetHitPos.x < parentControl.transform.localPosition.x)
            {
                parentControl.DOLocalMoveX(targetHitPos.x + 1, 0.5f)
                    .OnComplete(() => {
                        Debug.LogError(targetHitPos.x);
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
            else
            {
                parentControl.DOLocalMoveX(targetHitPos.x - 1, 0.5f)
                    .OnComplete(() => {
                        Debug.LogError(targetHitPos.x);
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
        }
        else
        {
            if (targetHitPos.z < parentControl.transform.localPosition.z)
            {
                parentControl.DOLocalMoveZ(targetHitPos.z + 1, 0.5f)
                    .OnComplete(() => {
                        Debug.LogError(targetHitPos.z);
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
            else
            {
                parentControl.DOLocalMoveZ(targetHitPos.z - 1, 0.5f)
                    .OnComplete(() => {
                        Debug.LogError(targetHitPos.z);
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                    }); ;
            }
        }
    }

    List<GameObject> listScrollHit;
    List<GameObject> listWallHit;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Scroll"))
        {
            //listScrollHit.Add(other.gameObject);
            Debug.LogError("Scroll");
            if (isReleasing && !other.GetComponent<ScrollControl>().isReleased)
            {
                DOTween.KillAll();
                ScrollReap();
            }
        }
        if(other.CompareTag("Wall"))
        {
            //listWallHit.Add(other.gameObject);
            if (isReleasing)
            {
                isTouchWall = true;
                Debug.LogError("WallRelease");
            }
            else if (isMoving)
            {
                //Debug.LogError("WallReap");
                //DOTween.KillAll();
                ////ScrollReap();
                //var targetHitPos = other.transform.parent.localPosition;
                //var parentControl = transform.parent.transform;
                //if (parentControl.localEulerAngles.y == 0 || parentControl.localEulerAngles.y == 180)
                //{
                //    if (targetHitPos.x < 0)
                //    {
                //        parentControl.DOLocalMoveX(targetHitPos.x + 1, 0.5f)
                //            .OnComplete(() =>
                //            {
                //                isMoving = false;
                //                DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                //            }); ;
                //    }
                //    else
                //    {
                //        parentControl.DOLocalMoveX(targetHitPos.x - 1, 0.5f)
                //            .OnComplete(() =>
                //            {
                //                isMoving = false;
                //                DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                //            }); ;
                //    }
                //}
                //else
                //{
                //    if (targetHitPos.z < 0)
                //    {
                //        parentControl.DOLocalMoveZ(targetHitPos.z + 1, 0.5f)
                //            .OnComplete(() =>
                //            {
                //                isMoving = false;
                //                DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                //            }); ;
                //    }
                //    else
                //    {
                //        parentControl.DOLocalMoveZ(targetHitPos.z - 1, 0.5f)
                //            .OnComplete(() =>
                //            {
                //                isMoving = false;
                //                DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f);
                //            }); ;
                //    }
                //}
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Scroll"))
        //{
        //    listScrollHit.Remove(other.gameObject);
        //}
        //if (other.CompareTag("Wall"))
        //{
        //    listWallHit.Remove(other.gameObject);
        //}
    }
}

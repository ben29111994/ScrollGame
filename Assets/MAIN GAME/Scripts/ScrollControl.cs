using MegaFiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.SceneManagement;
using System.Net;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Linq;

public class ScrollControl : MonoBehaviour
{
    public MegaBend bend;
    public MegaMeshPage page;
    public float fullScrollLengthOffset;
    float baseOffset;
    bool isReleasing;
    bool isMoving;
    bool isIncreaseHeight;
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
            isReleasing = false;
            isReleased = true;
            if (isTouchWall)
            {
                ScrollReap();
                isTouchWall = false;
            }
            //else
            //{
            //    page.Height += 0.03f;
            //    page.Rebuild();
            //}
        });      
    }

    public void ScrollReap()
    {
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => { 
            transform.DOLocalMoveY(0.05f, 0);
            //if (!isIncreaseHeight)
            //    page.Height = 0.2f;
            //else
            //{
            //    page.Height += 0.03f;
            //    isIncreaseHeight = false;
            //}
            //page.Rebuild();
            isReleased = false;
        });
    }

    public void ScrollReapRevert()
    {
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => {
            transform.DOLocalMoveY(0.05f, 0);
            isReleased = false;
            var scroll = transform.parent;
            float value = 0;
            if (scroll.localEulerAngles.y == 0 || scroll.localEulerAngles.y == 180)
            {
                if (transform.localScale.x < 0)
                    value = scroll.transform.localPosition.x - int.Parse(name) + 1;
                else
                    value = scroll.transform.localPosition.x + int.Parse(name) - 1;
                transform.parent = null;
                scroll.transform.localPosition = new Vector3(value, scroll.transform.localPosition.y, scroll.transform.localPosition.z);
            }
            else
            {
                if (transform.localScale.x > 0)
                    value = scroll.transform.localPosition.z - int.Parse(name) + 1;
                else
                    value = scroll.transform.localPosition.z + int.Parse(name) - 1;
                transform.parent = null;
                scroll.transform.localPosition = new Vector3(scroll.transform.localPosition.x, scroll.transform.localPosition.y, value);
            }
            transform.parent = scroll.transform;
        });
    }

    public void ScrollMove()
    {     
        Vector3 targetHitPos = Vector3.zero;
        var parentControl = transform.parent.transform;
        RaycastHit hit;
        Vector3 dir = Vector3.left;
        if(transform.localScale.x < 0)
        {
            dir = -Vector3.left;
        }
        RaycastHit[] HitObjects = Physics.RaycastAll(parentControl.position, parentControl.transform.TransformDirection(dir), Mathf.Infinity, layerMask);
        if (HitObjects.Length > 0)
        {
            Debug.DrawRay(parentControl.position, parentControl.transform.TransformDirection(dir), Color.red, 1);
            for (int i = HitObjects.Length - 1; i >= 0; i--)
            {
                if (HitObjects[i].transform.CompareTag("Wall"))
                {
                    Debug.LogError("RayWall");
                    targetHitPos = HitObjects[i].transform.parent.localPosition;
                    break;
                }
                if (HitObjects[i].transform.CompareTag("Scroll"))
                {
                    Debug.LogError("RayScroll " + HitObjects[i].transform.name);
                    if (HitObjects[i].transform.GetComponent<ScrollControl>().isReleased)
                    {
                        isIncreaseHeight = true;
                        continue;
                    }
                    targetHitPos = HitObjects[i].transform.parent.localPosition;
                    break;
                }
            }
        }
        DOTween.KillAll();
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
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                        });
                    });
            }
            else
            {
                parentControl.DOLocalMoveX(targetHitPos.x - 1, 0.5f)
                    .OnComplete(() => {
                        Debug.LogError(targetHitPos.x);
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                        }); 
                    });
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
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                        });
                    }); 
            }
            else
            {
                parentControl.DOLocalMoveZ(targetHitPos.z - 1, 0.5f)
                    .OnComplete(() => {
                        Debug.LogError(targetHitPos.z);
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                        });
                    }); 
            }
        }
    }

    List<GameObject> listScrollHit;
    List<GameObject> listWallHit;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Scroll"))
        {
            if (isReleasing && !other.GetComponent<ScrollControl>().isReleased)
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
                Debug.LogError("WallRelease");
            }
            else if (isMoving)
            {

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }
}

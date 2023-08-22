using MegaFiers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Net;
using System.Linq;
using UnityEngine.SceneManagement;

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
    bool isTouchScroll;
    public LayerMask layerMask;
    // Start is called before the first frame update

    void Start()
    {
        baseOffset = bend.Offset.x;
    }

    public void ScrollRelease(float moveY)
    {
        GameController.isControl = false;
        //DOTween.KillAll();
        transform.DOLocalMoveY(0.2f + moveY, 0);
        isReleasing = true;
        //IncreaseShaderLayer();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, fullScrollLengthOffset, 0.5f).OnComplete(() =>
        {
            isReleasing = false;
            isReleased = true;
            if (isTouchWall)
            {
                ScrollReap();
                isTouchWall = false;
            }
            else
            {
                var height = (GameController.Instance.listReleaseScroll.Count - 1) * 0.002f;
                transform.DOLocalMoveY(height, 0);
                GameController.isControl = true;
                GameController.isDrag = false;
                //GameController.Instance.RefreshHeight();
            }
            if (LevelGenerator.Instance.CheckWin())
            {
                GameController.isControl = false;
                DataManager.Instance.Task++;
                if (DataManager.Instance.Task > 2)
                {
                    DataManager.Instance.Task = 0;
                    DataManager.Instance.LevelGame++;
                    GameController.Instance.Win();
                }
                else
                {
                    StartCoroutine(delayNextTask());
                }
            }
            //else
            //{
            //    page.Height += 0.03f;
            //    page.Rebuild();
            //}
        });
    }

    IEnumerator delayNextTask()
    {
        yield return new WaitForSeconds(2);
        GameController.Instance.rateText.gameObject.SetActive(false);
        LevelGenerator.Instance.NextTask();
    }

    public void ScrollReap()
    {
        GameController.isControl = false;
        //DOTween.KillAll();
        //IncreaseShaderLayer();
        if(isTouchScroll)
        {
            GameController.isControl = true;
            GameController.isDrag = false;
            return;
        }
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => { 
            transform.DOLocalMoveY(0.2f, 0);
            //if (!isIncreaseHeight)
            //    page.Height = 0.2f;
            //else
            //{
            //    page.Height += 0.03f;
            //    isIncreaseHeight = false;
            //}
            //page.Rebuild();
            isReleased = false;
            GameController.Instance.listReleaseScroll.Remove(transform);
            GameController.Instance.RefreshHeight();
            GameController.isControl = true;
            GameController.isDrag = false;
        });
    }

    public void ScrollReapRevert()
    {
        GameController.isControl = false;
        //DOTween.KillAll();
        if (isTouchScroll)
        {
            GameController.isControl = true;
            GameController.isDrag = false;
            return;
        }
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.5f).OnComplete(() => {
            transform.DOLocalMoveY(0.2f, 0);
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
            GameController.Instance.listReleaseScroll.Remove(transform);
            GameController.Instance.RefreshHeight();
            GameController.isControl = true;
            GameController.isDrag = false;
        });
    }

    public void ScrollMove()
    {
        GameController.isControl = false;
        //IncreaseShaderLayer();
        Vector3 targetHitPos = Vector3.zero;
        var parentControl = transform.parent.transform;
        //RaycastHit hit;
        Vector3 dir = Vector3.left;
        if(transform.localScale.x < 0)
        {
            dir = -Vector3.left;
        }
        RaycastHit[] HitObjects = Physics.RaycastAll(parentControl.position, parentControl.transform.TransformDirection(dir), Mathf.Infinity, layerMask);
        if (HitObjects.Length > 0)
        {
            Debug.DrawRay(parentControl.position, parentControl.transform.TransformDirection(dir), Color.red, 1);
            float distance = 9999;
            for (int i = HitObjects.Length - 1; i >= 0; i--)
            {
                if (HitObjects[i].transform.CompareTag("Wall"))
                {
                    if (Vector3.Distance(HitObjects[i].transform.parent.localPosition, transform.parent.localPosition) < distance)
                    {
                        targetHitPos = HitObjects[i].transform.parent.localPosition;
                        distance = Vector3.Distance(HitObjects[i].transform.parent.localPosition, transform.parent.localPosition);
                    }
                    //break;
                }
                if (HitObjects[i].transform.CompareTag("Scroll"))
                {
                    if (HitObjects[i].transform.GetComponent<ScrollControl>().isReleased)
                    {
                        //isIncreaseHeight = true;
                        continue;
                    }
                    
                    if(Vector3.Distance(HitObjects[i].transform.parent.localPosition, transform.parent.localPosition) < distance)
                    {
                        targetHitPos = HitObjects[i].transform.parent.localPosition;
                        distance = Vector3.Distance(HitObjects[i].transform.parent.localPosition, transform.parent.localPosition);
                    }
                    //break;
                }
            }
        }
        DOTween.KillAll();
        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset + 3, 0.5f).OnComplete(() => {
            isReleased = false;
        });
        isMoving = true;
        if (parentControl.localEulerAngles.y == 0 || parentControl.localEulerAngles.y == 180)
        {
            if(targetHitPos.x < parentControl.transform.localPosition.x)
            {
                parentControl.DOLocalMoveX(targetHitPos.x + 1, 0.5f)
                    .OnComplete(() => {
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.25f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                            transform.DOLocalMoveY(0.2f, 0);
                            GameController.isControl = true;
                            GameController.isDrag = false;
                        });
                    });
            }
            else
            {
                parentControl.DOLocalMoveX(targetHitPos.x - 1, 0.5f)
                    .OnComplete(() => {
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.25f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                            transform.DOLocalMoveY(0.2f, 0);
                            GameController.isControl = true;
                            GameController.isDrag = false;
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
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.25f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                            transform.DOLocalMoveY(0.2f, 0);
                            GameController.isControl = true;
                            GameController.isDrag = false;
                        });
                    }); 
            }
            else
            {
                parentControl.DOLocalMoveZ(targetHitPos.z - 1, 0.5f)
                    .OnComplete(() => {
                        isMoving = false;
                        DOTween.To(() => bend.Offset.x, x => bend.Offset.x = x, baseOffset, 0.25f).OnComplete(() => {
                            //if (!isIncreaseHeight)
                            //    page.Height = 0.2f;
                            //else
                            //{
                            //    page.Height += 0.03f;
                            //    isIncreaseHeight = false;
                            //}
                            //page.Rebuild();
                            transform.DOLocalMoveY(0.2f, 0);
                            GameController.isControl = true;
                            GameController.isDrag = false;
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
                //DOTween.KillAll();
                ScrollReap();               
            }
            if(isReleased)
            {
                isTouchScroll = true;
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

    //public void IncreaseShaderLayer()
    //{
    //    var shaders = GetComponent<Renderer>().materials;
    //    foreach(var item in shaders)
    //    {
    //        item.renderQueue--;
    //    }
    //}
}

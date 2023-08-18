using UnityEngine;
using GPUInstancer;
using DG.Tweening;
using System.Linq;

public class Tile : MonoBehaviour
{
    public Color tileColor;
    public Renderer meshRenderer;
    public bool isCheck = false;
    public bool isMagnet = false;
    public bool isHole = false;
    public int ballLevel;
    GameController gameController;
    Rigidbody rigid;

    private void OnEnable()
    {
        Init();
        gameController = GameObject.FindGameObjectWithTag("Player").GetComponent<GameController>();
        rigid = GetComponent<Rigidbody>();
    }

    public void Init()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();
    }

    public void SetTransfrom(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public void SetColor(Color inputColor)
    {
        tileColor = inputColor;
        if (meshRenderer.materials.Length > 1)
        {
            foreach (var mat in meshRenderer.materials)
            {
                mat.color = tileColor;
            }
        }
        else
            meshRenderer.material.color = tileColor;
        //tag = "Pixel";
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if(collision.gameObject.CompareTag("Wall"))
    //    {
    //        //Debug.Log("Hit");
    //        if (transform.childCount > 0)
    //        {
    //            isCheck = true;
    //            isMagnet = false;
    //            transform.parent = null;
    //            gameController.pixels.Remove(rigid);
    //            var prefab = PoolManager.instance.GetObject(PoolManager.NameObject.bullet);
    //            if (prefab != null)
    //            {
    //                prefab.SetActive(true);
    //                prefab.transform.position = collision.gameObject.transform.position;
    //                prefab.GetComponent<ParticleSystem>().Play();
    //            }

    //            prefab = PoolManager.instance.GetObject(PoolManager.NameObject.pixelExplode);
    //            if (prefab != null)
    //            {
    //                prefab.SetActive(true);
    //                prefab.transform.position = transform.position;
    //                prefab.GetComponent<ParticleSystem>().Play();
    //            }
    //            Destroy(collision.gameObject);
    //            transform.DOKill();
    //            Destroy(gameObject, 0.1f);
    //        }
    //        else
    //        {
    //            isMagnet = false;
    //            transform.parent = null;
    //            //transform.DOMoveY(0.5f, 0.5f);
    //            //gameController.RemovePixel(rigid);
    //        }
    //    }
    //}

    //public void Check()
    //{
    //    isCheck = true;
    //}   
}

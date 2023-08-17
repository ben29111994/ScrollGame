using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using TMPro;

public class LevelGenerator : MonoBehaviour {

    public static LevelGenerator Instance;
    [Header("Level And Tasks")]
    public List<Tasks> list2DMaps = new List<Tasks>();
    public List<Color32> listColors = new List<Color32>();
    public List<Tile> listScrolls = new List<Tile>();
    public List<Tile> listFloors = new List<Tile>();
    public List<ScrollControl> listScrollsSpawn = new List<ScrollControl>();
    public Texture2D map;
    public Tile tilePrefab;
    public GameObject parentObject;
    public GameObject wallPrefab;
    public int numOfStacks;
    public int totalBall;
    Transform currentParent;
    Vector3 originalPos;
    float width;
    List<float> listPosX = new List<float>();
    public LayerMask layer;
    public TextMeshProUGUI challengeTxt;

    [System.Serializable]
    public class Tasks
    {
        public List<Texture2D> listTasks = new List<Texture2D>();
    }

    void Start()
    {
        Instance = this;
        originalPos = parentObject.transform.position;
        NextTask();
    }

    public void NextTask()
    {
        foreach(Transform child in parentObject.transform)
        {
            Destroy(child.gameObject);
        }
        listFloors.Clear();
        listScrollsSpawn.Clear();
        listPosX.Clear();
        GameController.Instance.Reset();
        var currentTask = DataManager.Instance.Task;
        challengeTxt.text = "Challenge " + (currentTask + 1).ToString();
        var currentLevel = DataManager.Instance.LevelGame;
        if (currentLevel >= list2DMaps.Count - 1)
        {
            currentLevel = 0;
            DataManager.Instance.LevelGame = currentLevel;
        }
        map = list2DMaps[currentLevel].listTasks[currentTask];       
        currentParent = parentObject.transform;
        GameController.totalPixel = 0;
        currentParent.transform.DOMoveZ(50, 0);
        GenerateMap(map);
        //parentObject.transform.position = originalPos;
        parentObject.transform.localScale = Vector3.one * (20 / width);
        float centerPos = 0;
        for (int i = 0; i < listPosX.Count; i++)
        {
            centerPos += listPosX[i] * parentObject.transform.localScale.x;
        }
        centerPos /= listPosX.Count;
        parentObject.transform.DOMoveX(Mathf.Abs(centerPos), 0);
        parentObject.transform.DOMoveZ(originalPos.z, 1);
        //CameraFitter.instance.CameraCompute();
    }

    public bool CheckWin()
    {
        foreach(var item in listScrollsSpawn)
        {
            if (item.isReleased == false)
                return false;
        }
        foreach (var item in listFloors)
        {
            RaycastHit hit;
            Vector3 upVector = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y + 1, item.transform.localPosition.z);
            Vector3 dir = upVector - item.transform.localPosition;
            if (Physics.Raycast(item.transform.position, dir, out hit, Mathf.Infinity, layer))
            {
                if (!hit.transform.CompareTag("Scroll"))
                    return false;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private void GenerateMap(Texture2D texture)
    {
        width = texture.width;
        float ratioX = texture.width;
        float ratioY = texture.height;
        float ratio;
        if (ratioY > ratioX)
        {
            ratio = ratioX / ratioY;
        }
        else
        {
            ratio = ratioY / ratioX;
        }
        if(ratio < 0.6f && ratio > 0.4f)
        {
            ratio = 1;
        }

        Vector3 positionTileParent = new Vector3(-((texture.width - 1) / 2), 0, -((texture.height - 1) / 2));
        currentParent.localPosition = positionTileParent;      

        for (int x = -1; x <= texture.width; x++)
        {
            for (int y = -1; y <= texture.height; y++)
            {
                if (x == texture.width || y == texture.height || x == -1 || y == -1)
                {
                    GenerateWall(texture, x, y, ratio);
                }
                else
                {
                    GenerateTile(texture, x, y, ratio);
                }
            }
        }
    }

    void GenerateWall(Texture2D texture, int x, int y, float ratio)
    {
        GameObject wall;

        wall = Instantiate(wallPrefab);
        Vector3 scale = Vector3.one * 0.1f;
        Vector3 pos = new Vector3(x - texture.width / 2, 0, y);
        wall.transform.parent = currentParent;
        wall.transform.localScale = scale;
        wall.transform.localPosition = pos;
    }

    private void GenerateTile(Texture2D texture, int x, int y, float ratio)
    {
        Color pixelColor = texture.GetPixel(x, y);
        Color rgbaColor = pixelColor;
        if (pixelColor == new Color32(255, 255, 255, 255) || pixelColor.a == 0 || pixelColor == null)
        {
            pixelColor = Color.white;
            rgbaColor = Color.white;
        }

        Tile floor;

        floor = Instantiate(tilePrefab);
        Vector3 scale = Vector3.one * 0.1f;
        Vector3 pos = new Vector3(x - texture.width / 2, 0, y);
        if(y==0)
            listPosX.Add(pos.x);
        floor.transform.parent = currentParent;
        floor.transform.localScale = scale;
        floor.Init();
        floor.SetTransfrom(pos);
        floor.SetColor(Color.white);
        listFloors.Add(floor);

        Tile instance;
        var hex = ColorUtility.ToHtmlStringRGBA(pixelColor);
        hex = hex.Remove(6, 2);
        hex = hex.ToLower();
        //Debug.Log(hex);
        scale = Vector3.one * 0.098f;

        switch (hex)
        {
            //up
            case "2ae9f7":
                instance = Instantiate(listScrolls[1]);
                instance.transform.localEulerAngles = new Vector3(0, 270, 0);
                break;
            //down
            case "c6f723":
                instance = Instantiate(listScrolls[1]);
                instance.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            //left
            case "2762f5":
                instance = Instantiate(listScrolls[1]);
                instance.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            //right
            case "eef527":
                instance = Instantiate(listScrolls[1]);
                break;
            //left
            case "04db9e":
                instance = Instantiate(listScrolls[2]);
                instance.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            //right
            case "d98404":
                instance = Instantiate(listScrolls[2]);
                break;
            //up
            case "62f527":
                instance = Instantiate(listScrolls[3]);
                instance.transform.localEulerAngles = new Vector3(0, 270, 0);
                break;
            //down
            case "279cf5":
                instance = Instantiate(listScrolls[3]);
                instance.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            //left
            case "f44236":
                instance = Instantiate(listScrolls[3]);
                instance.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            //right
            case "cc27f5":
                instance = Instantiate(listScrolls[3]);
                break;
            //up
            case "fae605":
                instance = Instantiate(listScrolls[4]);
                instance.transform.localEulerAngles = new Vector3(0, 270, 0);
                break;
            //down
            case "fa0526":
                instance = Instantiate(listScrolls[4]);
                instance.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            //left
            case "2efa05":
                instance = Instantiate(listScrolls[4]);
                instance.transform.localEulerAngles = new Vector3(0, 180, 0);
                break;
            //right
            case "ee05fa":
                instance = Instantiate(listScrolls[4]);
                break;
            //up
            case "3facfa":
                instance = Instantiate(listScrolls[5]);
                instance.transform.localEulerAngles = new Vector3(0, 270, 0);
                break;
            //down
            case "fa41cf":
                instance = Instantiate(listScrolls[5]);
                instance.transform.localEulerAngles = new Vector3(0, 90, 0);
                break;
            default:
                instance = null;
                break;
        }

        if (instance != null)
        {
            instance.transform.parent = currentParent;
            instance.transform.localScale = scale;
            pos = new Vector3(x - texture.width / 2, 0.5f, y);
            GameController.totalPixel++;
            instance.Init();
            instance.SetTransfrom(pos);
            instance.SetColor(rgbaColor);
            var scrollControl = instance.GetComponentInChildren<ScrollControl>();
            listScrollsSpawn.Add(scrollControl);
        }
    }

}

using System.Linq;
using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    public static CameraFitter instance;
    public Camera Camera;
    public GameObject FitObject;

    public void Awake()
    {
        instance = this;
    }

    public void CameraCompute()
    {
        FocusOn(Camera, FitObject, .99f);
    }

    public static Bounds GetBoundsWithChildren(GameObject gameObject)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0 ? renderers.FirstOrDefault().bounds : new Bounds();

        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i].enabled)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return bounds;
    }
    public static void FocusOn(Camera camera, GameObject focusedObject, float marginPercentage)
    {
        Bounds bounds = GetBoundsWithChildren(focusedObject);
        Vector3 centerAtFront = new(bounds.center.x, bounds.max.y, bounds.center.z);
        Vector3 centerAtFrontTop = new(bounds.center.x, bounds.max.y, bounds.max.z);
        float centerToTopDist = (centerAtFrontTop - centerAtFront).magnitude;
        float minDistanceY = centerToTopDist * marginPercentage / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        Vector3 centerAtFrontRight = new(bounds.max.x, bounds.center.y, bounds.max.z);
        float centerToRightDist = (centerAtFrontRight - centerAtFront).magnitude;
        float minDistanceX = centerToRightDist * marginPercentage / Mathf.Tan(camera.fieldOfView * camera.aspect * Mathf.Deg2Rad);

        float minDistance = Mathf.Max(minDistanceX, minDistanceY);

        camera.transform.position = new Vector3(bounds.center.x, bounds.center.y + minDistance, bounds.center.z);
        camera.transform.LookAt(bounds.center);
    }
}

using UnityEngine;
using Creation;

[RequireComponent(typeof(ObjectMeshBuilder))]
public class ObjectCamera : MonoBehaviour
{
    public static ObjectCamera GetInstance()
    {
        return ins;
    }

    public Camera cam;
    public ObjectMeshBuilder meshBuilder;
    public Transform objectPosition;
    public Light spot;
    public Shader shader;

    private Quaternion objRotation;
    private static ObjectCamera ins;

    private GameObject obj;
    private Bounds bounds;

    private void Awake()
    {
        if (null == ins)
        {
            ins = this;
            objRotation = objectPosition.localRotation;
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlaceObject(GameObject obj, Transform lookPos = null)
    {
        UnplaceObject();

        GameObject copy = new GameObject(obj.transform.name + " TMP");
        objectPosition.transform.localPosition = Vector3.zero;
        objectPosition.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        copy.transform.SetParent(objectPosition, false);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        copy.layer = transform.gameObject.layer;
        bounds = meshBuilder.CloneObject(obj, copy, shader);
        this.obj = copy;

        if (null == lookPos)
        {
            Vector3 objectSizes = bounds.max - bounds.min;
            float distance = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);

            Vector3 pos = cam.transform.forward * distance + new Vector3(
                (bounds.max.x + bounds.min.x) * 0.5f * obj.transform.right.x,
                (bounds.max.y + bounds.min.y) * 0.5f * -obj.transform.up.y,
                (bounds.max.z + bounds.min.z)
            );

            objectPosition.transform.localRotation = objRotation;
            objectPosition.transform.localPosition = pos;
        }
        else
        {
            // TODO optimize positioning
            Vector3 relPosition = lookPos.position - obj.transform.position;
            objectPosition.transform.localRotation = obj.transform.localRotation;
            this.obj.transform.localPosition -= bounds.center;
            cam.transform.position = objectPosition.position + relPosition;
        }

        cam.transform.LookAt(this.obj.transform.position);
        //TODO ??? spot.transform.LookAt(this.obj.transform.position);
        gameObject.SetActive(true);
    }

    private void UnplaceObject()
    {
        gameObject.transform.SetParent(UIGame.GetInstance().transform, false);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (null != obj)
        {
            Destroy(obj.gameObject);
            obj = null;
        }

        gameObject.SetActive(false);
    }

    public Texture2D CreateObjectIcon(GameObject obj, int width, int height, Transform lookPos = null)
    {
        bool isActive = obj.activeSelf;
        obj.SetActive(true);
        PlaceObject(obj, lookPos);
        obj.SetActive(isActive);
        cam.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        cam.Render();
        RenderTexture.active = cam.targetTexture;
        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false)
        {
            name = obj.name,
            filterMode = FilterMode.Bilinear
        };
        Rect rect = new Rect(0, 0, width, height);
        tex.ReadPixels(rect, 0, 0);
        tex.Apply();
        // TODO do not unplace when testing object place method
        UnplaceObject();

        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(cam.targetTexture);
        return tex;
    }
}
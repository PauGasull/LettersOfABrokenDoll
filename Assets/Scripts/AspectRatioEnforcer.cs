using UnityEngine;

[ExecuteInEditMode]
public class AspectRatioEnforcer : MonoBehaviour
{
    public Vector2 aspectRatio = new Vector2(16f, 9f);

    Camera cam;
    float targetAspect;

    void Awake()
    {
        cam = GetComponent<Camera>(); // Getting Reference to the Camera
    }

    void Start()
    {
        targetAspect = aspectRatio.x / aspectRatio.y; // Calculate Aspect Ratio
    }

    void Update()
    {
        float scale = CalculateAspect();

        // if scaled height is less than current height, add letterbox
        if (scale < 1.0f)
            addLetterbox(scale);
        else // if it's greater, add pillarbox
            addPillarbox(scale);
    }

    float CalculateAspect()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height; // Current aspect ratio
        return windowAspect / targetAspect; // Scale Factor
    }

    void addLetterbox(float scaleHeight)
    {
        Rect rect = cam.rect;

        rect.width = 1.0f;
        rect.height = scaleHeight;
        rect.x = 0;
        rect.y = (1.0f - scaleHeight) / 2.0f;

        cam.rect = rect;
    }

    void addPillarbox(float scaleHeight)
    {
        float scalewidth = 1.0f / scaleHeight;

        Rect rect = cam.rect;

        rect.width = scalewidth;
        rect.height = 1.0f;
        rect.x = (1.0f - scalewidth) / 2.0f;
        rect.y = 0;

        cam.rect = rect;
    }
}

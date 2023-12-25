using UnityEngine;

public class AspectRatioEnforcer : MonoBehaviour
{
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        if (camera == null)
        {
            Debug.LogError("AspectRatioEnforcer needs to be attached to a camera.");
            return;
        }

        // Calculate the aspect ratio
        float targetAspect = 4.0f / 3.0f;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // Create a new rect with the adjusted size
        Rect rect = camera.rect;

        if (scaleHeight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        camera.rect = rect;
    }
}

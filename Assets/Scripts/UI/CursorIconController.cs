using UnityEngine;
using UnityEngine.UI;

public class CursorIconController : MonoBehaviour
{
    public Image cursorImage;
    public Canvas canvas;

    void Awake() { if (cursorImage) cursorImage.enabled = false; }

    public void Show(Sprite s)
    {
        if (!cursorImage) return;
        cursorImage.sprite = s;
        cursorImage.preserveAspect = true;
        cursorImage.enabled = s != null;
    }

    public void Hide() { if (cursorImage) cursorImage.enabled = false; }

    void LateUpdate()
    {
        if (cursorImage && cursorImage.enabled)
            cursorImage.rectTransform.position = Input.mousePosition;
    }
}

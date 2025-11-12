using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeWindowUI : MonoBehaviour
{
    public Transform gridContainer;
    public Image cellTemplate;
    public Color gapsColor = new Color(0.85f, 0.87f, 0.9f);
    public Color cellBgColor = Color.white;
    public Vector2 spacing = new Vector2(6, 6);
    public int padding = 6;
    public bool autoSquareCell = true;
    public float minCell = 64f;
    public float maxCell = 128f;

    GridLayoutGroup layout;
    readonly List<GameObject> spawned = new List<GameObject>();
    Image bg;

    void Awake()
    {
        if (gridContainer == null) gridContainer = transform;
        if (cellTemplate != null) cellTemplate.gameObject.SetActive(false);
        layout = gridContainer.GetComponent<GridLayoutGroup>();
        if (layout == null) layout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
        bg = gridContainer.GetComponent<Image>();
        if (bg == null) bg = gridContainer.gameObject.AddComponent<Image>();
        bg.raycastTarget = false;
    }

    public void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
        ForceRebuild();
    }

    public void Render(Recipe recipe)
    {
        if (recipe == null) { Clear(); return; }
        bg.color = gapsColor;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = Mathf.Max(1, recipe.Width);
        layout.spacing = spacing;
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.childAlignment = TextAnchor.UpperLeft;
        if (autoSquareCell)
        {
            var rt = gridContainer as RectTransform;
            float totalW = rt.rect.width - layout.padding.left - layout.padding.right - spacing.x * (recipe.Width - 1);
            float totalH = rt.rect.height - layout.padding.top - layout.padding.bottom - spacing.y * (recipe.Height - 1);
            float cw = Mathf.Floor(totalW / recipe.Width);
            float ch = Mathf.Floor(totalH / recipe.Height);
            float size = Mathf.Clamp(Mathf.Min(cw, ch), minCell, maxCell);
            layout.cellSize = new Vector2(size, size);
        }
        Clear();
        for (int y = 0; y < recipe.Height; y++)
        for (int x = 0; x < recipe.Width; x++)
        {
            Image cell = Instantiate(cellTemplate, gridContainer);
            cell.gameObject.SetActive(true);
            cell.sprite = null;
            cell.color = cellBgColor;
            cell.type = Image.Type.Simple;
            cell.preserveAspect = false;
            Image icon = FindOrCreateIcon(cell.transform);
            var def = recipe.Get(x, y);
            if (def != null && def.icon != null)
            {
                icon.enabled = true;
                icon.sprite = def.icon;
                icon.color = Color.white;
                icon.preserveAspect = true;
            }
            else
            {
                icon.enabled = false;
                icon.sprite = null;
            }
            spawned.Add(cell.gameObject);
        }
        ForceRebuild();
    }

    Image FindOrCreateIcon(Transform cell)
    {
        var t = cell.Find("Icon");
        if (t != null) return t.GetComponent<Image>();
        var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(cell, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    void ForceRebuild()
    {
        var rt = gridContainer as RectTransform;
        if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}

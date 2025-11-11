using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ForgeUIController : MonoBehaviour
{
    [Header("Grid")]
    public GridLayoutGroup gridLayout;
    public Button cellTemplate;

    [Header("Actions")]
    public Button forgeButton;
    public Button deliverButton;

    [Header("Feedback")]
    public GameObject feedbackCoin;
    public GameObject feedbackAngry;

    [Header("Result Output")]
    public Image resultImage;
    public Text resultLabel;            public TMP_Text resultLabelTMP;
    public Text movesLabel;             public TMP_Text movesLabelTMP;

    [Header("Requested HUD")]
    public Image requestedImage;
    public Text requestedLabel;         public TMP_Text requestedLabelTMP;
    public Text scoreLabel;             public TMP_Text scoreLabelTMP;
    public Text timerLabel;             public TMP_Text timerLabelTMP;

    [Header("Materials DB & Selector")]
    public MaterialDatabase materialDB;
    public Transform selectorContainer;
    public Button selectorButtonTemplate; // Button con hijo Image llamado "Icon"

    [Header("Cursor Icon")]
    public CursorIconController cursorIcon;

    private ForgeManager manager;
    private Button[,] buttons;
    private const string IconChildName = "Icon";

    // Solo destruimos lo que nosotros instanciamos aquí
    private readonly List<GameObject> spawnedSelectorButtons = new List<GameObject>();

    // ---------- Public API ----------
    public void Bind(ForgeManager mgr)
    {
        manager = mgr;
        if (selectorButtonTemplate) selectorButtonTemplate.gameObject.SetActive(false);

        BuildGrid(manager.Grid.Width, manager.Grid.Height);
        BuildSelectorAllFromDatabase();               // inicial por si aún no hay receta
        if (forgeButton)   forgeButton.onClick.AddListener(manager.TryForge);
        if (deliverButton) deliverButton.onClick.AddListener(manager.Deliver);
    }

    public void UpdateGrid()
    {
        for (int y = 0; y < manager.Grid.Height; y++)
        for (int x = 0; x < manager.Grid.Width; x++)
        {
            var btn = buttons[x, y];
            Image icon = null;
            var iconTr = btn.transform.Find(IconChildName);
            if (iconTr != null) icon = iconTr.GetComponent<Image>();
            if (icon == null) icon = btn.GetComponent<Image>();

            if (icon != null)
            {
                var cell = manager.Grid.Cells[x, y];
                var sprite = cell != null && cell.Definition != null ? cell.Definition.icon : null;
                icon.sprite = sprite;
                icon.preserveAspect = true;
                icon.color = sprite ? Color.white : new Color(1, 1, 1, 0.2f);
                icon.enabled = true;
            }
        }
    }

    public void UpdateMoves(int moves)
    {
        if (movesLabel)     movesLabel.text     = "Moves: " + moves;
        if (movesLabelTMP)  movesLabelTMP.text  = "Moves: " + moves;
    }

    public void ShowForgeResult(string name, Sprite sprite, bool success)
    {
        if (resultLabel)    resultLabel.text    = name;
        if (resultLabelTMP) resultLabelTMP.text = name;

        if (resultImage)
        {
            resultImage.sprite = sprite;
            resultImage.enabled = sprite != null;
            resultImage.color = success ? Color.white : Color.red;
            resultImage.preserveAspect = true;
        }
    }

    public void ClearForgeResult()
    {
        if (resultLabel)    resultLabel.text    = "";
        if (resultLabelTMP) resultLabelTMP.text = "";
        if (resultImage) { resultImage.sprite = null; resultImage.enabled = false; }
    }

    public void ShowRequested(string name, Sprite sprite)
    {
        if (requestedLabel)    requestedLabel.text    = name;
        if (requestedLabelTMP) requestedLabelTMP.text = name;

        if (requestedImage)
        {
            requestedImage.sprite = sprite;
            requestedImage.enabled = sprite != null;
            requestedImage.preserveAspect = true;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreLabel)    scoreLabel.text    = "Score: " + score;
        if (scoreLabelTMP) scoreLabelTMP.text = "Score: " + score;
    }

    public void UpdateTimer(float t)
    {
        string txt = "Time: " + Mathf.CeilToInt(t) + "s";
        if (timerLabel)    timerLabel.text    = txt;
        if (timerLabelTMP) timerLabelTMP.text = txt;
    }

    public void ShowFeedback(bool success)
    {
        if (feedbackCoin)  feedbackCoin.SetActive(success);
        if (feedbackAngry) feedbackAngry.SetActive(!success);
        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), 1.2f);
    }

    public void ShowCursor(Sprite s) { if (cursorIcon) cursorIcon.Show(s); }
    public void HideCursor()         { if (cursorIcon) cursorIcon.Hide(); }

    // ---------- Selector dinámico ----------
    public void UpdateSelectorForRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            BuildSelectorAllFromDatabase();
            return;
        }

        var mats = recipe.GetRequiredMaterials();
        if (mats == null || mats.Count == 0)
        {
            BuildSelectorAllFromDatabase();
            return;
        }

        BuildSelectorFromList(mats);
        if (mats.Count > 0 && manager != null && mats[0] != null)
            manager.SetSelectedMaterial(mats[0]);
    }

    void BuildSelectorAllFromDatabase()
    {
        if (materialDB == null) { ClearSelectorButtons(); return; }
        BuildSelectorFromList(materialDB.materials);
    }

    void ClearSelectorButtons()
    {
        foreach (var go in spawnedSelectorButtons)
            if (go) Destroy(go);
        spawnedSelectorButtons.Clear();

        // no tocamos otros hijos del contenedor (fondos, placeholders, etc.)
    }

    void BuildSelectorFromList(IList<MaterialDefinition> list)
    {
        if (!selectorContainer || !selectorButtonTemplate) return;

        ClearSelectorButtons();

        if (list == null) return;

        // Evitar duplicados (p. ej., patrón con el mismo material repetido)
        var unique = new HashSet<MaterialDefinition>();
        foreach (var m in list) if (m) unique.Add(m);

        foreach (var mat in unique)
        {
            var btn = Instantiate(selectorButtonTemplate, selectorContainer);
            ActivateHierarchy(btn.gameObject);  // activa root + hijos

            // Garantiza que el Button esté operativo
            var buttonComp = btn.GetComponent<Button>();
            if (buttonComp)
            {
                buttonComp.enabled = true;
                buttonComp.interactable = true;
                if (buttonComp.targetGraphic) buttonComp.targetGraphic.enabled = true;
            }

            // Imagen del icono
            Image img = null;
            var iconTr = btn.transform.Find(IconChildName);
            if (iconTr) img = iconTr.GetComponent<Image>();
            if (!img)   img = btn.GetComponent<Image>();
            if (!img)
            {
                var go = new GameObject(IconChildName, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(btn.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                img = go.GetComponent<Image>();
            }

            img.enabled = true;
            img.sprite = mat.icon;
            img.preserveAspect = true;
            img.color = Color.white;

            btn.onClick.AddListener(() => manager.SetSelectedMaterial(mat));
            spawnedSelectorButtons.Add(btn.gameObject);
        }

        // refrescar layout
        var rtContainer = selectorContainer as RectTransform;
        if (rtContainer) LayoutRebuilder.ForceRebuildLayoutImmediate(rtContainer);
    }

    // ---------- Internals ----------
    void HideFeedback()
    {
        if (feedbackCoin)  feedbackCoin.SetActive(false);
        if (feedbackAngry) feedbackAngry.SetActive(false);
    }

    void BuildGrid(int w, int h)
    {
        foreach (Transform c in gridLayout.transform) Destroy(c.gameObject);
        buttons = new Button[w, h];

        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            var btn = Instantiate(cellTemplate, gridLayout.transform);
            btn.gameObject.SetActive(true);

            var t  = btn.GetComponentInChildren<Text>(true);     if (t)  t.text  = "";
            var tt = btn.GetComponentInChildren<TMP_Text>(true); if (tt) tt.text = "";

            int cx = x, cy = y;
            btn.onClick.AddListener(() => manager.PlaceMaterial(cx, cy));

            var trig  = btn.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener((ev) =>
            {
                var ped = (PointerEventData)ev;
                if (ped.button == PointerEventData.InputButton.Right)
                    manager.RemoveMaterial(cx, cy);
            });
            trig.triggers.Add(entry);

            buttons[x, y] = btn;
        }
    }

    void ActivateHierarchy(GameObject go)
    {
        if (!go.activeSelf) go.SetActive(true);
        var trs = go.GetComponentsInChildren<Transform>(true);
        foreach (var t in trs)
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
    }
}

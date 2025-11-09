using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ForgeUIController : MonoBehaviour
{
    public GridLayoutGroup gridLayout;
    public Button cellTemplate;
    public Button forgeButton;
    public Button deliverButton;

    public GameObject feedbackCoin;
    public GameObject feedbackAngry;

    public Image resultImage;
    public Text resultLabel;            public TMP_Text resultLabelTMP;
    public Text movesLabel;             public TMP_Text movesLabelTMP;

    [Header("Requested HUD")]
    public Image requestedImage;
    public Text requestedLabel;          public TMP_Text requestedLabelTMP;
    public Text scoreLabel;              public TMP_Text scoreLabelTMP;
    public Text timerLabel;              public TMP_Text timerLabelTMP;

    [Header("Material Sprites")]
    public Sprite ironSprite;
    public Sprite woodSprite;
    public Sprite diamondSprite;
    public Sprite emptySprite;

    [Header("Cursor Icon")]
    public CursorIconController cursorIcon;

    private ForgeManager manager;
    private Button[,] buttons;
    private const string IconChildName = "Icon";

    public void Bind(ForgeManager mgr)
    {
        manager = mgr;
        BuildGrid(manager.Grid.Width, manager.Grid.Height);
        forgeButton.onClick.AddListener(manager.TryForge);
        if (deliverButton != null) deliverButton.onClick.AddListener(manager.Deliver);
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

            var t = btn.GetComponentInChildren<Text>(true); if (t) t.text = "";
            var tt = btn.GetComponentInChildren<TMP_Text>(true); if (tt) tt.text = "";

            int cx = x, cy = y;
            btn.onClick.AddListener(() => manager.PlaceMaterial(cx, cy));

            var trig = btn.gameObject.AddComponent<EventTrigger>();
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
                var type = cell == null ? MaterialType.None : cell.Type;
                icon.sprite = GetSprite(type);
                icon.preserveAspect = true;
                icon.color = icon.sprite ? Color.white : new Color(1,1,1,0.2f);
            }

            var t = btn.GetComponentInChildren<Text>(true); if (t) t.text = "";
            var tt = btn.GetComponentInChildren<TMP_Text>(true); if (tt) tt.text = "";
        }
    }

    public void UpdateMoves(int moves)
    {
        if (movesLabel) movesLabel.text = "Moves: " + moves;
        if (movesLabelTMP) movesLabelTMP.text = "Moves: " + moves;
    }

    public void ShowForgeResult(string name, Sprite sprite, bool success)
    {
        if (resultLabel) resultLabel.text = name;
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
        if (resultLabel) resultLabel.text = "";
        if (resultLabelTMP) resultLabelTMP.text = "";
        if (resultImage) { resultImage.sprite = null; resultImage.enabled = false; }
    }

    public void ShowRequested(string name, Sprite sprite)
    {
        if (requestedLabel) requestedLabel.text = name;
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
        if (scoreLabel) scoreLabel.text = "Score: " + score;
        if (scoreLabelTMP) scoreLabelTMP.text = "Score: " + score;
    }

    public void UpdateTimer(float t)
    {
        string txt = "Time: " + Mathf.CeilToInt(t) + "s";
        if (timerLabel) timerLabel.text = txt;
        if (timerLabelTMP) timerLabelTMP.text = txt;
    }

    public void ShowFeedback(bool success)
    {
        if (feedbackCoin)  feedbackCoin.SetActive(success);
        if (feedbackAngry) feedbackAngry.SetActive(!success);
        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), 1.2f);
    }

    void HideFeedback()
    {
        if (feedbackCoin)  feedbackCoin.SetActive(false);
        if (feedbackAngry) feedbackAngry.SetActive(false);
    }

    public void ShowCursorFor(MaterialType type)
    {
        if (!cursorIcon) return;
        cursorIcon.Show(GetSprite(type));
    }

    public void HideCursor()
    {
        if (!cursorIcon) return;
        cursorIcon.Hide();
    }

    Sprite GetSprite(MaterialType type)
    {
        switch (type)
        {
            case MaterialType.Iron: return ironSprite;
            case MaterialType.Wood: return woodSprite;
            case MaterialType.Diamond:  return diamondSprite;
            default: return emptySprite;
        }
    }
}

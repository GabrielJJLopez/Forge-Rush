using UnityEngine;
using UnityEngine.SceneManagement;

public class ForgeManager : MonoBehaviour
{
    public RecipeBook recipeBook;
    public MaterialDatabase materialDB;
    public ForgeUIController ui;
    [SerializeField] private GameRules rules;

    public int gridSize = 3;
    public int movesStart = 12;

    public GridModel Grid { get; private set; }
    public int MovesLeft { get; private set; }
    public int Score { get; private set; }
    public MaterialDefinition SelectedMaterial { get; private set; }

    private Recipe requested;
    private Recipe currentCraft;
    private float timeLeft;
    private bool gameOver = false;

    public bool IsGameOver { get { return gameOver; } }

    void Start()
    {
        Grid = new GridModel(gridSize, gridSize);
        MovesLeft = movesStart;

        ui.Bind(this);
        ui.UpdateGrid();
        ui.UpdateMoves(MovesLeft);
        ui.UpdateScore(Score);

        if (materialDB != null && materialDB.materials.Count > 0)
            SetSelectedMaterial(materialDB.materials[0]);

        StartNewOrder();
    }

    void Update()
    {
        if (gameOver) return;
        if (requested == null) return;

        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            ui.UpdateTimer(timeLeft);
            if (timeLeft <= 0f && rules != null && rules.defeatOnTimeout)
                GameOver("Out of time");
        }
    }

    public void SetSelectedMaterial(MaterialDefinition def)
    {
        if (gameOver) return;
        SelectedMaterial = def;
        ui.ShowCursor(def ? def.icon : null);
    }

    public void PlaceMaterial(int x, int y)
    {
        if (gameOver) return;
        if (SelectedMaterial == null) return;
        if (MovesLeft <= 0) { if (rules.defeatOnOutOfMoves) GameOver("No moves left"); return; }

        if (Grid.IsEmpty(x, y))
        {
            if (Grid.Place(x, y, new MaterialToken(SelectedMaterial)))
            {
                MovesLeft--;
                ui.UpdateGrid();
                ui.UpdateMoves(MovesLeft);
                if (MovesLeft <= 0 && rules.defeatOnOutOfMoves) GameOver("No moves left");
            }
        }
    }

    public void RemoveMaterial(int x, int y)
    {
        if (gameOver) return;
        if (MovesLeft <= 0) { if (rules.defeatOnOutOfMoves) GameOver("No moves left"); return; }

        if (!Grid.IsEmpty(x, y))
        {
            if (Grid.ClearCell(x, y))
            {
                MovesLeft--;
                ui.UpdateGrid();
                ui.UpdateMoves(MovesLeft);
                if (MovesLeft <= 0 && rules.defeatOnOutOfMoves) GameOver("No moves left");
            }
        }
    }

    public void TryForge()
    {
        if (gameOver) return;

        ui.HideCursor();
        currentCraft = recipeBook ? recipeBook.FindMatch(Grid.Cells) : null;

        if (currentCraft != null)
        {
            Grid.ClearAll();
            ui.UpdateGrid();
            ui.ShowForgeResult(currentCraft.ResultName, currentCraft.ResultSprite, true);
        }
        else
        {
            int penalty = (rules != null) ? Mathf.Max(0, rules.wrongForgePenalty) : 5;
            int newScore = Score - penalty;
            Score = (rules != null && rules.clampScoreToZero) ? Mathf.Max(0, newScore) : newScore;
            ui.UpdateScore(Score);
            ui.ShowForgeResult("Invalid Craft", null, false);
        }
    }

    public void Deliver()
    {
        if (gameOver) return;
        if (currentCraft == null) { ui.ShowFeedback(false); return; }

        bool correct = (requested != null && currentCraft.ResultName == requested.ResultName);
        if (correct)
        {
            Score += requested.Points;
            ui.UpdateScore(Score);
            ui.ShowFeedback(true);
        }
        else
        {
            ui.ShowFeedback(false);
        }

        if (gameOver) return;

        ui.ClearForgeResult();
        if (SelectedMaterial) ui.ShowCursor(SelectedMaterial.icon);
        StartNewOrder();
    }

    void StartNewOrder()
    {
        if (gameOver) return;

        requested = recipeBook ? recipeBook.GetRandom() : null;
        currentCraft = null;

        Grid.ClearAll(); ui.UpdateGrid();
        MovesLeft = movesStart; ui.UpdateMoves(MovesLeft);
        ui.ClearForgeResult();

        if (requested != null)
        {
            timeLeft = Mathf.Max(1f, requested.TimeLimit);
            ui.ShowRequested(requested.ResultName, requested.ResultSprite);
            ui.UpdateTimer(timeLeft);
            ui.UpdateSelectorForRecipe(requested);
            ui.ShowRequestedRecipe(requested);
        }
        else
        {
            timeLeft = 0f;
            ui.ShowRequested("No Recipes", null);
            ui.UpdateTimer(0f);
            ui.UpdateSelectorForRecipe(null);
            ui.ShowRequestedRecipe(null);
        }
    }

    void GameOver(string reason)
    {
        if (gameOver) return;
        gameOver = true;
        ui.ShowGameOver(reason, Score);
    }

    public void Restart()
    {
        var s = SceneManager.GetActiveScene();
        SceneManager.LoadScene(s.buildIndex);
    }
}

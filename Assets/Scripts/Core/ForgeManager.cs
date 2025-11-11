using UnityEngine;

public class ForgeManager : MonoBehaviour
{
    public RecipeBook recipeBook;
    public MaterialDatabase materialDB;
    public ForgeUIController ui;

    public int gridSize = 3;
    public int movesStart = 12;

    public GridModel Grid { get; private set; }
    public int MovesLeft { get; private set; }
    public int Score { get; private set; }
    public MaterialDefinition SelectedMaterial { get; private set; }

    private Recipe requested;
    private Recipe currentCraft;
    private float timeLeft;

    void Start()
    {
        Grid = new GridModel(gridSize, gridSize);
        MovesLeft = movesStart;

        ui.Bind(this);
        ui.UpdateGrid();
        ui.UpdateMoves(MovesLeft);
        ui.UpdateScore(Score);

        // Si hay DB, selecciona el primero por defecto hasta que llegue la primera orden
        if (materialDB != null && materialDB.materials.Count > 0)
            SetSelectedMaterial(materialDB.materials[0]);

        StartNewOrder();
    }

    void Update()
    {
        if (requested == null) return;
        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            ui.UpdateTimer(timeLeft);
            if (timeLeft <= 0f) OnOrderTimeout();
        }
    }

    public void SetSelectedMaterial(MaterialDefinition def)
    {
        SelectedMaterial = def;
        ui.ShowCursor(def ? def.icon : null);
    }

    public void PlaceMaterial(int x, int y)
    {
        if (SelectedMaterial == null) return;
        if (MovesLeft <= 0) return;
        if (Grid.IsEmpty(x, y))
        {
            if (Grid.Place(x, y, new MaterialToken(SelectedMaterial)))
            { MovesLeft--; ui.UpdateGrid(); ui.UpdateMoves(MovesLeft); }
        }
    }

    public void RemoveMaterial(int x, int y)
    {
        if (MovesLeft <= 0) return;
        if (!Grid.IsEmpty(x, y))
        {
            if (Grid.ClearCell(x, y))
            { MovesLeft--; ui.UpdateGrid(); ui.UpdateMoves(MovesLeft); }
        }
    }

    public void TryForge()
    {
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
            ui.ShowForgeResult("Invalid Craft", null, false);
        }
    }

    public void Deliver()
    {
        if (currentCraft == null) { ui.ShowFeedback(false); return; }

        bool correct = (requested != null && currentCraft.ResultName == requested.ResultName);
        if (correct) { Score += requested.Points; ui.UpdateScore(Score); ui.ShowFeedback(true); }
        else { ui.ShowFeedback(false); }

        ui.ClearForgeResult();
        if (SelectedMaterial) ui.ShowCursor(SelectedMaterial.icon);
        StartNewOrder();
    }

    void StartNewOrder()
    {
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

            // ★ Actualiza el selector para mostrar SOLO los materiales requeridos
            ui.UpdateSelectorForRecipe(requested);
        }
        else
        {
            timeLeft = 0f;
            ui.ShowRequested("No Recipes", null);
            ui.UpdateTimer(0f);

            // Si no hay orden, lista todos los materiales de la DB
            ui.UpdateSelectorForRecipe(null);
        }
    }

    void OnOrderTimeout()
    {
        ui.ShowFeedback(false);
        ui.ClearForgeResult();
        if (SelectedMaterial) ui.ShowCursor(SelectedMaterial.icon);
        StartNewOrder();
    }
}

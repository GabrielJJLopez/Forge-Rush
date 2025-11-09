using UnityEngine;

[System.Serializable]
public class Recipe
{
    public string ResultName;
    public Sprite ResultSprite;
    public int Width = 3;
    public int Height = 3;
    public MaterialDefinition[] Pattern = new MaterialDefinition[9];

    public int Points = 10;
    public float TimeLimit = 30f;

    public MaterialDefinition Get(int x, int y)
    {
        int idx = y * Width + x;
        if (Pattern == null || idx < 0 || idx >= Pattern.Length) return null;
        return Pattern[idx];
    }

    public bool Matches(MaterialToken[,] grid)
    {
        if (grid == null) return false;

        int gw = grid.GetLength(0);
        int gh = grid.GetLength(1);

        if (Width <= 0 || Height <= 0) return false;
        if (gw < Width || gh < Height) return false;
        if (Pattern == null || Pattern.Length < Width * Height) return false;

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var expected = Get(x, y);
            var token    = grid[x, y];
            var placed   = token != null ? token.Definition : null;
            if (expected != placed) return false;
        }
        return true;
    }

    // Utilidad pública: ajusta el tamaño del Pattern a Width*Height
    public void FixPatternSize()
    {
        int target = Mathf.Max(1, Width) * Mathf.Max(1, Height);
        if (Pattern == null || Pattern.Length != target)
        {
            var old = Pattern;
            Pattern = new MaterialDefinition[target];
            if (old != null)
            {
                int copy = Mathf.Min(old.Length, Pattern.Length);
                for (int i = 0; i < copy; i++) Pattern[i] = old[i];
            }
        }
    }
}

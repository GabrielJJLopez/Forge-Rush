public class GridModel
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    private MaterialToken[,] _cells;

    public GridModel(int width, int height)
    {
        Width = width; Height = height;
        _cells = new MaterialToken[width, height];
    }

    public MaterialToken[,] Cells => _cells;

    public bool IsInside(int x, int y)  { return x >= 0 && y >= 0 && x < Width && y < Height; }
    public bool IsEmpty(int x, int y)   { return IsInside(x, y) && _cells[x, y] == null; }

    public bool Place(int x, int y, MaterialToken token)
    {
        if (!IsInside(x, y) || _cells[x, y] != null) return false;
        _cells[x, y] = token; return true;
    }

    public bool ClearCell(int x, int y)
    {
        if (!IsInside(x, y) || _cells[x, y] == null) return false;
        _cells[x, y] = null; return true;
    }

    public void ClearAll()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _cells[x, y] = null;
    }
}

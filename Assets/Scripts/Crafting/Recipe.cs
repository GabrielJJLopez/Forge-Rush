using UnityEngine;

[System.Serializable]
public class Recipe
{
    public string ResultName;
    public Sprite ResultSprite;
    public int Width = 3;
    public int Height = 3;
    public MaterialType[] Pattern = new MaterialType[9];

    [Header("Game Rules")]
    public int Points = 10;
    public float TimeLimit = 30f;

    public MaterialType Get(int x, int y){ return Pattern[y*Width + x]; }

    public bool Matches(MaterialToken[,] grid)
    {
        for(int y=0;y<Height;y++)
            for(int x=0;x<Width;x++)
            {
                var expected = Get(x,y);
                var placed = grid[x,y]!=null ? grid[x,y].Type : MaterialType.None;
                if(expected!=placed) return false;
            }
        return true;
    }
}

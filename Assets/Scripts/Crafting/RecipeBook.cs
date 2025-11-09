using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ForgeRush/RecipeBook")]
public class RecipeBook : ScriptableObject
{
    public List<Recipe> Recipes = new List<Recipe>();

    public Recipe FindMatch(MaterialToken[,] grid)
    {
        foreach (var r in Recipes) if (r != null && r.Matches(grid)) return r;
        return null;
    }

    public Recipe GetRandom()
    {
        if (Recipes == null || Recipes.Count == 0) return null;
        int i = Random.Range(0, Recipes.Count);
        return Recipes[i];
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (Recipes == null) return;
        foreach (var r in Recipes)
            if (r != null) r.FixPatternSize();

        // Marcamos el asset (este sí es UnityEngine.Object)
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ForgeRush/MaterialDatabase")]
public class MaterialDatabase : ScriptableObject
{
    public List<MaterialDefinition> materials = new List<MaterialDefinition>();

    public MaterialDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var m in materials) if (m != null && m.id == id) return m;
        return null;
    }
}

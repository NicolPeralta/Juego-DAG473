using System;
using UnityEngine;

[DisallowMultipleComponent]
public class RandomCharacterByTagAssigner : MonoBehaviour
{
    [Tooltip("Tag de los GameObjects (el Empty que contiene Hair/Clothes/Hat)")]
    public string targetTag = "Character";

    [Serializable]
    public struct Part
    {
        [Tooltip("Nombre del child que contiene el Renderer (ej. 'Hair', 'Clothes', 'Hat')")]
        public string partName;

        [Tooltip("Materiales disponibles para esta parte (arrástralos desde Assets)")]
        public Material[] materials;

        [Tooltip("Si >=0, reemplaza solo ese índice de material (submesh). Si -1, reemplaza todo o el material principal.")]
        public int materialIndex;

        [Tooltip("Buscar el child recursivamente")]
        public bool searchRecursively;

        [Tooltip("Si false, esta parte se ignora")]
        public bool enabled;
    }

    public Part hair;
    public Part clothes;
    public Part hat;

    [Tooltip("Si true usa sharedMaterial (asigna el asset). Si false crea instancia para el Renderer.")]
    public bool useSharedMaterial = true;

    [Tooltip("Aplicar automáticamente en Start")]
    public bool applyOnStart = true;

    [ContextMenu("Asignar materiales aleatorios por Tag")]
    public void AssignRandomMaterialsByTag()
    {
        if (string.IsNullOrEmpty(targetTag))
        {
            Debug.LogWarning("[RandomCharacterByTagAssigner] targetTag vacío.");
            return;
        }

        GameObject[] targets;
        try { targets = GameObject.FindGameObjectsWithTag(targetTag); }
        catch (Exception)
        {
            Debug.LogWarning($"[RandomCharacterByTagAssigner] El tag '{targetTag}' no existe.");
            return;
        }

        if (targets == null || targets.Length == 0)
        {
            Debug.LogWarning($"[RandomCharacterByTagAssigner] No se encontraron objetos con tag '{targetTag}'.");
            return;
        }

        var parts = new Part[] { hair, clothes, hat };

        foreach (var go in targets)
        {
            foreach (var part in parts)
            {
                if (!part.enabled) continue;
                if (part.materials == null || part.materials.Length == 0)
                {
                    Debug.LogWarning($"[RandomCharacterByTagAssigner] No hay materials para '{part.partName}'.");
                    continue;
                }

                Transform t = part.searchRecursively ? FindDeepChild(go.transform, part.partName) : go.transform.Find(part.partName);
                if (t == null)
                {
                    Debug.LogWarning($"[RandomCharacterByTagAssigner] No se encontró child '{part.partName}' en '{go.name}'.");
                    continue;
                }

                // Buscar Renderer en ese child o en sus hijos (permite que solo pongas Renderer)
                var rend = t.GetComponentInChildren<Renderer>(true);
                if (rend == null)
                {
                    Debug.LogWarning($"[RandomCharacterByTagAssigner] '{part.partName}' en '{go.name}' no tiene Renderer.");
                    continue;
                }

                var chosen = part.materials[UnityEngine.Random.Range(0, part.materials.Length)];

                if (part.materialIndex >= 0)
                {
                    // Intentar reemplazar un slot específico
                    var mats = useSharedMaterial ? rend.sharedMaterials : rend.materials;
                    if (part.materialIndex < mats.Length)
                    {
                        mats[part.materialIndex] = chosen;
                        if (useSharedMaterial) rend.sharedMaterials = mats; else rend.materials = mats;
                    }
                    else
                    {
                        // Si el índice no existe, aplicar al material principal
                        if (useSharedMaterial) rend.sharedMaterial = chosen; else rend.material = chosen;
                    }
                }
                else
                {
                    // Reemplazar material principal (o asignar a todos los slots si prefieres)
                    if (useSharedMaterial)
                        rend.sharedMaterial = chosen;
                    else
                        rend.material = chosen;
                }
            }
        }
    }

    void Start()
    {
        if (applyOnStart) AssignRandomMaterialsByTag();
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
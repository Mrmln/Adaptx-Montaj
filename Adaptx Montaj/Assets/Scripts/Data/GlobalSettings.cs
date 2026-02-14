using UnityEngine;

[CreateAssetMenu(fileName = "NewGlobalSettings", menuName = "Adaptx/GlobalSettings")]
public class GlobalSettings : ScriptableObject
{
    [Header("Görsel Ayarlar")]
    [Tooltip("Büyük parçalara (Yan, Raf vb.) atanacak ahşap malzeme")]
    public Material mainWoodMaterial; // Buraya oluşturduğunuz Material'ı atayacaksınız

    [Header("Küçük Parça Prefabları")]
    public GameObject lincoPrefab;
    public GameObject pimPrefab;
    public GameObject rafPimiPrefab;
    public GameObject ayarliAyakPrefab;
    public GameObject civiPrefab;
    public GameObject askilikFlansiPrefab; // Yeni eklenen parça (a-flans)

    // İsme göre prefab bulma
    public GameObject GetPrefabByName(string socketName)
    {
        socketName = socketName.ToLower();

        if (socketName.Contains("linco")) return lincoPrefab;
        if (socketName.Contains("r-pim")) return rafPimiPrefab;
        if (socketName.Contains("a-ayak")) return ayarliAyakPrefab;
        if (socketName.Contains("çivi") || socketName.Contains("civi")) return civiPrefab;
        if (socketName.Contains("flans") || socketName.Contains("flanş")) return askilikFlansiPrefab;
        
        // En sona genel "pim"
        if (socketName.Contains("pim")) return pimPrefab;

        return null; 
    }
}
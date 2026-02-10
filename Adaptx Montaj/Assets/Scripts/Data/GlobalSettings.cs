using UnityEngine;

[CreateAssetMenu(fileName = "NewGlobalSettings", menuName = "Adaptx/GlobalSettings")]
public class GlobalSettings : ScriptableObject
{
    [Header("Küçük Parça Prefabları")]
    public GameObject lincoPrefab;      // Küre
    public GameObject pimPrefab;        // Küp (Normal Pim)
    public GameObject rafPimiPrefab;    // (r-pim)
    public GameObject ayarliAyakPrefab; // (a-ayak)
    public GameObject civiPrefab;       // Arkalık çivisi
    
    // İsme göre doğru parçayı bulan akıllı fonksiyon
    public GameObject GetPrefabByName(string socketName)
    {
        socketName = socketName.ToLower();

        if (socketName.Contains("linco")) return lincoPrefab;
        // Dikkat: "r-pim" içinde de "pim" geçtiği için sıralama önemli.
        // Önce özel olanları kontrol etmeliyiz.
        if (socketName.Contains("r-pim")) return rafPimiPrefab; 
        if (socketName.Contains("a-ayak")) return ayarliAyakPrefab;
        if (socketName.Contains("çivi") || socketName.Contains("civi")) return civiPrefab;
        
        // En sona genel "pim"i bırakıyoruz
        if (socketName.Contains("pim")) return pimPrefab;

        return null; 
    }
}
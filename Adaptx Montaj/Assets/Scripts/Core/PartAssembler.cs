using UnityEngine;
using System.Collections.Generic;

public class PartAssembler : MonoBehaviour
{
    // Parçanın kimliği (Örn: "sag", "sol")
    public string partCode; 

    // Soketleri türlerine göre sakladığımız liste (Örn: "linco" -> [Transform1, Transform2])
    private Dictionary<string, List<Transform>> socketMap = new Dictionary<string, List<Transform>>();

    private GlobalSettings settings;

    // --- KONUM HAFIZASI ---
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // 1. Hazırlık Aşaması: Sadece tarama yapar, hiçbir şey oluşturmaz.
    public void Initialize(GlobalSettings appSettings)
    {
        settings = appSettings;
        
        // İlk açılışta mevcut konumu ve rotasyonu kaydet
        UpdateOriginalTransform();

        socketMap.Clear();

        // Altındaki tüm empty objeleri tara ve kategorize et
        foreach (Transform child in transform)
        {
            string name = child.name.ToLower();
            
            // Soket tipini belirle (linco, pim, a-ayak vs.)
            string typeKey = IdentifySocketType(name);

            if (!string.IsNullOrEmpty(typeKey))
            {
                if (!socketMap.ContainsKey(typeKey))
                    socketMap[typeKey] = new List<Transform>();

                socketMap[typeKey].Add(child);
            }
        }
    }

    // --- YENİ EKLENEN FONKSİYON ---
    // AssemblyManager rotasyonu değiştirdiğinde (Rotation Fix), 
    // parçanın "Orijinal" halini güncellemek için bunu çağırıyoruz.
    public void UpdateOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    // Parçayı çalışma tezgahına (0,0,0) alır
    public void MoveToWorkbench()
    {
        transform.position = Vector3.zero;
        // Rotasyonla oynamıyoruz çünkü onu AssemblyManager ayarladı.
    }

    // Parçayı dolaptaki yerine geri gönder (İleride lazım olacak)
    public void ResetToOriginal()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    // Emir Geldiğinde Çalışan Fonksiyon (Vidayı Tak)
    public void InstallHardware(string hardwareType)
    {
        if (socketMap.ContainsKey(hardwareType))
        {
            foreach (Transform socket in socketMap[hardwareType])
            {
                // Eğer bu sokete daha önce parça takılmamışsa tak
                if (socket.childCount == 0) 
                {
                    GameObject prefab = settings.GetPrefabByName(hardwareType);
                    if (prefab != null)
                    {
                        // Vidayı oluştur ve soketin içine koy
                        GameObject hardware = Instantiate(prefab, socket.position, socket.rotation);
                        hardware.transform.SetParent(socket); 
                    }
                }
            }
        }
    }

    // Soket isminden tip anahtarı çıkaran yardımcı fonksiyon
    string IdentifySocketType(string name)
    {
        if (name.Contains("linco")) return "linco";
        if (name.Contains("r-pim")) return "r-pim"; // Raf pimi
        if (name.Contains("a-ayak")) return "a-ayak"; // Ayarlı Ayak
        if (name.Contains("çivi") || name.Contains("civi")) return "civi"; // Arkalık Çivisi
        if (name.Contains("flans") || name.Contains("flanş")) return "flans"; // Askılık Flanşı
        
        if (name.Contains("pim")) return "pim"; // Normal pim (En son kontrol edilmeli)
        
        return "";
    }

    // Bu parçada istenen vida türü var mı? (Adım atlamak için soracağız)
    public bool HasHardware(string type)
    {
        return socketMap.ContainsKey(type) && socketMap[type].Count > 0;
    }
}
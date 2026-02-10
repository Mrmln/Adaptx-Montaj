using UnityEngine;
using System.Collections.Generic;

public class PartAssembler : MonoBehaviour
{
    public string partCode; 
    private Dictionary<string, List<Transform>> socketMap = new Dictionary<string, List<Transform>>();
    private GlobalSettings settings;

    // --- YENİ EKLENEN KISIM: KONUM HAFIZASI ---
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    // ------------------------------------------

    public void Initialize(GlobalSettings appSettings)
    {
        settings = appSettings;
        
        // 1. Orijinal konumu kaydet (Asla unutma!)
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Soketleri tara
        socketMap.Clear();
        foreach (Transform child in transform)
        {
            string name = child.name.ToLower();
            string typeKey = IdentifySocketType(name);

            if (!string.IsNullOrEmpty(typeKey))
            {
                if (!socketMap.ContainsKey(typeKey))
                    socketMap[typeKey] = new List<Transform>();
                socketMap[typeKey].Add(child);
            }
        }
    }

    // Parçayı çalışma tezgahına (0,0,0) alır
    public void MoveToWorkbench()
    {
        transform.position = Vector3.zero;
        // Rotasyonu da sıfırlayabiliriz ama parça dik dursun istiyorsan
        // originalRotation'ı koruyabilir veya Identity yapabiliriz.
        // Şimdilik sadece pozisyonu sıfırlıyoruz, rotasyon orijinal kalsın.
        // transform.rotation = Quaternion.identity; // Eğer dümdüz olsun istersen bunu aç.
    }

    // İleride lazım olacak: Parçayı dolaptaki yerine geri gönder
    public void ResetToOriginal()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    public void InstallHardware(string hardwareType)
    {
        if (socketMap.ContainsKey(hardwareType))
        {
            foreach (Transform socket in socketMap[hardwareType])
            {
                if (socket.childCount == 0) 
                {
                    GameObject prefab = settings.GetPrefabByName(hardwareType);
                    if (prefab != null)
                    {
                        GameObject hardware = Instantiate(prefab, socket.position, socket.rotation);
                        hardware.transform.SetParent(socket); 
                    }
                }
            }
        }
    }

    string IdentifySocketType(string name)
    {
        if (name.Contains("linco")) return "linco";
        if (name.Contains("r-pim")) return "r-pim"; 
        if (name.Contains("a-ayak")) return "a-ayak";
        if (name.Contains("çivi") || name.Contains("civi")) return "civi";
        if (name.Contains("pim")) return "pim"; 
        return "";
    }

    public bool HasHardware(string type)
    {
        return socketMap.ContainsKey(type) && socketMap[type].Count > 0;
    }
}
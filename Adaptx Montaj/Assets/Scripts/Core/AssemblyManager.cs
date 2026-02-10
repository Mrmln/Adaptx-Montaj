using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AssemblyManager : MonoBehaviour
{
    public GlobalSettings settings;
    private Dictionary<string, PartAssembler> activeParts = new Dictionary<string, PartAssembler>();
    
    private List<System.Action> assemblySteps = new List<System.Action>();
    private List<System.Action> undoSteps = new List<System.Action>(); // Geri tuşu için basit tutuyoruz şimdilik
    
    public int currentStepIndex = -1;

    private string[] bigPartOrder = { "sag", "sol", "tab", "baz", "tav", "raf", "ark" }; 
    private string[] hardwareOrder = { "linco", "pim", "r-pim", "a-ayak", "civi" }; 

    // Küçük parça takılmayacak olanlar listesi
    private List<string> hardwareExcludedParts = new List<string>() { "raf", "ark" };

    void Start()
    {
        if (settings == null) settings = Resources.Load<GlobalSettings>("MainConfig");
        ScanScene();
        GenerateAssemblyScript();
    }

    void ScanScene()
    {
        activeParts.Clear();
        PartAssembler[] parts = GetComponentsInChildren<PartAssembler>(true);

        if (parts.Length == 0)
        {
            foreach (Transform child in transform.GetChild(0)) 
            {
                if (Regex.IsMatch(child.name, @"^\d+"))
                    child.gameObject.AddComponent<PartAssembler>();
            }
            parts = GetComponentsInChildren<PartAssembler>(true);
        }

        foreach (var p in parts)
        {
            p.Initialize(settings);
            // Regex: "1ark.001" -> "ark"
            string rawName = Regex.Match(p.name, @"\d+([a-zA-Z]+)").Groups[1].Value.ToLower();
            p.partCode = rawName;

            if (!activeParts.ContainsKey(rawName))
                activeParts.Add(rawName, p);

            // Başlangıçta hepsini gizle ve orijinal yerine çek (ne olur ne olmaz)
            p.gameObject.SetActive(false);
        }
    }

    void GenerateAssemblyScript()
    {
        assemblySteps.Clear();
        // undoSteps mantığını şimdilik basitleştirdik, sadece ileriye odaklanalım

        foreach (string partName in bigPartOrder)
        {
            if (!activeParts.ContainsKey(partName)) continue; 

            PartAssembler currentPart = activeParts[partName];

            // ADIM 1: "SAHNEYİ TEMİZLE VE PARÇAYI MERKEZE AL"
            assemblySteps.Add(() => {
                HideAllParts(); // Her şeyi gizle
                currentPart.gameObject.SetActive(true); // Sadece bunu aç
                currentPart.MoveToWorkbench(); // Parçayı (0,0,0)'a taşı
                
                // Kamerayı buraya odaklayabiliriz (Opsiyonel)
                Debug.Log($"---> PARÇA GELDİ: {partName.ToUpper()}");
            });

            // FİLTRE KONTROLÜ: Arkalık veya Raf ise vida adımlarını atla!
            if (hardwareExcludedParts.Contains(partName))
            {
                continue; // Döngünün başına dön, sonraki büyük parçaya geç
            }

            // ADIM 2: KÜÇÜK PARÇALAR
            foreach (string hwType in hardwareOrder)
            {
                if (currentPart.HasHardware(hwType))
                {
                    assemblySteps.Add(() => {
                        currentPart.InstallHardware(hwType);
                        Debug.Log($"+ Montaj: {hwType} takıldı.");
                    });
                }
            }
        }
    }

    void HideAllParts()
    {
        foreach (var kvp in activeParts)
        {
            kvp.Value.gameObject.SetActive(false);
        }
    }

    public void NextStep()
    {
        if (currentStepIndex < assemblySteps.Count - 1)
        {
            currentStepIndex++;
            assemblySteps[currentStepIndex].Invoke();
        }
    }

    public void PrevStep()
    {
        // Geri alma mantığı şu an "Exploded View" olmadığı için biraz karışık.
        // En basit yöntem: Index'i azaltıp o anki adımı tekrar çalıştırmak
        // Ama "Vida sökme" kodu yazmadığımız için şimdilik sadece index düşüyoruz.
        // İleride "Command Pattern" ile tam geri alma yapacağız.
        if (currentStepIndex > 0)
        {
            currentStepIndex--;
            // Şimdilik sadece log basalım, tam geri alma bir sonraki işimiz olsun
            Debug.Log("Geri gidildi (Tam görsel güncelleme için sistemi sıfırlamak gerekebilir)");
        }
    }
}
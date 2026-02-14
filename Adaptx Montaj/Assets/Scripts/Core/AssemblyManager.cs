using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Inspector'da görünecek ayar sınıfı
[System.Serializable]
public class PartRotationSetting
{
    public string partCode; // Örn: "sag"
    public Vector3 fixRotation; // Örn: (90, 0, 0)
}

public class AssemblyManager : MonoBehaviour
{
    public GlobalSettings settings;
    
    [Header("Rotasyon Düzeltme Ayarları")]
    [Tooltip("Hangi parça hangi açıda durmalı? Buradan ayarlayın.")]
    public List<PartRotationSetting> rotationFixes = new List<PartRotationSetting>();

    // --- Diğer Değişkenler ---
    private Dictionary<string, PartAssembler> activeParts = new Dictionary<string, PartAssembler>();
    private List<System.Action> assemblySteps = new List<System.Action>();
    public int currentStepIndex = -1;

    private string[] bigPartOrder = { "sag", "sol", "tab", "baz", "tav", "raf", "ark" }; 
    private string[] hardwareOrder = { "linco", "pim", "r-pim", "a-ayak", "civi", "flans" }; 
    private List<string> hardwareExcludedParts = new List<string>() { "raf", "ark" };

    void Start()
    {
        if (settings == null) settings = Resources.Load<GlobalSettings>("MainConfig");
        ScanScene();
        GenerateAssemblyScript();
    }

    void Update()
    {
        // Klavye Testi
        if (Input.GetKeyDown(KeyCode.RightArrow)) NextStep();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) PrevStep();
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
            // 1. Ayarları Yükle
            p.Initialize(settings);
            
            // 2. Kimliği Bul ("1sag" -> "sag")
            string rawName = Regex.Match(p.name, @"\d+([a-zA-Z]+)").Groups[1].Value.ToLower();
            p.partCode = rawName;

            // 3. Rotasyon Düzeltmesini Uygula (Varsa)
            ApplyRotationFix(p);

            // 4. Malzemeyi (Texture) Ata
            ApplyMaterial(p);

            if (!activeParts.ContainsKey(rawName))
                activeParts.Add(rawName, p);

            p.gameObject.SetActive(false);
        }
    }

    // --- YENİ EKLENEN FONKSİYON: Rotasyon Düzeltme ---
    void ApplyRotationFix(PartAssembler part)
    {
        // Listede bu parça kodu (Örn: "sag") için bir ayar var mı?
        PartRotationSetting fix = rotationFixes.Find(x => x.partCode == part.partCode);
        
        if (fix != null)
        {
            // Varsa, parçanın o anki rotasyonunu ez ve bizimkini yap
            // Dikkat: Bu işlem Local Rotation'ı değiştirir.
            part.transform.localRotation = Quaternion.Euler(fix.fixRotation);
            
            // PartAssembler içindeki "Orijinal Rotasyon" bilgisini de güncellemeliyiz
            // Çünkü reset atınca eski bozuk haline dönmesin.
            part.UpdateOriginalTransform(); 
        }
    }

    // --- YENİ EKLENEN FONKSİYON: Malzeme Atama ---
    void ApplyMaterial(PartAssembler part)
    {
        if (settings.mainWoodMaterial != null)
        {
            Renderer rend = part.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = settings.mainWoodMaterial;
            }
        }
    }

    // ... (GenerateAssemblyScript, NextStep, PrevStep, HideAllParts aynı kalıyor) ...
    // Sadece aşağıya kopyalamıyorum yer kaplamasın diye.
    // Eğer GenerateAssemblyScript fonksiyonunu sildiysen önceki koddan geri alabilirsin.
    
    // --- ÖNEMLİ: Kopyala Yapıştır yapacaksan GenerateAssemblyScript, NextStep, PrevStep, HideAllParts kodları önceki cevaptaki gibi kalmalı. ---
    void GenerateAssemblyScript()
    {
         assemblySteps.Clear();
         // ... (Önceki kodun aynısı) ...
         foreach (string partName in bigPartOrder)
        {
            if (!activeParts.ContainsKey(partName)) continue; 
            PartAssembler currentPart = activeParts[partName];

            assemblySteps.Add(() => {
                HideAllParts(); 
                currentPart.gameObject.SetActive(true); 
                currentPart.MoveToWorkbench(); // Orijine al
            });

            if (hardwareExcludedParts.Contains(partName)) continue;

            foreach (string hwType in hardwareOrder)
            {
                if (currentPart.HasHardware(hwType))
                {
                    assemblySteps.Add(() => currentPart.InstallHardware(hwType));
                }
            }
        }
    }

    void HideAllParts() { foreach (var kvp in activeParts) kvp.Value.gameObject.SetActive(false); }
    public void NextStep() { if (currentStepIndex < assemblySteps.Count - 1) { currentStepIndex++; assemblySteps[currentStepIndex].Invoke(); } }
    public void PrevStep() { if (currentStepIndex > 0) { currentStepIndex--; } }
}
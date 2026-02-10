using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    public Transform target; // Genelde (0,0,0) noktası olacak
    public float distance = 2.0f; // Başlangıç uzaklığı
    
    [Header("Hız Ayarları")]
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float zoomSpeed = 2.0f;

    [Header("Açı Sınırlamaları")]
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    // Kameranın o anki açısı
    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Başlangıçta 90 derece ve hafif yukarıdan bakması için manuel ayar
        // (Eğer sahnedeki açıyı kullanmak istersen burayı silebilirsin)
        x = 45f; // Hafif çapraz
        y = 30f; // Hafif yukarıdan
    }

    void LateUpdate()
    {
        // Mouse Sol veya Sağ tık basılıysa döndür
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // Zoom (Mouse Tekerleği)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, 0.5f, 10f); // Çok girmesin veya çok uzaklaşmasın

        // Pozisyonu güncelle
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + (target ? target.position : Vector3.zero);

        transform.rotation = rotation;
        transform.position = position;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
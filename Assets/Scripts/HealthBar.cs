using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Health targetHealth;
    public Image fillImage;

    private Transform targetTransform;
    private Vector3 offset = new Vector3(0f, 1f, 0f);
    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
        if (targetHealth != null)
            targetTransform = targetHealth.transform;
    }

    void Update()
    {
        if (targetHealth == null || fillImage == null) return;

        transform.position = targetTransform.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);

        float ratio = targetHealth.GetLifeRatio();
        fillImage.fillAmount = ratio;
    }
}

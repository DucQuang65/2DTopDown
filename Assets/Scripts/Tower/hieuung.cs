using UnityEngine;

public class LevelUpHandler : MonoBehaviour
{
    public TowerController tower;
    public ParticleSystem upgradeEffect;

    public void OnLevelUpClick()
    {
        if (tower == null || upgradeEffect == null)
        {
            Debug.LogWarning("Tower hoặc Effect chưa được gán!");
            return;
        }

        int beforeLevel = tower.currentLevel;
        tower.RequestUpgrade();

        if (tower.currentLevel > beforeLevel)
        {
            Debug.Log("Đã lên cấp, phát hiệu ứng!");
            upgradeEffect.Play();
        }
        else
        {
            Debug.Log("Không lên cấp (hết level hoặc không đủ vàng).");
        }
    }
}

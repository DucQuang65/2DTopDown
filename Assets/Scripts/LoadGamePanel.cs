using UnityEngine;

public class LoadGamePanel : MonoBehaviour
{
    public SaveLoadSlot[] saveSlots; // Gán từ Inspector
    public GameObject loadPanel; // Gán LoadPanel để bật/tắt

    private void Start()
    {
        RefreshSlots();
    }

    public void OpenLoadPanel()
    {
        loadPanel.SetActive(true);
        RefreshSlots();
    }

    public void CloseLoadPanel()
    {
        loadPanel.SetActive(false);
    }

    private void RefreshSlots()
    {
        foreach (var slot in saveSlots)
        {
            slot.RefreshSaveInfo();
        }
    }
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoryScroller : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform panelRect;         // Panel chứa text (có mask, cố định)
    public RectTransform storyTextRect;     // RectTransform của TextMeshProUGUI (chứa text)
    public TextMeshProUGUI storyText;       // TextMeshProUGUI hiển thị câu chuyện

    [Header("Scroll Settings")]
    public float scrollSpeed = 30f;          // Tốc độ scroll text

    private bool isScrolling = false;

    void Update()
    {
        if (!isScrolling)
            return;

        Vector2 pos = storyTextRect.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        storyTextRect.anchoredPosition = pos;

        float panelHeight = panelRect.rect.height;
        float textHeight = storyTextRect.rect.height;

        // Dừng scroll khi text chạy hết khỏi panel
        if (pos.y >= textHeight + panelHeight)
        {
            isScrolling = false;
            Debug.Log("Story scroll finished.");
        }
    }

    /// <summary>
    /// Bắt đầu hiển thị câu chuyện mới, reset vị trí text dưới panel và scroll lên
    /// </summary>
    /// <param name="fullText">Nội dung story</param>
    public void SetStory(string fullText)
    {
        if (storyText == null)
        {
            Debug.LogError("Bạn chưa gán TextMeshProUGUI cho storyText");
            return;
        }

        // Căn text về bên trái (bạn có thể tùy chỉnh alignment ở đây)
        storyText.alignment = TextAlignmentOptions.TopLeft;

        storyText.text = fullText;

        // Cập nhật layout để lấy đúng chiều cao text
        LayoutRebuilder.ForceRebuildLayoutImmediate(storyTextRect);

        float panelHeight = panelRect.rect.height;
        float textHeight = storyTextRect.rect.height;

        // Tính vị trí bắt đầu: đáy text trùng đáy panel
        float startPosY = -(textHeight - panelHeight);

        // Nếu text nhỏ hơn panel thì không dịch xuống dưới
        if (startPosY > 0)
            startPosY = 0;

        // Đặt vị trí text về dưới panel để bắt đầu scroll
        storyTextRect.anchoredPosition = new Vector2(storyTextRect.anchoredPosition.x, startPosY);

        isScrolling = true;
    }
}

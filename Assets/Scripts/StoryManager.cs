using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryScroller storyScroller; // Gán trong Inspector

    void Start()
    {
        string story = @"Thế giới Eldara từng là vùng đất phồn vinh, nơi ma thuật và loài người chung sống trong hòa bình nhờ vào hệ thống Trụ Bảo Hộ (Aether Beacons) – những cột trụ phép thuật cổ xưa được dựng lên để bảo vệ ranh giới giữa thế giới sống và cõi hỗn mang.

Nhưng sau Đêm Rạn Nứt, các trụ lần lượt sụp đổ. Ác ma, quái thú và những linh hồn sa đọa từ cõi khác tràn vào Eldara. Chúng tấn công bất cứ thứ gì có hơi thở, phá hủy các Aether Beacon để mở cổng cho bóng tối hoàn toàn chiếm lĩnh.

Chỉ còn một trụ cuối cùng. Nó nằm giữa một khu rừng cổ đại đã bị chiếm giữ – và bạn là người được chọn để bảo vệ nó cho đến khi viện binh tìm đến, hoặc chết cùng nó.

    Bạn vào vai một Guardian – người cuối cùng thuộc Hội Pháp Sư Bảo Hộ. Không còn đồng đội, bạn phải tự mình di chuyển khắp bản đồ, hạ gục quái vật đang tấn công từ mọi phía để bảo vệ trụ chính.";

        storyScroller.SetStory(story);
    }
}

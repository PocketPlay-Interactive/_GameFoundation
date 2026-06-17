public static class MemoryChecksum
{
    public const int FixedKey = 0xABCDEF;
    // Tạo checksum từ giá trị và key
    public static int GenerateChecksum(int value)
    {
        return (value ^ FixedKey) + 0xABCDEF;
    }

    // Kiểm tra giá trị với checksum
    public static bool VerifyChecksum(int value, int checksum)
    {
        return GenerateChecksum(value) == checksum;
    }

    /*
    ===========================
    HƯỚNG DẪN SỬ DỤNG
    ===========================
    1. Khi lưu giá trị:
        int value = 123;
        int key = 0x1A2B3C4D;
        int checksum = MemoryChecksum.GenerateChecksum(value, key);
        // Lưu cả value và checksum

    2. Khi đọc lại giá trị:
        // Đọc value và checksum đã lưu
        bool isValid = MemoryChecksum.VerifyChecksum(value, key, checksum);
        if (isValid)
        {
            // Giá trị hợp lệ
        }
        else
        {
            // Giá trị đã bị thay đổi hoặc gian lận
        }
    */
}
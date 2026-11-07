namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Helpers;

public static class AccountInput
{
    public static string NormalizeName(string? value) => value?.Trim() ?? "";
    public static string NormalizeEmail(string? value) => NormalizeName(value).ToLowerInvariant();

    public static string NormalizePhone(string? value)
    {
        var phone = NormalizeName(value).Replace(" ", "").Replace("-", "")
            .Replace(".", "").Replace("(", "").Replace(")", "");
        return phone.StartsWith("+84", StringComparison.Ordinal) ? "0" + phone[3..] : phone;
    }
}

namespace QuanLyThietBi_UNETI04_DHTI17A2HN.Models.Common;

public record RegistrationResult(bool Succeeded, IReadOnlyDictionary<string, string> Errors)
{
    public static RegistrationResult Success() => new(true, new Dictionary<string, string>());
    public static RegistrationResult Failure(IReadOnlyDictionary<string, string> errors) => new(false, errors);
}

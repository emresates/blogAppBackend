namespace BlogApi.Constants;

public static class ErrorMessages
{
    public static readonly Dictionary<string, string> Messages = new()
    {
        ["nameIsRequired"] = "İsim zorunludur.",
        ["nameMinLength"] = "İsim en az 2 karakter olmalıdır.",
        ["nameMaxLength"] = "İsim en fazla 50 karakter olabilir.",

        ["emailIsRequired"] = "Email zorunludur.",
        ["emailValidationFailed"] = "Geçerli bir email adresi giriniz.",

        ["passwordIsRequired"] = "Şifre zorunludur.",
        ["passwordMinLength"] = "Şifre en az 6 karakter olmalıdır.",
        ["passwordMaxLength"] = "Şifre en fazla 100 karakter olabilir.",

        ["categoryNameIsRequired"] = "Kategori adı zorunludur.",
        ["categoryNameMinLength"] = "Kategori adı en az 2 karakter olmalıdır.",
        ["categoryNameMaxLength"] = "Kategori adı en fazla 50 karakter olabilir.",

        ["titleIsRequired"] = "Başlık zorunludur.",
        ["titleMinLength"] = "Başlık en az 3 karakter olmalıdır.",
        ["titleMaxLength"] = "Başlık en fazla 150 karakter olabilir.",

        ["contentIsRequired"] = "İçerik zorunludur.",
        ["contentMinLength"] = "İçerik en az 10 karakter olmalıdır.",

        ["categoryMinLength"] = "En az bir kategori seçmelisiniz."
    };

    public static string GetMessage(string errCode)
    {
        return Messages.TryGetValue(errCode, out var message)
            ? message
            : "Geçersiz veri.";
    }
}
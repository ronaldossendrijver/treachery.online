namespace Treachery.Server;

public partial class GameHub
{
    private const int MaximumUserNameLength = 40;
    private const int MaximumPlayerNameLength = 40;
    private const int MaximumEmailLength = 254;
    private const int MaximumGameNameLength = 128;
    private const int PasswordHashLength = 64;

    private static ErrorType ValidateUserName(string? name)
    {
        var length = name?.Trim().Length ?? 0;
        return length <= 3 ? ErrorType.UserNameTooShort :
            length > MaximumUserNameLength ? ErrorType.UserNameTooLong : ErrorType.None;
    }

    private static ErrorType ValidatePlayerName(string? name)
    {
        var length = name?.Trim().Length ?? 0;
        return length <= 3 ? ErrorType.PlayerNameTooShort :
            length > MaximumPlayerNameLength ? ErrorType.PlayerNameTooLong : ErrorType.None;
    }

    private static ErrorType ValidateEmail(string? email)
    {
        var trimmedEmail = email?.Trim();
        if (trimmedEmail?.Length > MaximumEmailLength)
            return ErrorType.EmailTooLong;

        if (string.IsNullOrEmpty(trimmedEmail) ||
            !MailAddress.TryCreate(trimmedEmail, out var address) ||
            !string.Equals(address.Address, trimmedEmail, StringComparison.Ordinal) ||
            trimmedEmail.Any(char.IsControl))
            return ErrorType.InvalidEmail;

        return ErrorType.None;
    }

    private static ErrorType ValidatePasswordHash(string? hashedPassword, bool allowEmpty = false)
    {
        if (allowEmpty && string.IsNullOrEmpty(hashedPassword))
            return ErrorType.None;

        return hashedPassword is { Length: PasswordHashLength } && hashedPassword.All(char.IsAsciiHexDigit)
            ? ErrorType.None : ErrorType.InvalidPasswordHash;
    }

    private static ErrorType ValidateProfile(string? playerName, string? email, string? hashedPassword, bool allowEmptyPassword = false)
    {
        var error = ValidatePlayerName(playerName);
        if (error == ErrorType.None)
            error = ValidateEmail(email);
        if (error == ErrorType.None)
            error = ValidatePasswordHash(hashedPassword, allowEmptyPassword);
        return error;
    }
}

namespace CashVouchersManager.API.Configuration;

/// <summary>
/// Application settings configuration class
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the port for the application to listen on
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Gets or sets whether to use HTTPS redirection
    /// </summary>
    public bool UseHttpsRedirection { get; set; } = false;

    /// <summary>
    /// Gets or sets the authentication settings
    /// </summary>
    public AuthenticationSettings Authentication { get; set; } = new();
}

/// <summary>
/// Authentication settings configuration class
/// </summary>
public class AuthenticationSettings
{
    /// <summary>
    /// Gets or sets the username for basic authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for basic authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

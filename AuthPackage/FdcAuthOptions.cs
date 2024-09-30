using Microsoft.AspNetCore.Authentication.JwtBearer;

/// <summary>
/// Options for configuring custom JWT Bearer authentication
/// </summary>
public class FdcAuthOptions : JwtBearerOptions
{
    public Uri SigningKeysUri { get; set; } = default!;
}

using System.Data.Odbc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using DashboardServer.Models;

namespace DashboardServer.Services;

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
    {
        try
        {
            // Oracle接続情報を取得
            var dataSourceName = _configuration["OracleConnection:DataSourceName"];
            var userId = _configuration["OracleConnection:UserId"];
            var password = _configuration["OracleConnection:Password"];

            // ODBC接続文字列を構築
            var connectionString = $"DSN={dataSourceName};UID={userId};PWD={password}";

            using var connection = new OdbcConnection(connectionString);
            await connection.OpenAsync();

            // UserMasterテーブルからユーザー情報を取得
            var query = "SELECT ID, Passwd, StaffLevel FROM UserMaster WHERE ID = ?";
            using var command = new OdbcCommand(query, connection);
            command.Parameters.AddWithValue("@ID", request.Id);

            using var reader = await command.ExecuteReaderAsync();
            
            if (!await reader.ReadAsync())
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "ユーザーIDまたはパスワードが正しくありません。"
                };
            }

            var userMaster = new UserMaster
            {
                Id = reader.GetString(0),
                Passwd = reader.GetString(1),
                StaffLevel = reader.GetString(2)
            };

            // パスワードの検証（平文比較）
            if (userMaster.Passwd != request.Password)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "ユーザーIDまたはパスワードが正しくありません。"
                };
            }

            // StaffLevelの検証
            var allowedLevels = _configuration.GetSection("Authentication:StaffLevels").Get<List<string>>() ?? new List<string>();
            if (!allowedLevels.Contains(userMaster.StaffLevel))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "アクセス権限がありません。"
                };
            }

            // JWT トークンを生成
            var token = GenerateJwtToken(userMaster);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                UserId = userMaster.Id,
                StaffLevel = userMaster.StaffLevel,
                Message = "ログインに成功しました。"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "認証中にエラーが発生しました。");
            return new LoginResponse
            {
                Success = false,
                Message = "認証処理中にエラーが発生しました。"
            };
        }
    }

    private string GenerateJwtToken(UserMaster user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, user.StaffLevel),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

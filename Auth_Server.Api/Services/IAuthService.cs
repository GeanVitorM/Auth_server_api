using Authetication.Server.Api.DTOs;
using System.Threading.Tasks;

namespace Authetication.Server.Api.Services;

public interface IAuthService
{
    Task<string> Authenticate(string username, string password);
    string GeneratePasswordResetToken(UsuarioDto user);
    bool ValidatePasswordResetToken(UsuarioDto user, string token);
}
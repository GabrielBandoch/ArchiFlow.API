using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Domain.Clientes;
using ArchiFlow.Domain.Usuarios;
using Microsoft.IdentityModel.Tokens;

namespace ArchiFlow.Infrastructure.Services;

public class TokenService : ITokenService
{
    public string GenerateToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim("user_type", "staff")
        };

        return CreateToken(claims);
    }

    public string GenerateToken(Cliente cliente, Guid? projectId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
            new Claim(ClaimTypes.Name, cliente.Nome),
            new Claim(ClaimTypes.Email, cliente.Email),
            new Claim(ClaimTypes.Role, "Cliente"),
            new Claim("user_type", "client")
        };

        if (projectId.HasValue)
        {
            claims.Add(new Claim("projeto_id", projectId.Value.ToString()));
        }

        return CreateToken(claims);
    }

    private static string CreateToken(IEnumerable<Claim> claims)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("A chave de assinatura do JWT (JWT_SECRET) não foi configurada.");
        }

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "ArchiFlow.API";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "ArchiFlow.App";
        var expirationStr = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES");
        var expirationInMinutes = double.TryParse(expirationStr, out var exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

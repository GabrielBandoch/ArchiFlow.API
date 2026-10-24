using ArchiFlow.Domain.Usuarios;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Usuarios;

public class TokenRecuperacaoSenhaRepository : ITokenRecuperacaoSenhaRepository
{
    private readonly ArchiFlowDbContext _context;

    public TokenRecuperacaoSenhaRepository(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(TokenRecuperacaoSenha token)
    {
        await _context.TokensRecuperacaoSenha.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    public async Task<TokenRecuperacaoSenha?> ObterPorTokenHashAsync(string tokenHash)
    {
        return await _context.TokensRecuperacaoSenha
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task InvalidarParaEmailAsync(string email)
    {
        var tokens = await _context.TokensRecuperacaoSenha
            .Where(t => t.Email.ToLower() == email.Trim().ToLower() && t.UsadoEm == null)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.UsadoEm = DateTime.UtcNow;
        }

        if (tokens.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }
}


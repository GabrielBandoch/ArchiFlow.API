using ArchiFlow.Domain.Chat;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Chat;

public class MensagemChatRepository : IMensagemChatRepository
{
    private readonly ArchiFlowDbContext _context;

    public MensagemChatRepository(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MensagemChat>> GetByProjetoId(Guid projetoId, int take = 50)
    {
        return await _context.MensagensChat
            .Where(m => m.ProjetoId == projetoId)
            .OrderByDescending(m => m.CriadoEm)
            .Take(take)
            .OrderBy(m => m.CriadoEm)
            .ToListAsync();
    }

    public async Task<MensagemChat> Create(MensagemChat mensagem)
    {
        await _context.MensagensChat.AddAsync(mensagem);
        return mensagem;
    }

    public async Task MarcarComoLidas(Guid projetoId, Guid usuarioId)
    {
        var naoLidas = await _context.MensagensChat
            .Where(m => m.ProjetoId == projetoId && m.RemetenteId != usuarioId && !m.Lida)
            .ToListAsync();

        foreach (var msg in naoLidas)
        {
            msg.Lida = true;
        }
    }
}

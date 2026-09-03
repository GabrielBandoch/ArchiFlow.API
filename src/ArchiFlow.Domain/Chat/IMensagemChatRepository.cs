using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Chat;

public interface IMensagemChatRepository
{
    Task<IEnumerable<MensagemChat>> GetByProjetoId(Guid projetoId, int take = 50);
    Task<MensagemChat> Create(MensagemChat mensagem);
    Task MarcarComoLidas(Guid projetoId, Guid usuarioId);
}

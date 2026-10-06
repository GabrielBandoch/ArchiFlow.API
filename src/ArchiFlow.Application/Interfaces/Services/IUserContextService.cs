using System;
using System.Threading.Tasks;
using ArchiFlow.Domain.Usuarios;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IUserContextService
{
    Task<(Usuario Usuario, Guid EscritorioId)> ObterUsuarioContextoAsync();
}

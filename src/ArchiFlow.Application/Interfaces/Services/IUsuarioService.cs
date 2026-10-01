using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IUsuarioService
{
    Task<IEnumerable<MembroEquipeDto>> ObterEquipeAsync();
    Task<MembroEquipeDto?> ObterMembroPorIdAsync(Guid id);
    Task<MembroEquipeDto> ConvidarMembroAsync(ConvidarMembroEquipeCommand command);
    Task<MembroEquipeDto> AtualizarMembroAsync(Guid id, AtualizarMembroEquipeCommand command);
    Task<MembroEquipeDto> AlterarStatusMembroAsync(Guid id, AlterarStatusMembroCommand command);
    Task RedefinirSenhaMembroAsync(Guid id, RedefinirSenhaMembroCommand command);
    Task ExcluirMembroAsync(Guid id);
}

using System;
using System.Collections.Generic;
using System.IO;
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

    Task<PerfilUsuarioDto> ObterPerfilAsync(Guid usuarioId);
    Task<PerfilUsuarioDto> AtualizarPerfilAsync(Guid usuarioId, AtualizarPerfilCommand command);
    Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaCommand command);
    Task<string> AtualizarFotoPerfilAsync(Guid usuarioId, Stream stream, string fileName, string contentType);
    Task RemoverFotoPerfilAsync(Guid usuarioId);
}

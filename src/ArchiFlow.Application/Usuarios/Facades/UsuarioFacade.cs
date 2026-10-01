using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArchiFlow.Application.Interfaces.Facades;
using ArchiFlow.Application.Interfaces.Services;
using ArchiFlow.Application.Usuarios.Commands;
using ArchiFlow.Application.Usuarios.DTOs;

namespace ArchiFlow.Application.Usuarios.Facades;

public class UsuarioFacade : IUsuarioFacade
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioFacade(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public Task<IEnumerable<MembroEquipeDto>> ObterEquipeAsync() =>
        _usuarioService.ObterEquipeAsync();

    public Task<MembroEquipeDto?> ObterMembroPorIdAsync(Guid id) =>
        _usuarioService.ObterMembroPorIdAsync(id);

    public Task<MembroEquipeDto> ConvidarMembroAsync(ConvidarMembroEquipeCommand command) =>
        _usuarioService.ConvidarMembroAsync(command);

    public Task<MembroEquipeDto> AtualizarMembroAsync(Guid id, AtualizarMembroEquipeCommand command) =>
        _usuarioService.AtualizarMembroAsync(id, command);

    public Task<MembroEquipeDto> AlterarStatusMembroAsync(Guid id, AlterarStatusMembroCommand command) =>
        _usuarioService.AlterarStatusMembroAsync(id, command);

    public Task RedefinirSenhaMembroAsync(Guid id, RedefinirSenhaMembroCommand command) =>
        _usuarioService.RedefinirSenhaMembroAsync(id, command);

    public Task ExcluirMembroAsync(Guid id) =>
        _usuarioService.ExcluirMembroAsync(id);
}

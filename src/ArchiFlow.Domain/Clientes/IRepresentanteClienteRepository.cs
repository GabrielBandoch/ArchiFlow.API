using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArchiFlow.Domain.Clientes;

public interface IRepresentanteClienteRepository
{
    Task<IEnumerable<RepresentanteCliente>> ObterPorClienteIdAsync(Guid clienteId);
    Task<RepresentanteCliente?> ObterPorIdAsync(Guid id);
    Task<RepresentanteCliente?> ObterPorEmailAsync(string email);
    Task<RepresentanteCliente?> ObterPorTokenConviteHashAsync(string tokenHash);
    Task AdicionarAsync(RepresentanteCliente representante);
    Task AtualizarAsync(RepresentanteCliente representante);
    Task RemoverAsync(Guid id);
}


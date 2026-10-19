using ArchiFlow.Domain.Clientes;
using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiFlow.Infrastructure.Repositories.Clientes;

public class RepresentanteClienteRepository : IRepresentanteClienteRepository
{
    private readonly ArchiFlowDbContext _context;

    public RepresentanteClienteRepository(ArchiFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RepresentanteCliente>> ObterPorClienteIdAsync(Guid clienteId)
    {
        return await _context.RepresentantesClientes
            .Where(r => r.ClienteId == clienteId)
            .OrderBy(r => r.Nome)
            .ToListAsync();
    }

    public async Task<RepresentanteCliente?> ObterPorIdAsync(Guid id)
    {
        return await _context.RepresentantesClientes
            .Include(r => r.Cliente)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<RepresentanteCliente?> ObterPorEmailAsync(string email)
    {
        var normalized = email.Trim().ToLower();
        return await _context.RepresentantesClientes
            .Include(r => r.Cliente)
            .FirstOrDefaultAsync(r => r.Email.ToLower() == normalized);
    }

    public async Task<RepresentanteCliente?> ObterPorTokenConviteHashAsync(string tokenHash)
    {
        return await _context.RepresentantesClientes
            .Include(r => r.Cliente)
            .FirstOrDefaultAsync(r => r.ConviteTokenHash == tokenHash);
    }

    public async Task AdicionarAsync(RepresentanteCliente representante)
    {
        await _context.RepresentantesClientes.AddAsync(representante);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(RepresentanteCliente representante)
    {
        _context.RepresentantesClientes.Update(representante);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Guid id)
    {
        var rep = await _context.RepresentantesClientes.FindAsync(id);
        if (rep != null)
        {
            _context.RepresentantesClientes.Remove(rep);
            await _context.SaveChangesAsync();
        }
    }
}

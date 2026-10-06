using System;

namespace ArchiFlow.Application.Interfaces.Services;

public interface IOAuthStateService
{
    string GerarState(Guid usuarioId, Guid escritorioId);
    bool ValidarEConsumirState(string? state, Guid usuarioId, Guid escritorioId);
}

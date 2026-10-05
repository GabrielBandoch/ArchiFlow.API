using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using ArchiFlow.Application.Interfaces.Services;

namespace ArchiFlow.Infrastructure.Services;

public class OAuthStateService : IOAuthStateService
{
    private sealed record OAuthStateEntry(
        string State,
        Guid UsuarioId,
        Guid EscritorioId,
        DateTime ExpiraEm)
    {
        public bool Consumido { get; set; }
    }

    private static readonly ConcurrentDictionary<string, OAuthStateEntry> States = new();
    private static readonly TimeSpan DefaultValidity = TimeSpan.FromMinutes(15);

    public string GerarState(Guid usuarioId, Guid escritorioId)
    {
        LimparExpirados();

        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        var state = Convert.ToHexString(bytes).ToLowerInvariant();

        var entry = new OAuthStateEntry(
            state,
            usuarioId,
            escritorioId,
            DateTime.UtcNow.Add(DefaultValidity));

        States[state] = entry;
        return state;
    }

    public bool ValidarEConsumirState(string? state, Guid usuarioId, Guid escritorioId)
    {
        if (string.IsNullOrWhiteSpace(state))
            return false;

        if (!States.TryRemove(state.Trim(), out var entry))
            return false;

        if (entry.Consumido || entry.ExpiraEm < DateTime.UtcNow)
            return false;

        if (entry.UsuarioId != usuarioId || entry.EscritorioId != escritorioId)
            return false;

        entry.Consumido = true;
        return true;
    }

    private static void LimparExpirados()
    {
        var agora = DateTime.UtcNow;
        foreach (var kvp in States)
        {
            if (kvp.Value.ExpiraEm < agora || kvp.Value.Consumido)
            {
                States.TryRemove(kvp.Key, out _);
            }
        }
    }
}

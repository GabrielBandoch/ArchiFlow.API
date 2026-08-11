using System;

namespace ArchiFlow.Application.Leads.Commands;

public record CriarOrigemLeadCommand(
    string Descricao
);

public record AtualizarOrigemLeadCommand(
    Guid Id,
    string Descricao
);

public record DesativarOrigemLeadCommand(
    Guid Id
);

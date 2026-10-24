using System.Text.Json.Serialization;

namespace ArchiFlow.Application.Clientes.Commands;

public record AlterarStatusRepresentanteCommand(
    [property: JsonRequired] bool Ativo
);


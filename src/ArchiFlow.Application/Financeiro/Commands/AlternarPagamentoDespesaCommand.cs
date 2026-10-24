using System;

namespace ArchiFlow.Application.Financeiro.Commands;

public record AlternarPagamentoDespesaCommand(
    bool? Pago = null,
    DateTime? DataPagamento = null
);


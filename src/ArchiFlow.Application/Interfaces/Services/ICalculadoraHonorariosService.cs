using ArchiFlow.Application.Honorarios.DTOs;

namespace ArchiFlow.Application.Interfaces.Services;

public interface ICalculadoraHonorariosService
{
    SimulacaoResultadoDto Calcular(SimulacaoParametrosDto parametros);
}

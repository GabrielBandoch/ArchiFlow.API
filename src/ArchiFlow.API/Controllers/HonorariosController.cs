using ArchiFlow.Application.Honorarios.DTOs;
using ArchiFlow.Application.Interfaces.Facades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArchiFlow.API.Controllers;

[ApiController]
[Route("api/honorarios")]
[Authorize]
public class HonorariosController : ControllerBase
{
    private readonly IPropostaHonorarioFacade _facade;

    public HonorariosController(IPropostaHonorarioFacade facade)
    {
        _facade = facade;
    }

    [HttpPost("simular")]
    public ActionResult<SimulacaoResultadoDto> Simular([FromBody] SimulacaoParametrosDto parametros)
    {
        return Ok(_facade.Simular(parametros));
    }
}

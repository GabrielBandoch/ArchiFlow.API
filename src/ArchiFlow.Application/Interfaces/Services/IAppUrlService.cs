namespace ArchiFlow.Application.Interfaces.Services;

public interface IAppUrlService
{
    string ObterUrlBaseConfiavel(string? requestedOrigin = null);
}


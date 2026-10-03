using ArchiFlow.Application.Configuracoes.DTOs;
using ArchiFlow.Domain.Shared;

namespace ArchiFlow.Application.Configuracoes.Services;

public interface IConfiguracaoSistemaService
{
    Task<IEnumerable<OpcaoConfiguracaoDto>> ObterPorCategoriaAsync(string categoria);
    Task<IDictionary<string, IEnumerable<OpcaoConfiguracaoDto>>> ObterTodasAgrupadasAsync();
    Task<OpcaoConfiguracaoDto> SalvarOpcaoAsync(SalvarOpcaoConfiguracaoCommand command);
}

public class ConfiguracaoSistemaService : IConfiguracaoSistemaService
{
    private readonly IOpcaoConfiguracaoRepository _repository;

    public ConfiguracaoSistemaService(IOpcaoConfiguracaoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OpcaoConfiguracaoDto>> ObterPorCategoriaAsync(string categoria)
    {
        var opcoes = (await _repository.ObterPorCategoriaAsync(categoria)).ToList();

        if (opcoes.Count == 0)
        {
            var defaults = ObterDefaultsDaCategoria(categoria);
            foreach (var def in defaults)
            {
                await _repository.Create(def);
            }
            opcoes = defaults;
        }

        return opcoes.Select(MapearParaDto);
    }

    public async Task<IDictionary<string, IEnumerable<OpcaoConfiguracaoDto>>> ObterTodasAgrupadasAsync()
    {
        var todas = await _repository.ObterTodasAgrupadasAsync();
        return todas.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.Select(MapearParaDto)
        );
    }

    public async Task<OpcaoConfiguracaoDto> SalvarOpcaoAsync(SalvarOpcaoConfiguracaoCommand command)
    {
        OpcaoConfiguracao? opcao = null;
        if (command.Id.HasValue && command.Id.Value != Guid.Empty)
        {
            opcao = await _repository.GetById(command.Id.Value);
        }

        if (opcao == null)
        {
            opcao = new OpcaoConfiguracao
            {
                Id = Guid.NewGuid(),
                Categoria = command.Categoria.Trim(),
                Chave = command.Chave.Trim(),
                Rotulo = command.Rotulo.Trim(),
                SubRotulo = command.SubRotulo?.Trim(),
                Icone = command.Icone?.Trim(),
                Cor = command.Cor?.Trim(),
                CorFundo = command.CorFundo?.Trim(),
                Ordem = command.Ordem,
                DadosExtrasJson = command.DadosExtrasJson,
                Ativo = true
            };
            await _repository.Create(opcao);
        }
        else
        {
            opcao.Rotulo = command.Rotulo.Trim();
            opcao.SubRotulo = command.SubRotulo?.Trim();
            opcao.Icone = command.Icone?.Trim();
            opcao.Cor = command.Cor?.Trim();
            opcao.CorFundo = command.CorFundo?.Trim();
            opcao.Ordem = command.Ordem;
            opcao.DadosExtrasJson = command.DadosExtrasJson;
            await _repository.Update(opcao);
        }

        return MapearParaDto(opcao);
    }

    private static OpcaoConfiguracaoDto MapearParaDto(OpcaoConfiguracao o)
    {
        return new OpcaoConfiguracaoDto
        {
            Id = o.Id,
            Categoria = o.Categoria,
            Chave = o.Chave,
            Rotulo = o.Rotulo,
            SubRotulo = o.SubRotulo,
            Icone = o.Icone,
            Cor = o.Cor,
            CorFundo = o.CorFundo,
            Ordem = o.Ordem,
            Ativo = o.Ativo,
            DadosExtrasJson = o.DadosExtrasJson
        };
    }

    private static List<OpcaoConfiguracao> ObterDefaultsDaCategoria(string categoria)
    {
        var list = new List<OpcaoConfiguracao>();
        switch (categoria.ToLowerInvariant())
        {
            case "tipocompromisso":
                list.Add(new OpcaoConfiguracao { Categoria = "TipoCompromisso", Chave = "ReuniaoCliente", Rotulo = "Reunião com Cliente", Icone = "groups", Cor = "#765538", CorFundo = "#f6f0ea", Ordem = 1 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoCompromisso", Chave = "VisitaObra", Rotulo = "Visita à Obra", Icone = "construction", Cor = "#8d4f25", CorFundo = "#fbf2eb", Ordem = 2 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoCompromisso", Chave = "MedicaoTecnica", Rotulo = "Medição Técnica", Icone = "square_foot", Cor = "#426b58", CorFundo = "#eef5f1", Ordem = 3 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoCompromisso", Chave = "ApresentacaoProjeto", Rotulo = "Apresentação de Projeto", Icone = "slideshow", Cor = "#6e4359", CorFundo = "#f9eff4", Ordem = 4 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoCompromisso", Chave = "EntregaEtapa", Rotulo = "Entrega de Etapa", Icone = "task_alt", Cor = "#36663f", CorFundo = "#eef6f0", Ordem = 5 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoCompromisso", Chave = "Geral", Rotulo = "Geral / Outro", Icone = "event_note", Cor = "#5f5e5e", CorFundo = "#f0eded", Ordem = 6 });
                break;

            case "formapagamento":
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "Pix", Rotulo = "Pix", Ordem = 1 });
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "Transferencia", Rotulo = "Transferência Bancária (TED/DOC)", Ordem = 2 });
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "Boleto", Rotulo = "Boleto Bancário", Ordem = 3 });
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "CartaoCredito", Rotulo = "Cartão de Crédito", Ordem = 4 });
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "CartaoDebito", Rotulo = "Cartão de Débito", Ordem = 5 });
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "Dinheiro", Rotulo = "Dinheiro / Espécie", Ordem = 6 });
                list.Add(new OpcaoConfiguracao { Categoria = "FormaPagamento", Chave = "Outro", Rotulo = "Outro", Ordem = 7 });
                break;

            case "canalcontatolead":
                list.Add(new OpcaoConfiguracao { Categoria = "CanalContatoLead", Chave = "WhatsApp", Rotulo = "WhatsApp", SubRotulo = "Mensagem instantânea", Icone = "chat", Ordem = 1 });
                list.Add(new OpcaoConfiguracao { Categoria = "CanalContatoLead", Chave = "Ligacao", Rotulo = "Ligação Telefônica", SubRotulo = "Chamada de voz", Icone = "phone", Ordem = 2 });
                list.Add(new OpcaoConfiguracao { Categoria = "CanalContatoLead", Chave = "Email", Rotulo = "E-mail", SubRotulo = "Mensagem formal", Icone = "mail", Ordem = 3 });
                list.Add(new OpcaoConfiguracao { Categoria = "CanalContatoLead", Chave = "ReuniaoOnline", Rotulo = "Reunião Online", SubRotulo = "Google Meet / Teams", Icone = "video_call", Ordem = 4 });
                list.Add(new OpcaoConfiguracao { Categoria = "CanalContatoLead", Chave = "Presencial", Rotulo = "Reunião Presencial", SubRotulo = "Visita física", Icone = "person", Ordem = 5 });
                list.Add(new OpcaoConfiguracao { Categoria = "CanalContatoLead", Chave = "Outro", Rotulo = "Outro", SubRotulo = "Outro canal de contato", Icone = "more_horiz", Ordem = 6 });
                break;

            case "tipoprojeto":
                list.Add(new OpcaoConfiguracao { Categoria = "TipoProjeto", Chave = "Residencial", Rotulo = "Residencial", Ordem = 1 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoProjeto", Chave = "Comercial", Rotulo = "Comercial", Ordem = 2 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoProjeto", Chave = "Corporativo", Rotulo = "Corporativo", Ordem = 3 });
                list.Add(new OpcaoConfiguracao { Categoria = "TipoProjeto", Chave = "Interiores", Rotulo = "Design de Interiores", Ordem = 4 });
                break;

            case "roleequipe":
                list.Add(new OpcaoConfiguracao { Categoria = "RoleEquipe", Chave = "ArquitetoAdmin", Rotulo = "Arquiteto Sócio / Administrador", SubRotulo = "Acesso total a todos os módulos, financeiro e membros", Ordem = 1 });
                list.Add(new OpcaoConfiguracao { Categoria = "RoleEquipe", Chave = "ArquitetoColaborador", Rotulo = "Arquiteto Colaborador", SubRotulo = "Acesso a projetos, etapas e cronograma do estúdio", Ordem = 2 });
                list.Add(new OpcaoConfiguracao { Categoria = "RoleEquipe", Chave = "Estagiario", Rotulo = "Estagiário", SubRotulo = "Acesso às tarefas e projetos com supervisão", Ordem = 3 });
                list.Add(new OpcaoConfiguracao { Categoria = "RoleEquipe", Chave = "Financeiro", Rotulo = "Administrativo / Financeiro", SubRotulo = "Acesso ao controle de receitas, despesas e contratos", Ordem = 4 });
                list.Add(new OpcaoConfiguracao { Categoria = "RoleEquipe", Chave = "Gerente", Rotulo = "Gerente de Projetos", SubRotulo = "Acesso a gestão de equipes, clientes e projetos", Ordem = 5 });
                list.Add(new OpcaoConfiguracao { Categoria = "RoleEquipe", Chave = "Colaborador", Rotulo = "Colaborador Técnico", SubRotulo = "Acesso geral de produção", Ordem = 6 });
                break;
        }
        return list;
    }
}

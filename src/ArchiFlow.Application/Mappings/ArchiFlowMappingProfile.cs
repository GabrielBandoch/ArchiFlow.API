using AutoMapper;
using ArchiFlow.Domain.Projetos;
using ArchiFlow.Domain.Projetos.Enum;
using ArchiFlow.Application.Projetos.Commands;
using ArchiFlow.Application.Projetos.DTOs;

namespace ArchiFlow.Application.Mappings;

public class ArchiFlowMappingProfile : Profile
{
    public ArchiFlowMappingProfile()
    {
        CreateMap<CriarProjetoCommand, Projeto>()
            .ForMember(d => d.Id,       o => o.MapFrom(_ => Guid.NewGuid()))
            .ForMember(d => d.CriadoEm, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.Status,   o => o.MapFrom(_ => StatusProjetoEnum.Briefing))
            .ForMember(d => d.Etapas,   o => o.Ignore())
            .ForMember(d => d.AtualizadoEm, o => o.Ignore());

        CreateMap<AtualizarProjetoCommand, Projeto>()
            .ForMember(d => d.AtualizadoEm, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.CriadoEm,     o => o.Ignore())
            .ForMember(d => d.Etapas,       o => o.Ignore())
            .ForMember(d => d.ClienteId,    o => o.Ignore());
    }
}

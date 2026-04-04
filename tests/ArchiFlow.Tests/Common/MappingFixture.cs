using AutoMapper;
using ArchiFlow.Application.Mappings;

namespace ArchiFlow.Tests.Common;

public static class MappingFixture
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<ArchiFlowMappingProfile>());

        config.AssertConfigurationIsValid();

        return config.CreateMapper();
    }
}

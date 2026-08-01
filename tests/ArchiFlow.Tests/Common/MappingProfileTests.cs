using ArchiFlow.Application.Mappings;
using AutoMapper;
using Xunit;

namespace ArchiFlow.Tests.Common;

public class MappingProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_Is_Valid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ArchiFlowMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}

using ArchiFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArchiFlow.Tests.Common;

public static class TestDbContextFactory
{
    public static ArchiFlowDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ArchiFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ArchiFlowDbContext(options);
    }
}

using WebAPI.Services.GuidHelpers;

namespace WebAPI.Services;

public class GuidService : IGuidService
{
    private readonly IRandomGuidProvider _randomGuidProvider;
    private Guid _randomGuid { get; } = Guid.NewGuid();

    public GuidService(IRandomGuidProvider randomGuidProvider)
    {
        _randomGuidProvider = randomGuidProvider;
    }

    public void PrintSomething()
    {
        Console.WriteLine(_randomGuidProvider.RandomGuid);
    }
}

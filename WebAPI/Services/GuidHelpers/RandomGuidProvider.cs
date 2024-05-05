namespace WebAPI.Services.GuidHelpers;

public class RandomGuidProvider : IRandomGuidProvider
{
    public Guid RandomGuid { get; } = Guid.NewGuid();
}

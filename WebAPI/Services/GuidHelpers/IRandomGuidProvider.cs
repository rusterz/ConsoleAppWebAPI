namespace WebAPI.Services.GuidHelpers;

public interface IRandomGuidProvider
{
    Guid RandomGuid { get; }
}
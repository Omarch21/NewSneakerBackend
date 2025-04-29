using SneakerWebAPI.DTOs;

namespace SneakerWebAPI.Services.SneakerReleaseService
{
    public interface ISneakerReleaseService
    {
        Task<List<SneakerReleaseResponse>> GetReleases();
    }
}

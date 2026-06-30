using VentureOS.Domain.Cases;

namespace VentureOS.Application.Cases;

public interface ICaseRepository
{
    Task AddAsync(Case ventureCase, CancellationToken cancellationToken = default);

    Task<Case?> GetByIdAsync(Guid caseId, CancellationToken cancellationToken = default);
}
using VentureOS.Application.Research.ResearchCase;

public sealed record AcceptResearchPackageCommand(
    Guid CaseId,
    ResearchPackageDto ResearchPackage);

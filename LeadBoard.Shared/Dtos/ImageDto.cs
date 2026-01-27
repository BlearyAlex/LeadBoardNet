namespace LeadBoard.Shared.Dtos.Settings;

public record UploadImage(string Url, string PublicId);
public record ImageDetails(long Id, string Url, string PublicId);
public record ImageDetailsSummary(string Url, string PublicId);
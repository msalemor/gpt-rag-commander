namespace backend.Models;
public record ParseRequest(string text, string? method = "SK", int? maxTokensPerLine = 512, int? maxTokensPerParagraph = 1024, int? overlapTokens = 256);
public record ParseResponse(string text, int tokenCount);
public record ParseCompletion(List<ChunkInfo> chunks);
public record ChunkInfo(string text, int tokenCount);
public record UrlFileRequest(string url);
public record UrlFileResponse(string url, string fileName, string content);
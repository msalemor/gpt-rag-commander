namespace backend.Models;

public record FileInfo(string fileName, string content);
public record ParseRequest(string userName, FileInfo[] files, string? method = "SK", int? maxTokensPerLine = 512, int? maxTokensPerParagraph = 1024, int? overlapTokens = 256);
public record ParseResponse(string text, int tokenCount);
public record ParseCompletion(List<MemoryChunkInfo> chunks);
public record ChunkInfo(string text, int tokenCount);
public record MemoryChunkInfo(string userName, string fileName, string chunkId, string text, int tokenCount, string? embedding);
public record UrlFileRequest(string url);
public record UrlFileResponse(string url, string fileName, string content);

public record TokenizeRequest(string text, string? method = "SK", int? maxTokensPerLine = 512, int? maxTokensPerParagraph = 1024, int? overlapTokens = 256);
record QueryRequest(string collection, string prompt, int limit = 3, float relevance = 0.7f, int max_tokens = 500, float temperature = 0.3f);
record QueryResponse(string collection, string context, string completion, List<MemoryChunkInfo> memories);
record CompletionRequest(string prompt, int max_tokens = 500, float temperature = 0.3f);
record CompletionResponse(string text);
record ReconfigureRequest(string key, string uri, string model);
record MemoryInfo(string collection, string id, string text);
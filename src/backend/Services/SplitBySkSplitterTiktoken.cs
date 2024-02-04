using backend.Models;
using Microsoft.SemanticKernel.Text;

namespace backend.Services;
public class SplitBySkSplitterTiktoken : ITextSplitter<SKSplitterTiktoken>
{

    static List<string> Chunk(string content, int? maxTokensPerLine, int? maxTokensPerParagraph, int? overlapTokens = 256)
    {
#pragma warning disable SKEXP0055 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var lines = TextChunker.SplitPlainTextLines(content, maxTokensPerLine ?? 512, ITextSplitter<SKSplitter>.Counter);
        return TextChunker.SplitPlainTextParagraphs(lines, maxTokensPerParagraph ?? 1024, overlapTokens ?? 256, null, ITextSplitter<SKSplitter>.Counter);
#pragma warning restore SKEXP0055 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }
    public List<ChunkInfo>? ChunkText(string text, int? maxTokensPerLine, int? maxTokensPerParagraph, int? overlapTokens = 256)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        List<ChunkInfo> chunksInfo = new();
        var chunks = Chunk(text, maxTokensPerLine, maxTokensPerParagraph, overlapTokens);

        foreach (var chunk in chunks)
        {
            chunksInfo.Add(new ChunkInfo(chunk, ITextSplitter<SKSplitterTiktoken>.TikTokenEncoder.CountTokens(chunk)));
        }

        return chunksInfo;
    }
}
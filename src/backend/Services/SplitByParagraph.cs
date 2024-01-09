using backend.Models;

namespace backend.Services;
public class SplitByParagraph : ITextSplitter<ParagraphSplitter>
{
    public List<ChunkInfo>? ChunkText(string text, int? maxTokensPerLine, int? maxTokensPerParagraph, int? overlapTokens)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        var paragraphs = text.Split("\n\n");
        if (paragraphs.Length == 1)
        {
            paragraphs = text.Split("\r");
        }

        List<ChunkInfo> chunksInfo = new();
        foreach (var paragraph in paragraphs)
        {
            if (!string.IsNullOrEmpty(paragraph))
                chunksInfo.Add(new ChunkInfo(paragraph, ITextSplitter<SplitByParagraph>.TikTokenEncoder.CountTokens(paragraph)));
        }

        return chunksInfo;
    }
}
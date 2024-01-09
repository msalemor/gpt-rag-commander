using backend.Models;
using Tiktoken;

namespace backend.Services;
public interface ITextSplitter<T>
{
    private const string EncodingName = "gpt-4";//"cl100k_base";
    List<ChunkInfo>? ChunkText(string text, int? maxTokensPerLine, int? maxTokensPerParagraph, int? overlapTokens);
    public static Encoding TikTokenEncoder = Encoding.ForModel(EncodingName);
    public static int Counter(string input)
    {
        return TikTokenEncoder.CountTokens(input);
    }
}

public class SKSplitter { }
public class SKSplitterTiktoken { }
public class ParagraphSplitter { }
public class ParagraphWordsSplitter { }

public enum SplitterType
{
    SK,
    SKTiktoken,
    Paragraph,
    ParagraphWords
}

using System.Text;
using System.Text.RegularExpressions;
using backend.Models;

namespace backend.Services;
public class SplitByParagraphWords : ITextSplitter<ParagraphWordsSplitter>
{
    public static string CleanText(string content)
    {
        string pattern = @"[ ]{2,}";
        string replacement = " ";
        content = Regex.Replace(content, pattern, replacement);
        pattern = "\n{2,}";
        replacement = "<X-X>";
        content = Regex.Replace(content, pattern, replacement);
        pattern = @"\n [ \t]*\n";
        replacement = "<X-X>";
        content = Regex.Replace(content, pattern, replacement);
        //TODO: considere end of line cleanup
        // pattern = "\n{1,}";
        // replacement = "";
        // content = Regex.Replace(content, pattern, replacement);
        return content.Replace("<X-X>", "\n\n");
    }

    public static List<string> ChunkText(string text, int wordCount = 500, bool cleanup = true)
    {
        if (cleanup)
            text = CleanText(text);

        var paragraphs = text.Split("\n\n");
        List<string> chunks = new();
        StringBuilder chunk = new();
        foreach (var paragraph in paragraphs)
        {
            var paragraphWords = paragraph.Split(" ").Length;
            var chunkWords = chunk.ToString().Split(" ").Length;
            //Console.WriteLine($"Paragraph: {paragraph} Chunk: {chunk}");

            if ((paragraphWords + chunkWords) <= wordCount)
            {
                chunk.Append(paragraph + "\n\n");
            }
            else
            {
                chunks.Add(chunk.ToString());
                chunk.Clear();
                chunk.Append(paragraph);
            }
        }
        if (chunk.Length > 0)
            chunks.Add(chunk.ToString() + "\n\n");

        return chunks.ToList();
    }
    public List<ChunkInfo>? ChunkText(string text, int? maxTokensPerLine, int? maxTokensPerParagraph, int? overlapTokens)
    {
        var chunks = ChunkText(text, maxTokensPerParagraph ?? 500, true);
        List<ChunkInfo> chunksInfo = new();

        foreach (var chunk in chunks)
        {
            chunksInfo.Add(new ChunkInfo(chunk, ITextSplitter<SplitByParagraphWords>.TikTokenEncoder.CountTokens(chunk)));
        }

        return chunksInfo;
    }
}
using backend.Models;
using Microsoft.Data.Sqlite;
using Tiktoken;

namespace backend.Services;

public static class SqliteMemoryHelper
{
    public static string DB_MEMORY_PATH = "/home/alex/github/msalemor/gpt-rag-playground/src/backend/data/memorystore.db";

    public static void SetPath(string path)
    {
        DB_MEMORY_PATH = Path.Combine(path ?? "data/", "memorystore.db");
    }

    public async static Task<string> GetMemoryEmbedding(string collection, string key)
    {
        using var connection = new SqliteConnection("Data Source=" + DB_MEMORY_PATH);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT embedding FROM SKMemoryTable WHERE collection=@collection and key=@key";
        command.Parameters.AddWithValue("@collection", collection);
        command.Parameters.AddWithValue("@key", key);
        using var reader = await command.ExecuteReaderAsync();
        var files = new List<ChunkInfo>();
        while (await reader.ReadAsync())
        {
            return reader.GetString(0)[..100];
        }
        return string.Empty;
    }

    public async static Task<MemoryChunkInfo?> GetMemory(string? collection, string? key, string? content)
    {
        if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(content))
        {
            return null;
        }
        using var connection = new SqliteConnection("Data Source=" + DB_MEMORY_PATH);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM SKMemoryTable WHERE collection=@collection and key=@key";
        command.Parameters.AddWithValue("@collection", collection);
        command.Parameters.AddWithValue("@key", key);
        using var reader = await command.ExecuteReaderAsync();
        var files = new List<ChunkInfo>();
        var list = new List<MemoryChunkInfo>();
        while (await reader.ReadAsync())
        {
            var col = reader.GetString(0);
            var id = reader.GetString(1);
            var fileName = id;
            var metaJson = reader.GetString(2);
            var emb = reader.GetString(3)[..100];
            if (!string.IsNullOrEmpty(emb))
            {
                return new MemoryChunkInfo(col, fileName, id, content, TokenCounter(content), emb);
            }
        }
        return null;
    }

    public async static Task<int> DeleteMemoriesForUser(string collection)
    {
        using var connection = new SqliteConnection("Data Source=" + DB_MEMORY_PATH);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM SKMemoryTable WHERE collection=@collection";
        command.Parameters.AddWithValue("@collection", collection);
        return await command.ExecuteNonQueryAsync();
    }

    private static Encoding tikToken = Encoding.ForModel("gpt-4");

    public static int TokenCounter(string content)
    {
        return tikToken.CountTokens(content);
    }
}

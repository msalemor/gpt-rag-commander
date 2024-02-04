using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace backend.Services;

public static class KCVStore
{
    public const string DB_PATH = "/home/alex/github/msalemor/gpt-rag-playground/src/backend/data/kcvstore.db";

    public static async Task CreateStore()
    {
        using var connection = new SqliteConnection("Data Source=" + DB_PATH);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS chunkstore (username TEXT, filename TEXT, fileid TEXT, chunk TEXT)";
        await command.ExecuteNonQueryAsync();

        // Create index
        var command1 = connection.CreateCommand();
        command1.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS chunkstore_key ON chunkstore (username, filename, fileid)";
        await command1.ExecuteNonQueryAsync();
    }

    public static async Task<List<string>> GetAllUsers()
    {
        using var connection = new SqliteConnection("Data Source=" + DB_PATH);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT username FROM chunkstore ORDER BY username ASC";
        using var reader = await command.ExecuteReaderAsync();
        var users = new List<string>();
        while (await reader.ReadAsync())
        {
            users.Add(reader.GetString(0));
        }
        return users;
    }

    public record ChunkInfo(string name, string id, string text);

    public static async Task<List<ChunkInfo>> GetAllFilesForUser(string username)
    {
        using var connection = new SqliteConnection("Data Source=" + DB_PATH);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT DISTINCT filename FROM chunkstore WHERE username = @username ORDER BY filename ASC";
        command.Parameters.AddWithValue("@username", username);
        using var reader = await command.ExecuteReaderAsync();
        var files = new List<ChunkInfo>();
        while (await reader.ReadAsync())
        {
            files.Add(new ChunkInfo(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }
        return files;
    }

    public static async Task UpsertChunk(string username, string filename, string fileid, string chunk)
    {
        using var connection = new SqliteConnection("Data Source=" + DB_PATH);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT OR REPLACE INTO chunkstore (username, filename, fileid, chunk) VALUES (@username, @filename, @fileid, @chunk)";
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@filename", filename);
        command.Parameters.AddWithValue("@fileid", fileid);
        command.Parameters.AddWithValue("@chunk", chunk);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<int> DeleteChunksForUser(string username)
    {
        using var connection = new SqliteConnection("Data Source=" + DB_PATH);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM chunkstore WHERE username = @username";
        command.Parameters.AddWithValue("@username", username);
        return await command.ExecuteNonQueryAsync();
    }

    public static async Task<int> DeleteAllChunks()
    {
        using var connection = new SqliteConnection("Data Source=" + DB_PATH);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM chunkstore";
        return await command.ExecuteNonQueryAsync();
    }
}
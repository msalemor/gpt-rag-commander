using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using dotenv.net;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using System.Text;

// Load the either the .env file or the environment variables
DotEnv.Load();
var endpoint = Environment.GetEnvironmentVariable("OPENAI_API_URI")!;
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
var gptDeploymentName = Environment.GetEnvironmentVariable("OPENAI_API_GPT")!;
var adaDeploymentName = Environment.GetEnvironmentVariable("OPENAI_API_ADA")!;
var kcvDbStorePath = Environment.GetEnvironmentVariable("DB_KVC_STORE_PATH")!;
var memoryDbPath = Environment.GetEnvironmentVariable("DB_MEMORY_PATH")!;

// Create the KCVStore and the memory store
await KCVStore.CreateStore(kcvDbStorePath);
SqliteMemoryHelper.SetPath(memoryDbPath);

if (endpoint is null || apiKey is null || gptDeploymentName is null || adaDeploymentName is null)
{
    Console.WriteLine("Please set the OPENAI_API_URI, OPENAI_API_KEY, OPENAI_API_GPT, and OPENAI_API_ADA environment variables.");
    Environment.Exit(1);
}

// Create the web application
var builder = WebApplication.CreateBuilder(args);


var kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(gptDeploymentName, endpoint, apiKey)
            .Build();

IMemoryStore memoryStore = await SqliteMemoryStore.ConnectAsync(SqliteMemoryHelper.DB_MEMORY_PATH);
var memoryWithCustomDb = new MemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration(adaDeploymentName, endpoint, apiKey)
            .WithMemoryStore(memoryStore)
            .Build();

// // Create an embedding generator to use for semantic memory.
// var embeddingGenerator = new AzureOpenAITextEmbeddingGenerationService("ada", endpoint, apiKey);

// // The combination of the text embedding generator and the memory store makes up the 'SemanticTextMemory' object used to
// // store and retrieve memories.
// var textMemory = new SemanticTextMemory(memoryStore, embeddingGenerator);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(opt => opt.AddDefaultPolicy(builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITextSplitter<SKSplitter>, SplitBySkSplitter>();
builder.Services.AddSingleton<ITextSplitter<SKSplitterTiktoken>, SplitBySkSplitterTiktoken>();
builder.Services.AddSingleton<ITextSplitter<ParagraphSplitter>, SplitByParagraph>();
builder.Services.AddSingleton<ITextSplitter<ParagraphWordsSplitter>, SplitByParagraphWords>();
builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton(memoryWithCustomDb);

var tikToken = Tiktoken.Encoding.ForModel("gpt-4");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();

//app.UseAuthorization();
var group = app.MapGroup("/api/v1/content");

group.MapPost("/tokenize", ([FromBody] TokenizeRequest request) =>
{
    if (string.IsNullOrEmpty(request.text))
    {
        return Results.BadRequest(new { message = "Text cannot be empty" });
    }
    return Results.Ok(new ParseResponse(request.text, tikToken.Encode(request.text).Count));
})
.Produces<ParseResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostTokenize")
.WithOpenApi();

group.MapPost("/split", async ([FromBody] ParseRequest request, ITextSplitter<SKSplitter> skSplitter,
ITextSplitter<ParagraphSplitter> paragraphSplitter, ITextSplitter<ParagraphWordsSplitter> paragraphWordSplitter, ITextSplitter<SKSplitterTiktoken> skTiktokenSplitter,
ISemanticTextMemory memoryService) =>
{
    if (request.files is null || request.files.Length == 0)
    {
        return Results.BadRequest(new { message = "Files cannot be empty" });
    }


    // Delete all existing chunks for the user
    await SqliteMemoryHelper.DeleteMemoriesForUser(request.userName);
    await KCVStore.DeleteChunksForUser(request.userName);


    List<ChunkInfo> chunks = [];
    var memoryChunks = new List<MemoryChunkInfo>();
    foreach (var file in request.files)
    {
        var splittingMethod = request.method;

        if (string.IsNullOrEmpty(splittingMethod) || request.method == SplitterType.SK.ToString())
        {
            chunks = skSplitter.ChunkText(file.content, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
        }
        else if (splittingMethod == SplitterType.SKTiktoken.ToString())
        {
            chunks = skTiktokenSplitter.ChunkText(file.content, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
        }
        else if (splittingMethod == SplitterType.Paragraph.ToString())
        {
            chunks = paragraphSplitter.ChunkText(file.content, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
        }
        else if (splittingMethod == SplitterType.ParagraphWords.ToString())
        {
            chunks = paragraphWordSplitter.ChunkText(file.content, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
        }

        var id = 1;
        // Save the chunks to the database and memory

        var totalChunks = chunks.Count;
        foreach (var chunk in chunks)
        {
            var chunkId = $"{file.fileName}-{totalChunks}-{id}";
            // TODO: Save memory and save chunk
            await KCVStore.UpsertChunk(request.userName, file.fileName, chunkId, chunk.text);
            var result = await memoryService.SaveInformationAsync(request.userName, chunk.text, chunkId);
            // TODO: Get memory information including the vector
            var memory = await memoryService.GetAsync(request.userName, chunkId);
            var embedding = await SqliteMemoryHelper.GetMemoryEmbedding(request.userName, chunkId);

            memoryChunks.Add(new MemoryChunkInfo(request.userName, file.fileName, chunkId, chunk.text, chunk.tokenCount, embedding + " ..."));
            id++;
        }

    }
    return Results.Ok(memoryChunks);
})
.Produces<ParseCompletion>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostSplit")
.WithOpenApi();

group.MapPost("/load", async ([FromBody] UrlFileRequest request, HttpClient client) =>
{
    var fileName = Path.GetFileName(request.url);
    var ext = Path.GetExtension(fileName);
    switch (ext)
    {
        case ".txt":
        case ".md":
            var response = await client.GetAsync(request.url);
            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest(new { message = $"Cannot download file from {request.url}" });
            }
            var text = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(text))
            {
                return Results.BadRequest(new { message = $"The file is empty: {request.url}" });
            }
            return Results.Ok(new UrlFileResponse(request.url, fileName, text));
        default:
            return Results.BadRequest(new { message = $"The file format is not supported: {fileName}" });
    }
})
.Produces<UrlFileResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostLoad")
.WithOpenApi();


group.MapPost("/completion", async ([FromBody] CompletionRequest request, Kernel? kernel) =>
{
    if (string.IsNullOrEmpty(request.prompt))
    {
        return Results.BadRequest(new { message = "Prompt cannot be empty" });
    }

    if (kernel is null)
    {
        return Results.BadRequest(new { message = "Kernel cannot be null" });
    }

    var excuseFunction = kernel.CreateFunctionFromPrompt(request.prompt, new OpenAIPromptExecutionSettings() { MaxTokens = 100, Temperature = 0.4, TopP = 1 });

    var result = await kernel.InvokeAsync(excuseFunction, []);
    var content = result.GetValue<string>();

    return Results.Ok(new CompletionResponse(content ?? ""));
})
.Produces<UrlFileResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostCompletion")
.WithOpenApi();


// group.MapPost("/reconfigure", ([FromBody] ReconfigureRequest request) =>
// {
//     if (string.IsNullOrEmpty(request.uri) || string.IsNullOrEmpty(request.key) || string.IsNullOrEmpty(request.model))
//     {
//         return Results.BadRequest(new { message = "URI, API Key, and model cannot be empty" });
//     }


//     client = new(new Uri(request.uri), new AzureKeyCredential(request.key));
//     api_model = request.model;

//     return Results.Ok();
// })
// .Produces<UrlFileResponse>(StatusCodes.Status200OK)
// .Produces(StatusCodes.Status400BadRequest)
// .WithName("PostReconfigure")
// .WithOpenApi();

var ragGroup = app.MapGroup("/api/v1/rag");

ragGroup.MapPost("/memory", async ([FromBody] MemoryInfo memory, ISemanticTextMemory memoryService) =>
{
    if (string.IsNullOrEmpty(memory.collection) || string.IsNullOrEmpty(memory.id) || string.IsNullOrEmpty(memory.text))
    {
        return Results.BadRequest(new { message = "Collection, Memory ID, and Text cannot be empty" });
    }
    var result = await memoryService.SaveInformationAsync(memory.collection, memory.id, memory.text);
    return Results.Ok(result);
})
.WithName("PostMemory")
.WithOpenApi();

ragGroup.MapGet("/memory/{collection}/{id}", async (string collection, string id, ISemanticTextMemory memoryService) =>
{
    if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
    {
        return Results.BadRequest(new { message = "Collection and Memory ID cannot be empty" });
    }
    var result = await memoryService.GetAsync(collection, id);
    return Results.Ok(result);
})
.WithName("PostMemoryById")
.WithOpenApi();

ragGroup.MapGet("/chunks/{collection}", (string collection) =>
{
    return Results.Ok();
})
.WithName("GetMemoriesByCollection")
.WithOpenApi();

ragGroup.MapDelete("/memory/{connection}/{id}", async (string collection, string id, ISemanticTextMemory memoryService) =>
{
    if (string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(id))
    {
        return Results.BadRequest(new { message = "Collection and Memory ID cannot be empty" });
    }
    await memoryService.RemoveAsync("collection", "id");
    return Results.Ok();
})
.WithName("DeleteMemoryById")
.WithOpenApi();

ragGroup.MapGet("/collection", async () =>
{
    var collections = await memoryWithCustomDb.GetCollectionsAsync();
    return Results.Ok(collections);
})
.WithName("GetAllCollections")
.WithOpenApi();

ragGroup.MapPost("/query", async ([FromBody] QueryRequest query, ISemanticTextMemory memoryService) =>
{
    if (string.IsNullOrEmpty(query.collection) || string.IsNullOrEmpty(query.prompt))
    {
        return Results.BadRequest(new { message = "Collection and Prompt cannot be empty" });
    }

    var memories = memoryService.SearchAsync(query.collection, query.prompt, query.limit, query.relevance);

    StringBuilder contextSb = new();
    var memoryChunks = new List<MemoryChunkInfo>();
    await foreach (var memory in memories)
    {
        contextSb.AppendLine(memory?.Metadata?.Text);
        var memoryInfo = await SqliteMemoryHelper.GetMemory(query.collection, memory?.Metadata?.Id, memory?.Metadata?.Text);
        if (memoryInfo is not null)
        {
            memoryChunks.Add(memoryInfo);
        }
    }

    var prompt_template = @"System:
You are a corporate HR knowledge assistant. Answer the user's questions with the provided text only. If you cannot answer the question say, ""I do not have this information."" After every interaction say, ""NOTE: Please make sure to follow proper procedures and documentation.""

User:

Text: """"""
<CONTEXT>
""""""

<QUESTION>
";

    var prompt = prompt_template.Replace("<CONTEXT>", contextSb.ToString()).Replace("<QUESTION>", query.prompt);

    var excuseFunction = kernel.CreateFunctionFromPrompt(prompt,
        new OpenAIPromptExecutionSettings() { MaxTokens = query.max_tokens, Temperature = query.temperature, TopP = 1 });

    var result = await kernel.InvokeAsync(excuseFunction);
    var content = result.GetValue<string>();

    return Results.Ok(new QueryResponse(query.collection, contextSb.ToString(), content ?? "", memoryChunks, prompt));
})
.WithName("SearchCollection")
.WithOpenApi();

ragGroup.MapDelete("/reset/{collection}", async (string collection) =>
{
    if (string.IsNullOrEmpty(collection))
    {
        return Results.BadRequest(new { message = "Collection cannot be empty" });
    }
    var items = await SqliteMemoryHelper.DeleteMemoriesForUser(collection);
    return Results.Ok(new { message = $"Deleted {items} items from {collection}" });
})
.WithName("ResetUser")
.WithOpenApi();

//app.MapControllers();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();



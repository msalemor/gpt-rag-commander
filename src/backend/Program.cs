using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Azure.AI.OpenAI;
using Azure;
using dotenv.net;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

var api_URI = Environment.GetEnvironmentVariable("OPENAI_API_URI")!;
var api_key = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
var api_model = Environment.GetEnvironmentVariable("OPENAI_API_GPT")!;

Uri azureOpenAIResourceUri = new(api_URI);
AzureKeyCredential azureOpenAIApiKey = new(api_key);
OpenAIClient client;
client = new(azureOpenAIResourceUri, azureOpenAIApiKey);

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

group.MapPost("/tokenize", ([FromBody] ParseRequest request) =>
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

group.MapPost("/split", ([FromBody] ParseRequest request, ITextSplitter<SKSplitter> skSplitter,
ITextSplitter<ParagraphSplitter> paragraphSplitter, ITextSplitter<ParagraphWordsSplitter> paragraphWordSplitter, ITextSplitter<SKSplitterTiktoken> skTiktokenSplitter) =>
{
    if (string.IsNullOrEmpty(request.text))
    {
        return Results.BadRequest(new { message = "Text cannot be empty" });
    }

    List<ChunkInfo> chunks = new();
    var splittingMethod = request.method;

    if (string.IsNullOrEmpty(splittingMethod) || request.method == SplitterType.SK.ToString())
    {
        chunks = skSplitter.ChunkText(request.text, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
    }
    else if (splittingMethod == SplitterType.SKTiktoken.ToString())
    {
        chunks = skTiktokenSplitter.ChunkText(request.text, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
    }
    else if (splittingMethod == SplitterType.Paragraph.ToString())
    {
        chunks = paragraphSplitter.ChunkText(request.text, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
    }
    else if (splittingMethod == SplitterType.ParagraphWords.ToString())
    {
        chunks = paragraphWordSplitter.ChunkText(request.text, request.maxTokensPerLine, request.maxTokensPerParagraph, request.overlapTokens) ?? new List<ChunkInfo>();
    }

    return Results.Ok(new ParseCompletion(chunks));
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


group.MapPost("/completion", async ([FromBody] CompletionRequest request) =>
{
    if (string.IsNullOrEmpty(request.prompt))
    {
        return Results.BadRequest(new { message = "Prompt cannot be empty" });
    }

    var chatCompletionsOptions = new ChatCompletionsOptions()
    {
        DeploymentName = api_model, // Use DeploymentName for "model" with non-Azure clients
        Messages =
        {
            // User messages represent current or historical input from the end user
            new ChatRequestUserMessage(request.prompt),
        },
        MaxTokens = request.max_tokens,
        Temperature = request.temperature
    };
    Response<ChatCompletions> response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
    ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
    return Results.Ok(new CompletionResponse(responseMessage.Content));
})
.Produces<UrlFileResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostCompletion")
.WithOpenApi();


group.MapPost("/reconfigure", ([FromBody] ReconfigureRequest request) =>
{
    if (string.IsNullOrEmpty(request.uri) || string.IsNullOrEmpty(request.key) || string.IsNullOrEmpty(request.model))
    {
        return Results.BadRequest(new { message = "URI, API Key, and model cannot be empty" });
    }
    client = new(new Uri(request.uri), new AzureKeyCredential(request.key));
    api_model = request.model;

    return Results.Ok();
})
.Produces<UrlFileResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostReconfigure")
.WithOpenApi();


//app.MapControllers();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

record CompletionRequest(string prompt, int max_tokens = 500, float temperature = 0.3f);
record CompletionResponse(string text);
record ReconfigureRequest(string key, string uri, string model);
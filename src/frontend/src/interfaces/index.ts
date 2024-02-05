interface ISettings {
    maxTokensPerLine: string
    maxTokensPerParagraph: string
    overlapTokens: string
    wordCount: string
    method: string
    chunks: string
    relevance: string
    prompt: string
    max_tokens: string
    temperature: string
    url: string
}

interface IChunk {
    text: string
    tokenCount: number
}

interface IParseCompletion {
    chunks: IChunk[]
}

interface IMemoryChunkInfo {
    userName: string
    fileName: string
    chunkId: string
    text: string
    tokenCount: number
    embedding: string
}

interface IQueryResponse {
    collection: string
    context: string
    completion: string
    memories: IMemoryChunkInfo[]
    fullPrompt: string
}
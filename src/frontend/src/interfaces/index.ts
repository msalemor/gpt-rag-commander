interface ISettings {
    maxTokensPerLine: string
    maxTokensPerParagraph: string
    overlapTokens: string
    wordCount: string
    method: string
    chunks: string
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
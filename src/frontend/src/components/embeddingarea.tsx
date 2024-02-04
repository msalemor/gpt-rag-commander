import { For } from "solid-js"
import Embedding from "./embedding"

const EmbeddingsArea = (props: { title: string, memories: IMemoryChunkInfo[], bg_color: string }) => {
    return (
        <>
            <div class={"space-x-2 text-white p-1 " + props.bg_color}>
                <label class="uppercase text-sm">{props.title} </label> <span title="Total Chunks" class='px-2 bg-slate-800 text-white rounded-xl'>{props.memories.length}</span> -
                <span title="Total Tokens" class='px-2 bg-slate-800 text-white rounded-xl'>{props.memories.reduce((acc, chunk) => acc + chunk.tokenCount, 0)}</span>
            </div>
            <div class="overflow-auto">
                <For each={props.memories}>
                    {(chunk) => (
                        <Embedding chunkId={chunk.chunkId} tokenCount={chunk.tokenCount} text={chunk.text} embedding={chunk.embedding} />
                    )}
                </For>
            </div>
        </>
    )
}
export default EmbeddingsArea
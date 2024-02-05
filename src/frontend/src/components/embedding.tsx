const Embedding = (props: any) => {
    return (
        <div class="flex flex-col mb-2 rounded-lg p-2 space-y-3 border-2 border-slate-300 hover:border-2 hover:border-slate-800 shadow">
            <div class="space-x-1">
                <span class="text-sm font-bold uppercase">Embedding: </span>
                <span class='px-2 bg-blue-900 text-white rounded-xl'>{props.chunkId}</span>
                <span class="text-sm font-bold uppercase">Tokens: </span>
                <span class='px-2 bg-blue-900 text-white rounded-xl'>{props.tokenCount}</span>
            </div>
            <hr class="border-black" />
            {/* <span class="">{chunk.text}</span> */}
            <label class="uppercase font-semibold text-sm">Content:</label>
            <textarea class="bg-slate-200 p-2" rows={10} value={props.text} readOnly></textarea>
            <label class="uppercase font-semibold text-sm">Embedding:</label>
            <textarea class="bg-yellow-200 p-2" rows={2} value={props.embedding} readOnly></textarea>
        </div>
    );
}
export default Embedding


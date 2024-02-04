const Embedding = (props: any) => {
    return (
        <div class="flex flex-col mb-2 rounded-lg p-2 space-y-3 border-2 border-slate-300 hover:border-2 hover:border-slate-800 shadow">
            <div>
                <span class="text-sm font-bold uppercase">Embedding: </span>{props.chunkId} <span class="text-sm font-bold uppercase">Tokens: </span>{props.tokenCount}
            </div>
            <hr class="border-black" />
            {/* <span class="">{chunk.text}</span> */}
            <textarea class="bg-slate-200 p-2" rows={10} value={props.text} readOnly></textarea>
            <textarea class="bg-yellow-200 p-2" rows={2} value={props.embedding} readOnly></textarea>
        </div>
    );
}
export default Embedding
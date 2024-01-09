export function Information() {
    return (
        <div class="bg-yellow-100 text-black p-2">
            <p>This tool is designed to help to understand tokens, text chunking, setting context in a Prompt, and to gage the quality of a response based on different chunking settings for a document. To use this tool:</p>
            <div class="p-2">
                <ul>
                    <li>- First paste the document content into the <strong>TEXT</strong> tab textarea, set the chunking method and sizes, and then click the <strong>Chunk</strong> button.</li>
                    <li>- The document will be split into chunks and displayed to the right.</li>
                    <li>- You will be navigated to the Context area. The chunks will be added to the <strong>CONTEXT</strong> textarea in the CONTEXT tab based on the desired chunk limit.</li>
                    <li>- Click on the Prompt tab, enter the Prompt, max tokens, and temperature, then click the <strong>Submit</strong> button. The additional context will be added to the prompt automatically.</li>
                    <li>- Upon receiving a response, you will be navigated to the <strong>COMPLETION</strong> tab's textarea.</li>
                </ul>
            </div>
        </div>
    )
}
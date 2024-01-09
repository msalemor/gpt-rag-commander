import axios from "axios"
import { For, createSignal } from "solid-js"
import { encode } from 'gpt-tokenizer'
import { makePersisted } from "@solid-primitives/storage"
import { sample1, sample2, samplePrompt } from "./components"
import { Header } from "./components/Header"
import { Information } from "./components/Information"

const SplittingMethod = {
  SK: "SK",
  SKTIKTOKEN: "SKTiktoken",
  Paragraph: "Paragraph",
  ParagraphWords: "ParagraphWords"
}

const DefaultSettings: ISettings = {
  maxTokensPerLine: "100",
  maxTokensPerParagraph: "1024",
  overlapTokens: "0",
  wordCount: "512",
  method: SplittingMethod.SKTIKTOKEN,
  chunks: "3",
  prompt: "",
  max_tokens: "500",
  temperature: "0.3",
  url: ""
}

const URI_BASE = "http://localhost:5096/"
const URI_CHUNK = URI_BASE + "api/v1/content/split"
const URI_LOAD = URI_BASE + "api/v1/content/load"
const URI_COMPLETION = URI_BASE + "api/v1/content/completion"

function App() {
  const [settings, setSettings] = makePersisted(createSignal<ISettings>(DefaultSettings))
  const [text, setText] = makePersisted(createSignal(''))
  const [text2, setText2] = makePersisted(createSignal(''))
  const [tokens, setTokens] = createSignal(0)
  const [tokensContext, setTokensContext] = createSignal(0)
  const [tokensPrompt, setTokensPrompt] = createSignal(0)
  const [tokensCompletion, setTokensCompletion] = createSignal(0)
  const [parseCompletion, setParseCompletion] = createSignal<IParseCompletion>({ chunks: [] })
  const [context, setContext] = createSignal('')
  const [completion, setCompletion] = createSignal('')
  const [processing, setProcessing] = createSignal(false)
  const [submitLabel, setSubmitLabel] = createSignal("Submit")
  const [useContext, _] = createSignal(true)
  const [tab, setTab] = createSignal('chunk')

  const getTokenCountAfterTyping = (text: string, control: string) => {
    if (control === "chunk") {
      setText(text)
      setTokens((encode(text)).length);
    }
    if (control === "chunk2") {
      setText2(text)
      //setTokens((encode(text)).length);
    }
    if (control === "context") {
      setContext(text)
      setTokensContext(encode(text).length);
    }
    if (control === "prompt") {
      setSettings({ ...settings(), prompt: text })
      setTokensPrompt((encode(text + context())).length);
    }
  }

  const UpdateTokenCounts = () => {
    setTokens((encode(text())).length);
    setTokensPrompt((encode(settings().prompt + context())).length);
    setTokensContext(encode(context()).length);
    setTokensCompletion(encode(completion()).length);
  }

  const LoadContext = () => {
    const totalChunks = parseInt(settings().chunks)
    const chunks = parseCompletion().chunks
    if (totalChunks > 0 && parseCompletion().chunks.length > 0) {
      let chunkText = ""
      for (let i = 0; i < totalChunks; i++) {
        if (i == chunks.length) break;
        chunkText += chunks[i].text + "\n\n"
      }
      setContext(chunkText)
      UpdateTokenCounts()
    }
  }

  const Process = async () => {
    if (processing()) return
    setProcessing(true)
    setParseCompletion({ chunks: [] })
    getTokenCountAfterTyping(text(), "chunk")
    const maxTokensPerParagraph = settings().method == SplittingMethod.Paragraph ? parseInt(settings().wordCount) : parseInt(settings().maxTokensPerLine)
    const payload = {
      text: text(),
      maxTokensPerLine: parseInt(settings().maxTokensPerLine),
      maxTokensPerParagraph,
      overlapTokens: parseInt(settings().overlapTokens),
      method: settings().method
    }
    try {
      const resp = await axios.post(URI_CHUNK, payload)
      const data: IParseCompletion = resp.data
      console.info(data)
      setParseCompletion(data)
      LoadContext()
      UpdateTokenCounts()
      setTab("context")
    } catch (err) {
      console.error(err)
    } finally {
      setProcessing(false)
    }
  }

  const LoadAndProcess = async () => {
    if (processing()) return
    setText(sample1)
    setText2(sample2)
    await Process()
  }

  const LoadSamplePrompt = () => {
    setSettings({ ...settings(), prompt: samplePrompt })
    UpdateTokenCounts()
  }

  const LoadFile = async () => {
    if (processing()) return
    setProcessing(true)
    const payload = { url: settings().url }
    setSettings({ ...settings(), url: "Loading ..." })
    try {
      const resp = await axios.post(URI_LOAD, payload)
      const content = resp.data.content;
      setText(content)
      UpdateTokenCounts()
    } catch (err) {
      console.error(err)
    } finally {
      setSettings({ ...settings(), url: "" })
      setProcessing(false)
    }
  }

  const Submit = async () => {
    if (processing()) return
    // Use a template form
    let prompt = settings().prompt
    if (prompt.indexOf("<CONTEXT>") > -1) {
      prompt = prompt.replace("<CONTEXT>", context())
    } else {
      prompt = prompt + "\n\n" + context()
    }
    // Maybe the user decides not to use the context
    if (!useContext()) {
      prompt = settings().prompt
    }
    const payload = {
      prompt,
      max_tokens: parseInt(settings().max_tokens),
      temperature: parseFloat(settings().temperature)
    }
    console.info(`Submit: ${JSON.stringify(payload)}`)
    setProcessing(true)
    try {
      setSubmitLabel("Busy...")
      const resp = await axios.post(URI_COMPLETION, payload)
      const data = resp.data
      console.info(`Completion: ${JSON.stringify(data)}`)
      setCompletion(data.text)
      setTab("completion")
      UpdateTokenCounts()
    } catch (err) {
      console.error(err)
    } finally {
      setProcessing(false)
      setSubmitLabel("Submit")
    }
  }

  return (
    <>
      <Header />
      <nav class="flex flex-row flex-wrap space-x-2 p-2 bg-blue-800 text-white">
        <div class="space-x-2 space-y-2">
          <label>Document URL:</label>
          <input
            value={settings().url}
            onInput={(e) => setSettings({ ...settings(), url: e.currentTarget.value })}
            class="px-1 md:w-[500px] w-[90%] text-black" type="text" />
          <button
            onClick={LoadFile}
            class="p-2 bg-green-800 hover:bg-green-700 font-semibold rounded">Load</button>
        </div>
      </nav>

      <div class="px-3 mt-3">
        <nav class="flex flex-row w-full flex-wrap space-x-[2px]">
          <button
            onClick={() => setTab("chunk")}
            class={`p-2 hover:underline hover:text-black ${tab() == "chunk" ? "border-black border-2 underline text-black" : "border"} font-semibold`}>Text&nbsp;
            <span title="Total Tokens" class='px-2 bg-blue-900 text-white rounded-xl'>{tokens()}</span>
          </button>
          <button
            onClick={() => setTab("prompt")}
            class={`p-2 hover:underline hover:text-black ${tab() == "prompt" ? "border-black border-2 underline text-black" : "border"} font-semibold`}>Prompt&nbsp;
            <span title="Total Tokens" class='px-2 bg-blue-900 text-white rounded-xl'>{tokensPrompt()}</span>
          </button>
          <button
            onClick={() => setTab("context")}
            class={`p-2 hover:underline hover:text-black ${tab() == "context" ? "border-black border-2 underline text-black" : "border"} font-semibold`}>Context&nbsp;
            <span title="Total Tokens" class='px-2 bg-blue-900 text-white rounded-xl'>{tokensContext()}</span>
          </button>
          <button
            onClick={() => setTab("completion")}
            class={`p-2 hover:underline hover:text-black ${tab() == "completion" ? "border-black border-2 underline text-black" : "border"} font-semibold`}>Completion&nbsp;
            <span title="Total Tokens" class='px-2 bg-blue-900 text-white rounded-xl'>{tokensCompletion()}</span>
          </button>
        </nav>
      </div>
      <div class="px-3 flex flex-row flex-wrap w-full">
        <div class="basis-full md:basis-3/4 border border-black">
          {/* Input Text */}
          <section hidden={tab() !== "chunk"}>
            <div class="flex flex-row flex-wrap space-x-2 p-2 bg-blue-900 text-white">
              <label>Method:</label>
              <div class="space-x-2">
                <input type='radio' name="method"
                  checked={settings().method === SplittingMethod.SKTIKTOKEN}
                  onInput={(e) => setSettings({ ...settings(), method: e.currentTarget.value })}
                  value={SplittingMethod.SKTIKTOKEN} />
                <label>Tokens</label>
              </div>
              <div class="space-x-2">
                <input type='radio' name="method"
                  checked={settings().method === SplittingMethod.SK}
                  onInput={(e) => setSettings({ ...settings(), method: e.currentTarget.value })}
                  value={SplittingMethod.SK} />
                <label>SK Counter</label>
              </div>
              <div class="space-x-2">
                <input type='radio' name="method"
                  checked={settings().method === SplittingMethod.Paragraph}
                  onInput={(e) => setSettings({ ...settings(), method: e.currentTarget.value })}
                  value={SplittingMethod.Paragraph} />
                <label>Paragraphs</label>
              </div>
              <div class="space-x-2">
                <input type='radio' name="method"
                  checked={settings().method === SplittingMethod.ParagraphWords}
                  onInput={(e) => setSettings({ ...settings(), method: e.currentTarget.value })}
                  value={SplittingMethod.ParagraphWords} />
                <label>Paragraph/Words</label>
              </div>
            </div>
            <div class="flex flex-row flex-wrap space-x-2 p-2 bg-blue-900 text-white">
              <div class={"space-x-2"} hidden={settings().method == SplittingMethod.Paragraph || settings().method == SplittingMethod.ParagraphWords}>
                <label>Tokens/Line:</label>
                <input
                  value={settings().maxTokensPerLine}
                  onInput={(e) => setSettings({ ...settings(), maxTokensPerLine: e.currentTarget.value })}
                  class="w-20 px-1 text-black" type="text" />
              </div>
              <div class="space-x-2" hidden={settings().method == SplittingMethod.Paragraph || settings().method == SplittingMethod.ParagraphWords}>
                <label>Tokens/Paragraph:</label>
                <input
                  value={settings().maxTokensPerParagraph}
                  onInput={(e) => setSettings({ ...settings(), maxTokensPerParagraph: e.currentTarget.value })}
                  class="w-20 px-1 text-black" type="text" />
              </div>
              <div class="space-x-2" hidden={settings().method == SplittingMethod.Paragraph || settings().method == SplittingMethod.ParagraphWords}>
                <label>Overlap Tokens:</label>
                <input
                  value={settings().overlapTokens}
                  onInput={(e) => setSettings({ ...settings(), overlapTokens: e.currentTarget.value })}
                  class="w-20 px-1 text-black" type="text" />
              </div>
              <div class="space-x-2" hidden={settings().method == SplittingMethod.Paragraph || settings().method == SplittingMethod.SK || settings().method == SplittingMethod.SKTIKTOKEN}>
                <label>Word Count:</label>
                <input
                  value={settings().wordCount}
                  onInput={(e) => setSettings({ ...settings(), wordCount: e.currentTarget.value })}
                  class="w-20 px-1 text-black" type="text" />
              </div>
            </div>
            <div class="flex flex-col bg-white space-y-2 p-2">
              <div class="flex flex-row flex-wrap">
                <textarea
                  class="border border-black p-2 round-lg w-full md:w-[calc(50%-5px)] md:mr-[5px]"
                  value={text()}
                  onInput={(e) => { getTokenCountAfterTyping(e.currentTarget.value, "chunk") }}
                  rows={20}>
                </textarea>
                <textarea
                  value={text2()}
                  onInput={(e) => { getTokenCountAfterTyping(e.currentTarget.value, "chunk2") }}
                  class="border border-black p-2 round-lg w-full md:w-1/2"
                  rows={20}>
                </textarea>
              </div>
              <div class="space-x-2">
                <button
                  onClick={Process}
                  class="w-24 p-2 bg-blue-900 hover:bg-blue-900 text-white font-semibold">Embed</button>
                <button
                  onClick={LoadAndProcess}
                  class="p-2 bg-green-800 hover:bg-green-700 text-white font-semibold">Load Samples & Embed</button>
              </div>
              <Information />
            </div>
          </section>
          <section hidden={tab() !== "context"}>
            <div class="bg-blue-900 p-1 text-white space-x-1">
              <label class='uppercase'>Chunks:</label>
              <input class="px-1 w-10 text-black"
                value={settings().chunks}
                onInput={(e) => setSettings({ ...settings(), chunks: e.currentTarget.value })}
              />
            </div>
            <div class="flex flex-col space-y-2 mt-2 p-2">
              <textarea
                class="border border-black p-2 round-lg w-1/2"
                value={context()}
                onInput={(e) => getTokenCountAfterTyping(e.currentTarget.value, "context")}
                rows={20}>
              </textarea>
              <button class="p-2 w-24 bg-blue-900 text-white"
                onclick={LoadContext}
              >Load</button>
            </div>
          </section>
          <section hidden={tab() !== "prompt"}>
            <div>
              <div class="bg-blue-900 p-1 text-white space-x-1">
                <label class='uppercase'>Max Tokens:</label>
                <input class="w-20 px-1 text-black"
                  value={settings().max_tokens}
                  oninput={(e) => setSettings({ ...settings(), max_tokens: e.currentTarget.value })}
                />
                <label class='uppercase'>Temperature:</label>
                <input class="w-20 px-1 text-black"
                  value={settings().temperature}
                  oninput={(e) => setSettings({ ...settings(), temperature: e.currentTarget.value })}
                />
              </div>
            </div>
            <div class="flex flex-col space-y-2 mt-2 p-2">
              <textarea
                class="border border-black p-2 round-lg"
                value={settings().prompt}
                onInput={(e) => getTokenCountAfterTyping(e.currentTarget.value, "prompt")}
                rows={20}>
              </textarea>
              <div class="p-2 bg-yellow-100">
                <p><strong>Note: </strong>the additional context will be added automatically at the end of the Prompt. If you need to set a specific placement, use the &lt;CONTEXT&gt; placeholder. This placeholder will be replaced in the final Prompt at the specific location.</p>
              </div>
              <div class="space-x-2">
                <button class="p-2 w-24 bg-blue-900 hover:bg-blue-700 text-white font-semibold"
                  onclick={Submit}
                >{submitLabel()}
                </button>
                <button class="p-2 bg-green-800 hover:bg-green-700 text-white font-semibold"
                  onclick={LoadSamplePrompt}
                >Sample Prompt
                </button>
              </div>
            </div>
          </section>
          <section hidden={tab() !== "completion"}>
            <div class="flex flex-col mt-2 space-y-2 p-2">
              <textarea
                readOnly
                value={completion()}
                class="border bg-slate-200 border-black p-2 round-lg"
                rows={20}>
              </textarea>
            </div>
          </section>
          <section class="w-full md:w-1/4 px-2 space-y-2">

          </section>
        </div>
        <div class="basis-full md:basis-1/4 px-2">
          <div class="space-x-2 bg-blue-900 text-white p-1">
            <label class="uppercase">Embeddings: </label> <span title="Total Chunks" class='px-2 bg-slate-800 text-white rounded-xl'>{parseCompletion().chunks.length}</span> -
            <span title="Total Tokens" class='px-2 bg-slate-800 text-white rounded-xl'>{parseCompletion().chunks.reduce((acc, chunk) => acc + chunk.tokenCount, 0)}</span>
          </div>

          <For each={parseCompletion().chunks}>
            {(chunk, idx) => (
              <div class="flex flex-col mb-2 rounded-lg p-2 space-y-3 border-2 border-slate-300 hover:border-2 hover:border-slate-800 shadow">
                <div>
                  <span class="text-sm font-bold uppercase">Embedding: </span>{idx() + 1} <span class="text-sm font-bold uppercase">Tokens: </span>{chunk.tokenCount}
                </div>
                <hr class="border-black" />
                {/* <span class="">{chunk.text}</span> */}
                <textarea class="bg-slate-200 p-2" rows={20} value={chunk.text} readOnly></textarea>
              </div>
            )}
          </For>
        </div>
      </div>
    </>
  )
}

export default App

# type "uvicorn main:app --reload" in terminal to start the web app
# -----------------------------------------------------------------------------

import os
import uuid
import shutil
from typing import Dict
from contextlib import asynccontextmanager

from fastapi import FastAPI, File, UploadFile, WebSocket, WebSocketDisconnect
from fastapi.responses import HTMLResponse
from fastapi.staticfiles import StaticFiles

from pydantic_ai import Agent
from pydantic_ai.models.openai import OpenAIChatModel
from pydantic_ai.providers.deepseek import DeepSeekProvider
from dotenv import load_dotenv

import tools # agent tools


load_dotenv()
api_key = os.getenv("DEEPSEEK_API_KEY")

# agent setup
model = OpenAIChatModel(
    "deepseek-reasoner",
    provider=DeepSeekProvider(api_key=api_key),
)

agent = Agent(
    model,
    system_prompt=(
        "You are an experienced materials scientist. "
        "When the user uploads a PDF, you can read it with `read_pdf(file_path, page_number)`. "
        "The file path will be provided by the system. "
        "You may also run spin‑coating experiments with `do_experiment`."
    ),
    deps_type=tools.Deps,
    tools=[tools.read_pdf, tools.do_experiment],
)


# session storage (in‑memory, for demo)
sessions: Dict[str, dict] = {}

@asynccontextmanager
async def lifespan(app: FastAPI):
    # ensure cache folder exists on startup
    os.makedirs("./pdf_cache", exist_ok=True)
    print("pdf_cache folder ready")
    yield
    # delete the entire pdf_cache folder on shutdown
    if os.path.exists("./pdf_cache"):
        shutil.rmtree("./pdf_cache")
        print("pdf_cache folder cleaned up")

app = FastAPI(lifespan=lifespan)

# serve static files (HTML, JS, CSS)
app.mount("/static", StaticFiles(directory="static"), name="static")

# HTML entry point
@app.get("/")
async def get_index():
    with open("static/index.html", "r", encoding="utf-8") as f:
        return HTMLResponse(f.read())

# PDF upload endpoint
@app.post("/upload")
async def upload_pdf(session_id: str, file: UploadFile = File(...)):
    # auto save file to ./pdf_cache with a safe name
    file_ext = os.path.splitext(file.filename)[1]
    safe_filename = f"{session_id}_{uuid.uuid4().hex}{file_ext}"
    file_path = os.path.join("./pdf_cache", safe_filename)

    contents = await file.read()
    with open(file_path, "wb") as f:
        f.write(contents)

    # store path in session
    if session_id not in sessions:
        sessions[session_id] = {"history": [], "pdf_path": None}
    sessions[session_id]["pdf_path"] = file_path

    return {"filename": safe_filename, "path": file_path}

# WebSocket chat endpoint
@app.websocket("/ws/{session_id}")
async def websocket_endpoint(websocket: WebSocket, session_id: str):
    await websocket.accept()

    if session_id not in sessions:
        sessions[session_id] = {"history": [], "pdf_path": None}
    session = sessions[session_id]

    # send json events to the frontend
    async def send_event(event: dict):
        await websocket.send_json(event)

    try:
        while True:
            # receive user message
            data = await websocket.receive_json()
            user_msg = data.get("text", "").strip()
            if not user_msg:
                continue

            print(f"Received: {user_msg}")

            if session["pdf_path"]:
                path_hint = f"[Current PDF is at: {session['pdf_path']}]"
                full_user_input = f"{path_hint}\n\n{user_msg}"
            else:
                full_user_input = user_msg

            deps = tools.Deps(send_event=send_event)

            try:
                # run agent synchronously (non‑streaming) to get the full response at once
                result = await agent.run(full_user_input, deps=deps, message_history=session["history"])
                full_response = result.output
                await websocket.send_json({"type": "assistant_response", "content": full_response})
                # update conversation history
                session["history"] = list(result.all_messages())
            except Exception as e:
                await websocket.send_json({"type": "error", "content": f"Agent error: {str(e)}"})
                print(f"Agent error: {e}")

    except WebSocketDisconnect:
        print("Client disconnected")
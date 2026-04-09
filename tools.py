import json
import time
from typing import Optional, List
import os
import base64
import PyPDF2
import fitz  # PyMuPDF
from pydantic_ai import RunContext

from agent_client import MQTTConnector

local_client = MQTTConnector()
experiment_topic = "do_experiment"
json_path = "bin\\Debug\\net8.0-windows\\reagent_layout.json"

class Deps:
    """Dependency container passed to the agent, not callable"""
    def __init__(self, send_event):
        self.send_event = send_event # async callback to push JSON to WebSocket

async def read_pdf(
    ctx: RunContext[Deps],
    file_path: str,
    page_number: Optional[int] = None
) -> str:
    """
    Extract text from a PDF. If page number is given, also render that page as an image and send it via the dependency
    callback. The image of the page reading will be shown in the window docked to the right of the web page
    """
    await ctx.deps.send_event({
        "type": "tool_call",
        "name": "read_pdf",
        "args": {"file_path": file_path, "page_number": page_number}
    })

    if not os.path.exists(file_path):
        err = f"File not found: {file_path}"
        await ctx.deps.send_event({"type": "tool_result", "name": "read_pdf", "result": err})
        return err

    try:
        with open(file_path, "rb") as f:
            reader = PyPDF2.PdfReader(f)
            num_pages = len(reader.pages)

            if page_number is not None:
                if 1 <= page_number <= num_pages:
                    page = reader.pages[page_number - 1]
                    text = page.extract_text() or ""

                    try:
                        doc = fitz.open(file_path)
                        page_img = doc[page_number - 1]
                        pix = page_img.get_pixmap()
                        img_data = pix.tobytes("png")
                        img_base64 = base64.b64encode(img_data).decode()
                        await ctx.deps.send_event({
                            "type": "pdf_page_image",
                            "page": page_number,
                            "image": img_base64
                        })
                        doc.close()
                    except Exception as img_err:
                        await ctx.deps.send_event({
                            "type": "warning",
                            "content": f"Could not render page {page_number} as image: {img_err}. Please install PyMuPDF with 'pip install PyMuPDF'."
                        })
                else:
                    text = f"Page {page_number} out of range (1–{num_pages})."
            else:
                text = ""
                for i, page in enumerate(reader.pages):
                    text += f"\n--- Page {i+1} ---\n"
                    text += page.extract_text() or ""

        # await ctx.deps.send_event({"type": "tool_result", "name": "read_pdf", "result": "reading"})
        return text

    except Exception as e:
        err = f"Error reading PDF: {str(e)}"
        # await ctx.deps.send_event({"type": "tool_result", "name": "read_pdf", "result": err})
        return err

def find_reagent(name:str, path = json_path) -> str:
    """
    Search through reagent_layout.json to find if the reagent we need is already loaded onto the experiment platform.
    Not callable, used in do_experiment()
    :param name: the reagent we want to use
    :param path: the path of reagent_layout.json
    :return: The position in the form of "BPxx" of the reagent on the platform if reagent found, otherwise, raise an
    error and return the description
    """
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
            points = data.get("Points", {})
            for point_id, info in points.items():
                reagent_name = info.get("name", "")

                if reagent_name == name:
                    return point_id
            return "Reagent is missing"
    except Exception as e:
        err = str(e)
        return err

async def get_all_reagents(
        ctx: RunContext[Deps],
        path = json_path
) -> str:
    """
    Scan reagent_layout.json for knowledge of all available reagents to use. When received "Reagent is missing", you can
    do this to check if there is simply any mistakes in spelling, or tell if the reagent is configured to platform
    :param path: The path of reagent_layout.json
    :return: All the reagent names available in a single string with the format of "reagent1, reagent2, ...". Otherwise,
    an error has occurred. Please seek human scientists for help
    """
    try:
        await ctx.deps.send_event({
            "type": "tool_call",
            "name": "get_all_reagents",
            "args": {}
        })

        with open(path, "r", encoding="utf-8") as f:
            available_reagents = ""
            idx = 0
            data = json.load(f)
            points = data.get("Points", {})
            for point_id, info in points.items():
                if info.get("name") != "":
                    available_reagents += f"{info.get("name")}, "
                    idx += 1
            msg = f"scan complete, found {idx} available reagents"
            await ctx.deps.send_event({"type": "tool_result", "name": "get_all_reagents", "result": msg})
            return available_reagents

    except Exception as e:
        err = str(e)
        return err

async def save_experiment_step(
        ctx: RunContext[Deps],
        spin_speed:int = 3000,
        spin_acc:int = 1000,
        spin_dur:int = 30000,
        reagent:str = "",
        volume:int = 10,
) -> str:
    """
    Register a single step of an in-situ spin coating experiment to the platform. This function will send all the
    parameters you have set one by one to the emqx server, and then it will pass them on to the platform for saving them.
    You have to first make sure parameters for each step is registered, then call start() to do whole round.
    :param spin_speed: spin speed for spin coating, max 6000rpm, default 3000rpm
    :param spin_acc: acceleration of the spin coater, must be integer and default 1000rpm/s
    :param spin_dur: spin duration for spin coating in ms, default 30000ms
    :param reagent: Name of the reagent to be used this round.
    :param volume: The volume of the reagent to be dispensed onto substrate, default 10ul
    :return: Whether there is any errors. No errors will return an "Experiment started" message. If the reagent is not
    ready, will return "Reagent is missing". You can do get_all_reagents() to figure out where the problem is. If the
    server is not connected, will return "Connect server failed". You can ask the scientists to check emqx connection.
    """
    try:
        await ctx.deps.send_event({
            "type": "tool_call",
            "name": "save_experiment_step",
            "args": {
                "spin_speed": spin_speed,
                "spin_acc": spin_acc,
                "spin_dur": spin_dur,
                "reagent": reagent,
                "volume": volume,
            }
        })

        reagent_pos = find_reagent(reagent)
        if reagent_pos[:2] != "BP":
            return reagent_pos

        if local_client.is_connected:
            local_client.publish(experiment_topic, f"p{spin_speed},{spin_acc},{spin_dur},{reagent_pos},{volume}")
            msg = (f"✅ Experiment registered: seeking {reagent} at {reagent_pos}, {spin_speed} rpm, "
                   f"acc {spin_acc} rpm/s, duration {spin_dur} ms, volume {volume} µl.")
            await ctx.deps.send_event({"type": "tool_result", "name": "do_experiment", "result": msg})

            return msg
        else:
            connect_state = local_client.connect()
            if connect_state:
                local_client.publish(experiment_topic, f"p{spin_speed},{spin_acc},{spin_dur},{reagent_pos},{volume}")
                msg = (f"✅ Experiment registered: seeking {reagent} at {reagent_pos}, {spin_speed} rpm, "
                       f"acc {spin_acc} rpm/s, duration {spin_dur} ms, volume {volume} µl.")
                await ctx.deps.send_event({"type": "tool_result", "name": "do_experiment", "result": msg})

                return msg
            else:
                return "Connect server failed"
    except Exception as e:
        err = f"Error occurred: {str(e)}"
        return err

async def start_experiment(
        ctx: RunContext[Deps],
) -> bool:
    """
    Tell the platform to do the whole experiment round step by step according to the parameters you have registered.
    This function should be called AFTER registering all experiment steps with save_experiment_steps().
    It triggers the platform to execute the registered spin-coating steps in sequence.
    :return: Whether successfully started the experiment round
    """
    try:
        await ctx.deps.send_event({
            "type": "tool_call",
            "name": "start_experiment",
            "args": {}
            })

        if local_client.is_connected:
            local_client.publish(experiment_topic, "pstart")
            msg = (f"✅ Experiment started")
            await ctx.deps.send_event({"type": "tool_result", "name": "do_experiment", "result": msg})
            return True
        else:
            connect_state = local_client.connect()
            if connect_state:
                local_client.publish(experiment_topic, "pstart")
                msg = (f"✅ Experiment started")
                await ctx.deps.send_event({"type": "tool_result", "name": "do_experiment", "result": msg})
                return True
            else:
                return False

    except Exception:
        return False

async def move_arm(
        ctx: RunContext[Deps],
        x:int = 220,
        y:int = -220,
        z:int = 200,
        r:int = 0
) -> bool:
    """
    Move the robot arm of the platform to specific point. {220, -220, 200, 0} is set as a safe position when the arm is
    not in use. This tool function must not be used together with start_experiment
    :param x: Position of X axis
    :param y: Position of Y axis
    :param z: Position of Z axis
    :param r: Position of R axis, how much the claw will rotate
    :return: Whether there is any problems. If returned True, the arm will move successfully
    """
    await ctx.deps.send_event({
        "type": "tool_call",
        "name": "move_arm",
        "args": {
            "x": x,
            "y": y,
            "z": z,
            "r": r
        }
    })

    if local_client.is_connected:
        local_client.publish(experiment_topic, f"a{x},{y},{z},{r}")
        local_client.publish(experiment_topic, "astart")
        msg = (f"✅ Arm moving")
        await ctx.deps.send_event({"type": "tool_result", "name": "move_arm", "result": msg})
        return True
    else:
        connect_state = local_client.connect()
        if connect_state:
            local_client.publish(experiment_topic, f"a{x},{y},{z},{r}")
            local_client.publish(experiment_topic, "astart")
            msg = (f"✅ Arm moving")
            await ctx.deps.send_event({"type": "tool_result", "name": "move_arm", "result": msg})
            return True
        else:
            return False
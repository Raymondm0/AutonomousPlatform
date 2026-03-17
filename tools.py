import json

from jinja2.utils import missing

from agent_client import MQTTConnector

local_client = MQTTConnector()
topic = "do_experiment"
json_path = "bin\\Debug\\net8.0-windows\\reagent_layout.json"

def read_pdf():
    pass

def get_reagents(name:str, path = json_path) -> str:
    """
    Search through reagent_layout.json to find if the reagent we need is already loaded onto the experiment platform.
    :param name: the reagent we want to use
    :param path: the path of reagent_layout.json
    :return: The position in the form of "BPxx" of the reagent on the platform if reagent found, otherwise, raise an error
    """
    try:
        with open(path, "r", encoding="utf-8") as f:
            data = json.load(f)
            points = data.get("Points", {})
            for point_id, info in points.items():
                reagent_name = info.get("name", "")

                if reagent_name == name:
                    return point_id
            raise Exception("Reagent is missing")
    except Exception as e:
        print(f"Error occurred: {e}")
        return e

def do_experiment(spin_speed:int, spin_acc:int, spin_dur:int, reagent:str, volume:int) -> str:
    """
    Tell the platform to conduct a single round of an in-situ spin coating experiment. This function will send all the
    parameters you have set one by one to the emqx server, and then it will pass them on to the platform to start the
    experiment.
    :param spin_speed: spin speed for spin coating, max 6000rpm
    :param spin_acc: acceleration of the spin coater, must be integer and default 1000rpm/s
    :param spin_dur: spin duration for spin coating in ms
    :param reagent: Name of the reagent to be used this round.
    :param volume: The volume of the reagent to be dispensed onto substrate
    :return: Whether there is any errors. No errors will return "experiment start". If there is any problem, please
    inform human scientists to tackle them
    """
    try:
        reagent_pos = get_reagents(reagent)
        if reagent_pos[:2] != "BP":
            raise Exception("Reagent is missing")
        print(reagent_pos)

        if local_client.is_connected:
            local_client.publish(topic, f"p{spin_speed},{spin_acc},{spin_dur},{reagent_pos},{volume}")
            return "experiment start"
        else:
            connect_state = local_client.connect()
            if connect_state:
                local_client.publish(topic, f"p{spin_speed},{spin_acc},{spin_dur},{reagent_pos},{volume}")
                return "experiment start"
            else:
                raise Exception("Connect server failed")
    except Exception as e:
        print(f"Error occurred: {e}")
        return e

print(do_experiment(2000,1000,5000,"hello",10))

# test code
# connector = MQTTConnector()
# if connector.connect():
#     print("Success logic")
# else:
#     print("Fail logic")
# while True:
#     connector.publish("test topic", "test msg")
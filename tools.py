from agent_client import MQTTConnector

local_client = MQTTConnector()

def read_pdf():
    pass

def connect_server() -> bool:
    """
    Connect or check connection with emqx server. Since we use emqx server to send commands to the experiment platform,
    do this function first whenever trying to communicate with the platform
    :return: The current connection state. True if agent_client is already or successfully connected, otherwise, False
    """
    if local_client.is_connected:
        return True
    else:
        connect_state = local_client.connect()
        if connect_state:
            return True
        else:
            return False

def get_reagents(name:str) -> bool:
    """
    Search through reagent_layout.json to find if the reagent we need is already loaded onto the experiment platform. If
    reagent not found, please don't do do_experiment() and inform human scientists
    :param name: the reagent we want to use
    :return: True if reagent found and False if not found
    """
    pass

def do_experiment(spin_speed:int, spin_acc:int, spin_dur:int, reagent:str, volume:int) -> None:
    """
    Tell the platform to conduct a single round of an in-situ spin coating experiment. This function will send all the
    parameters you have set in a certain form to the emqx server, and then it will pass them on to the platform to start
    the experiment.
    :param spin_speed: spin speed for spin coating, max 6000rpm
    :param spin_acc: acceleration of the spin coater, must be integer and default 1000rpm/s
    :param spin_dur: spin duration for spin coating
    :param reagent: Name of the reagent to be used this round. Before passing in this parameter, please do get_reagents()
     to make sure the reagent name was registered to the platform. If not, the experiment will not run.
    :param volume: The volume of the reagent to be dispensed onto substrate
    :return:
    """
    pass

# test code
# connector = MQTTConnector()
# if connector.connect():
#     print("Success logic")
# else:
#     print("Fail logic")
# while True:
#     connector.publish("test topic", "test msg")
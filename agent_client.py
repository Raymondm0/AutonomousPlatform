import threading
import paho.mqtt.client as mqtt

class Client_Conf:
    """
    class saving the configurations of the agent client, including client id, user name, password, server ip and port
    """
    def __init__(self):
        self.client_id = "bibilabu"
        self.usr_name = "agent"
        self.password = "s208ht"
        self.ip = "192.168.120.129"
        self.port = 1883

class MQTTConnector:
    """emqx server connection class"""
    def __init__(self):
        self.client_config = Client_Conf()
        self.client = None
        self.is_connected = False
        self.connect_event = threading.Event()

    def on_connect(self, client, userdata, flags, rc):
        """connection recall"""
        if rc == 0:
            print('Connected to emqx server')
            self.is_connected = True
        else:
            print(f'Connection failed! RC: {rc}')
            self.is_connected = False

        self.connect_event.set()

    def connect(self, timeout=5) -> bool:
        """
        connect the emqx server. automatically initiallizes mqtt.Client() class object
        :param timeout: how long the thread waits for connection recall. if timeout, connection is considered fail
        :return: True if connection success, False if failed
        """
        self.client = mqtt.Client()
        self.client.username_pw_set(username=self.client_config.usr_name, password=self.client_config.password)
        self.client.on_connect = self.on_connect

        # reset connection status
        self.is_connected = False
        self.connect_event.clear()

        try:
            self.client.connect(self.client_config.ip, self.client_config.port, 60)
            self.client.loop_start()

            # waiting for recall. if connection established, returns True
            if self.connect_event.wait(timeout):
                return self.is_connected
            else:
                print("Timeout waiting for connection.")
                return False
        except Exception as e:
            print(f"Error: {e}")
            return False

    def publish(self, topic:str, msg:str):
        self.client.publish(topic, msg)

# test code
# connector = MQTTConnector()
# if connector.connect():
#     print("Success logic")
# else:
#     print("Fail logic")
# while True:
#     connector.publish("test topic", "test msg")
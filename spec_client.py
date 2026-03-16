from typing import *
import paho.mqtt.client as mqtt
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

from test import save_fig


client_id = "987zyx"
usr_name = "s208"
password = "s208ht"
ip = "192.168.120.129"
topic = "counts"

counts = []
wavelength = []
time = []
running_state = 0
df = pd.DataFrame({
    'counts':[],
    'wavelength':[],
    'time': []
})

client = mqtt.Client()
client.username_pw_set(username=usr_name, password=password)

#connect success recall
def on_connect(client, userdata, flags, rc):
    print('Connected with result code '+str(rc))
    client.subscribe("counts")
    client.subscribe("wavelength")
    client.subscribe("control")
    client.subscribe("time")

#receive message recall
def get_msg(func):
    def wrapper(*args):
        global df
        global running_state
        message = func(*args)

        #if client is running when received a message
        if running_state == 1:
            if message.topic == 'counts':
                data = message.payload.split()
                # counts.append(np.array(data, dtype=np.float64))
                counts.append(data)

            elif message.topic == 'wavelength':
                data = message.payload.split()
                # wavelength.append(np.array(data, dtype=np.float64))
                wavelength.append(data)

            elif message.topic == 'time':
                data = float(message.payload)
                time.append(data)
                # print(time)

            elif message.topic == 'control':
                if message.payload == b'record':
                    new_data = pd.DataFrame({
                        'counts': counts,
                        'wavelength': wavelength,
                        'time': time
                    })
                    df = pd.concat([df, new_data], ignore_index=True)

                    counts.clear()
                    wavelength.clear()
                    time.clear()
                    # client.publish('control', 'next')

                # block client
                elif message.payload == b'stop':
                    running_state = 0
                # end connection
                elif message.payload == b'quit':
                    running_state= -1

        # if the client is blocked when received a message
        elif running_state == 0:
            if message.topic == 'control':
                # wake the client
                if message.payload == b'continue':
                    running_state = 1
                    df = pd.DataFrame({
                        'counts': [],
                        'wavelength': [],
                        'time': []
                    })
                    client.publish('control', 'next')
                # end connection
                elif message.payload == b'quit':
                    running_state = -1

        return func(*args)
    return wrapper

@get_msg
def data(client, userdata, msg):
    return msg

def show_data(func):
    def wrapper(*args):
        func()
    return wrapper

@show_data
def run() -> pd.DataFrame:
    '''
    Starts the loop, continuously reads data from spectrum and stores them in a dataframe.
    When message "stop" from topic "control" is received, the loop blocks.
    When message "continue" from topic "control" is received, the loop will start again.
    When message "quit" from topic "control" is received, the program stops the loop and quit.
    :return:
        pd.DataFrame({ 'counts':[], 'wavelength':[], 'time':[] })
    '''
    #set recall functions
    client.on_connect = on_connect
    client.on_message = data

    #establish connection
    client.connect(ip, 1883, 60)

    client.loop_start()

    while True:
        if running_state == -1:
            client.loop_stop()
            print("disconnected")
            break

        elif running_state == 1:
            while True:
                if running_state == 0:
                    print("data all received")
                    print(df.shape)
                    save_fig(df)
                    break
    return df

run()
from typing import TextIO
import numpy as np
import base64, cv2

class DataStream:

    def __init__(self, stream:TextIO):
        self.stream = stream

    def write(self, senddata):
        self.stream.write('{}\n'.format(senddata))
        self.stream.flush()
    
    def write_img(self, img):
        encimg = cv2.imencode('.png', img)[1]
        self.send('data:image/png;base64,' + base64.b64encode(encimg).decode('utf-8'))
    
    def read(self):
        try:
            return self.stream.readline()
        except:
            return ''
    
    def read_img(self):
        rcv = self.receive()
        if rcv.split(',')[0] != 'data:image/png;base64': return None
        rcv = rcv.split(',')[1]
        rcv = base64.b64decode(rcv)
        rcv = np.frombuffer(rcv,dtype=np.uint8)
        img = cv2.imdecode(rcv, cv2.IMREAD_UNCHANGED)
        return img
    
    def close(self):
        self.stream.close()

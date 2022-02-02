from typing import Any, TextIO
import numpy as np
import base64, cv2

class DataStream:

    def __init__(self, stream: TextIO) -> None:
        self.stream = stream
    
    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def write(self, senddata: Any) -> None:
        self.stream.write(f'{senddata}\n')
        self.stream.flush()
    
    def write_img(self, img: np.ndarray) -> None:
        encimg = cv2.imencode('.png', img)[1]
        self.write('data:image/png;base64,' + base64.b64encode(encimg).decode('utf-8'))
    
    def read(self) -> str:
        try:
            return self.stream.readline().strip()
        except:
            return ''
    
    def read_img(self) -> np.ndarray:
        rcv: str = self.read()
        if rcv.split(',')[0] != 'data:image/png;base64': return None
        rcv = rcv.split(',')[1]
        rcv = base64.b64decode(rcv)
        rcv = np.frombuffer(rcv, dtype=np.uint8)
        img = cv2.imdecode(rcv, cv2.IMREAD_UNCHANGED)
        return img
    
    def close(self) -> None:
        self.stream.close()
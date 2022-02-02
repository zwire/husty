import socket
from data_stream import DataStream

class UdpServer:

    def __init__(self, ip='127.0.0.1', port=3000) -> None:
        self._sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self._sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self._sock.bind((ip, port))
        self._stream = self._sock.makefile('rw', encoding='utf-8')
        print('Connected!')
    
    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()
    
    def get_stream(self) -> DataStream:
        return DataStream(self._stream)

    def close(self) -> None:
        self._stream.close()
        self._sock.close()


class UdpClient:

    def __init__(self, ip='127.0.0.1', port=3000) -> None:
        self._sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self._sock.connect((ip, port))
        self._stream = self._sock.makefile('rw', encoding='utf-8')
        print("Connected!")
    
    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()
    
    def get_stream(self) -> DataStream:
        return DataStream(self._stream)

    def close(self) -> None:
        self._stream.close()
        self._sock.close()
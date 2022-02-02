import socket
from data_stream import DataStream

class TcpServer:

    def __init__(self, ip='127.0.0.1', port=3000) -> None:
        self.serversock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.serversock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.serversock.bind((ip, port))
        self.serversock.listen(1)
        print('Waiting for connections...')
        self.clientsock, self.client_address = self.serversock.accept()
        self.stream = self.clientsock.makefile('rw', encoding='utf-8')
        print('Connected!')

    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()
    
    def get_stream(self) -> DataStream:
        return DataStream(self.stream)

    def close(self) -> None:
        self.stream.close()
        self.clientsock.close()
        self.serversock.close()

class TcpClient:

    def __init__(self, ip='127.0.0.1', port=3000) -> None:
        self.client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.client.connect((ip, port))
        self.stream = self.client.makefile('rw', encoding='utf-8')
        print("Connected!")

    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()
    
    def get_stream(self) -> DataStream:
        return DataStream(self.stream)

    def close(self) -> None:
        self.stream.close()
        self.client.close()
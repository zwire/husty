import time, requests, threading, random, socket, contextlib
from requests.exceptions import Timeout

reserved_ports = set()

def find_free_port(lower=10000, upper=50000) -> int:
    port = random.randint(lower, upper)
    while port < 65535:
        if port not in reserved_ports:
            try:
                with contextlib.closing(socket.socket(socket.AF_INET, socket.SOCK_STREAM)) as s:
                    s.bind(('', port))
                with contextlib.closing(socket.socket(socket.AF_INET, socket.SOCK_DGRAM))  as s:
                    s.bind(('', port))
                return port
            except socket.error:
                reserved_ports.add(port)
                continue
        port += 1
    raise Exception('could not find open port')

class DataConnectionInfo:

        def __init__(self, local_data_ep, remote_data_ep, remote_peer_id) -> None:
            self.local_data_ep = local_data_ep
            self.remote_data_ep = remote_data_ep
            self.remote_peer_id = remote_peer_id

class MediaConnectionInfo:

        def __init__(self, \
                local_video_ep, local_video_rtcp_ep, \
                local_audio_ep, local_audio_rtcp_ep, \
                remote_video_ep, remote_video_rtcp_ep, \
                remote_audio_ep, remote_audio_rtcp_ep, \
                remote_peer_id) -> None:
            self.local_video_ep = local_video_ep
            self.local_video_rtcp_ep = local_video_rtcp_ep
            self.local_audio_ep = local_audio_ep
            self.local_audio_rtcp_ep = local_audio_rtcp_ep
            self.remote_video_ep = remote_video_ep
            self.remote_video_rtcp_ep = remote_video_rtcp_ep
            self.remote_audio_ep = remote_audio_ep
            self.remote_audio_rtcp_ep = remote_audio_rtcp_ep
            self.remote_peer_id = remote_peer_id

class Peer:

    def __init__(self, key: str, local_id: str, use_turn=False, credential=None) -> None:
        self._key = key
        self._base_url = 'http://localhost:8000/'
        req = { 'key': key, 'domain': 'localhost', 'peer_id': local_id, 'turn': use_turn }
        if credential is not None:
            req['credential'] = credential
        r = requests.post(self._base_url + 'peers', json=req).json()['params']
        if 'errors' in r.keys():
            raise Exception(r['errors'])
        self.local_peer_id = r['peer_id']
        self._token = r['token']
        self._data_connection_id = ''
        self._media_connection_id = ''
        self._peer_opened = False
        self._killed = False
        self._lock = threading.Lock()
        self._loop_thread = threading.Thread(target=self._loop_listen_events)
        self._loop_thread.start()
        while self._peer_opened is False:
            time.sleep(1)

    def __enter__(self):
        return self
    
    def __exit__(self, exc_type, exc_value, traceback):
        self.close()

    def close(self) -> None:
        self._killed = True
        requests.delete(self._base_url + f'peers/{self.local_peer_id}?token={self._token}')
        self._loop_thread.join()
    
    def call_data_connection(self, remote_peer_id) -> DataConnectionInfo:
        return DataChannel(self).call(remote_peer_id)
    
    def listen_data_connection(self) -> DataConnectionInfo:
        return DataChannel(self).listen()
    
    def call_media_connection(self, remote_peer_id) -> MediaConnectionInfo:
        return MediaChannel(self).call(remote_peer_id)
    
    def listen_media_connection(self) -> MediaConnectionInfo:
        return MediaChannel(self).listen()

    def confirm_alive(self) -> bool:
        r = requests.get(self._base_url + f'peers/{self.local_peer_id}/status?token={self._token}').json()
        return r['disconnected'] is False
    
    def change_credential(self, credential) -> None:
        requests.put(f'peers/{self.local_peer_id}/credential?token={self._token}', credential)

    def _loop_listen_events(self):
        while self._killed == False:
            with self._lock:
                try:
                    # peer
                    r = requests.get(self._base_url + f'peers/{self.local_peer_id}/events?token={self._token}', timeout=10).json()
                    if r['event'] == 'OPEN': 
                        self._peer_opened = True
                    elif r['event'] == 'CLOSE': 
                        self._peer_opened = False
                        break
                    elif r['event'] == 'CONNECTION': 
                        self._data_connection_id = r['data_params']['data_connection_id']
                    elif r['event'] == 'CALL': 
                        self._media_connection_id = r['call_params']['media_connection_id']
                    elif r['event'] == 'ERROR': 
                        raise Exception(r['error_message'])
                except Timeout:
                    pass


class DataChannel:

    def __init__(self, peer: Peer) -> None:
        self._peer = peer
        r = requests.post(self._peer._base_url + 'data', json={}).json()
        self._data_id = r['data_id']
        self.local_data_ep = {'ip_v4': '127.0.0.1', 'port': find_free_port()}
        self.remote_data_ep = {'ip_v4': r['ip_v4'], 'port': int(r['port'])}
        self._data_connection_id = ''
        
    def close(self) -> None:
        if self._data_connection_id is not '':
            requests.delete(self._peer._base_url + f'data/connections/{self._data_connection_id}')
        if self._data_id is not None:
            requests.delete(self._peer._base_url + f'data/{self._data_id}')

    def listen(self) -> DataConnectionInfo:
        if self._data_connection_id is not '':
            raise Exception()
        while self._peer._data_connection_id is '':
            time.sleep(0.2)
        self._data_connection_id = self._peer._data_connection_id
        self._peer._data_connection_id = ''
        req = {
            'feed_params': {'data_id': self._data_id},
            'redirect_params': self.local_data_ep
        }
        requests.put(self._peer._base_url + f'data/connections/{self._data_connection_id}', json=req)
        self.remote_peer_id = requests.get(self._peer._base_url + f'data/connections/{self._data_connection_id}/status').json()['remote_id']
        return DataConnectionInfo(self.local_data_ep, self.remote_data_ep, self.remote_peer_id)

    def call(self, remote_peer_id) -> DataConnectionInfo:
        if self._data_connection_id is not '':
            raise Exception()
        self.remote_peer_id = remote_peer_id
        req = {
            'peer_id': self._peer.local_peer_id, 
            'token': self._peer._token, 
            'target_id': self.remote_peer_id,
            'params': {'data_id': self._data_id},
            'redirect_params': self.local_data_ep
        }
        self._data_connection_id = requests.post(self._peer._base_url + 'data/connections', json=req).json()['params']['data_connection_id']
        return DataConnectionInfo(self.local_data_ep, self.remote_data_ep, self.remote_peer_id)


class MediaChannel:

    def __init__(self, peer: Peer) -> None:
        self._peer = peer
        self.local_video_ep = {'ip_v4': '127.0.0.1', 'port': find_free_port()}
        self.local_video_rtcp_ep = {'ip_v4': '127.0.0.1', 'port': find_free_port()}
        self.local_audio_ep = {'ip_v4': '127.0.0.1', 'port': find_free_port()}
        self.local_audio_rtcp_ep = {'ip_v4': '127.0.0.1', 'port': find_free_port()}
        r = requests.post(self._peer._base_url + 'media', json={ 'is_video': True }).json()
        self._video_id = r['media_id']
        self.remote_video_ep = {'ip_v4': r['ip_v4'], 'port': int(r['port'])}
        r = requests.post(self._peer._base_url + 'media', json={ 'is_video': False }).json()
        self._audio_id = r['media_id']
        self.remote_audio_ep = {'ip_v4': r['ip_v4'], 'port': int(r['port'])}
        r = requests.post(self._peer._base_url + 'media/rtcp', json={ 'is_video': True }).json()
        self._video_rtcp_id = r['rtcp_id']
        self.remote_video_rtcp_ep = {'ip_v4': r['ip_v4'], 'port': int(r['port'])}
        r = requests.post(self._peer._base_url + 'media/rtcp', json={ 'is_video': False }).json()
        self._audio_rtcp_id = r['rtcp_id']
        self.remote_audio_rtcp_ep = {'ip_v4': r['ip_v4'], 'port': int(r['port'])}
        self.video_params = {
            'band_width': 0, 
            'codec': 'H264', 
            'media_id': self._video_id, 
            'rtcp_id': self._video_rtcp_id
        }
        self.audio_params = {
            'band_width': 0, 
            'codec': 'opus', 
            'media_id': self._audio_id, 
            'rtcp_id': self._audio_rtcp_id
        }
        self._media_connection_id = ''    

    def close(self) -> None:
        if self._media_connection_id is not None:
            requests.delete(self._peer._base_url + f'media/connections/{self._media_connection_id}')
        if self._video_rtcp_id is not None:
            requests.delete(self._peer._base_url + f'media/rtcp/{self._video_rtcp_id}')
        if self._audio_rtcp_id is not None:
            requests.delete(self._peer._base_url + f'media/rtcp/{self._audio_rtcp_id}')
        if self._video_id is not None:
            requests.delete(self._peer._base_url + f'media/{self._video_id}')
        if self._audio_id is not None:
            requests.delete(self._peer._base_url + f'media/{self._audio_id}')

    def listen(self) -> MediaConnectionInfo:
        if self._media_connection_id is not '':
            raise Exception()
        while self._peer._media_connection_id is '':
            time.sleep(0.2)
        self._media_connection_id = self._peer._media_connection_id
        self._peer._media_connection_id = ''
        req = {
            'constraints': self._get_constraints(), 
            'redirect_params': self._get_redirect_params()
        }
        requests.post(self._peer._base_url + f'media/connections/{self._media_connection_id}/answer', json=req)
        self.remote_peer_id = requests.get(self._peer._base_url + f'media/connections/{self._media_connection_id}/status').json()['remote_id']
        return MediaConnectionInfo(\
            self.local_video_ep, self.local_video_rtcp_ep, \
            self.local_audio_ep, self.local_audio_rtcp_ep, \
            self.remote_video_ep, self.remote_video_rtcp_ep, \
            self.remote_audio_ep, self.remote_audio_rtcp_ep, \
            self.remote_peer_id)

    def call(self, remote_peer_id) -> MediaConnectionInfo:
        if self._media_connection_id is not '':
            raise Exception()
        self.remote_peer_id = remote_peer_id
        req = {
            'peer_id': self._peer.local_peer_id, 
            'token': self._peer._token, 
            'target_id': self.remote_peer_id,
            'constraints': self._get_constraints(),
            'redirect_params': self._get_redirect_params()
        }
        self._media_connection_id = requests.post(self._peer._base_url + 'media/connections', json=req).json()['params']['media_connection_id']
        return MediaConnectionInfo(\
            self.local_video_ep, self.local_video_rtcp_ep, \
            self.local_audio_ep, self.local_audio_rtcp_ep, \
            self.remote_video_ep, self.remote_video_rtcp_ep, \
            self.remote_audio_ep, self.remote_audio_rtcp_ep, \
            self.remote_peer_id)
    
    def _get_constraints(self):
        return {
            'video': True, 
            'videoReceiveEnabled': True, 
            'audio': True, 
            'audioReceiveEnabled': True, 
            'video_params': self.video_params, 
            'audio_params': self.audio_params
        }
    
    def _get_redirect_params(self):
        return {
            'video': self.local_video_ep, 
            'video_rtcp': self.local_video_rtcp_ep, 
            'audio': self.local_audio_ep, 
            'audio_rtcp': self.local_audio_rtcp_ep
        }    
import json

from fastapi import FastAPI, WebSocket, WebSocketDisconnect

from networking import PacketRegistry
from packets import TellOthersToReadDatabaseC2SPacket, ReadDatabaseS2CPacket

app = FastAPI()


class ConnectionManager:
    def __init__(self):
        self.connected_clients = []
        self.packet_registry = PacketRegistry()

        self.packet_registry.register_packet(TellOthersToReadDatabaseC2SPacket.packet_type(),
                                             lambda packet: handle_tell_others_to_read_database_c2s(packet))
        self.packet_registry.register_packet(ReadDatabaseS2CPacket.packet_type())

    async def connect(self, client_id: int, websocket: WebSocket):
        await websocket.accept()
        self.connected_clients.append(ConnectedClient(client_id, websocket))

    def disconnect(self, websocket: WebSocket):
        self.connected_clients.remove(websocket)

    async def broadcast(self, message: str):
        for connection in self.connected_clients:
            await connection.send_text(message)

    def get_connected_client(self, client_id: int):
        for connected_client in self.connected_clients:
            if connected_client.client_id == client_id:
                return connected_client
        return None

    def handle_packet(self, packet_id, json_obj: dict):
        packet_type_actual = self.packet_registry.get_actual_type(packet_id)
        if packet_type_actual is None:
            raise Exception(f"Packet type {packet_id} is not registered")
        if self.packet_registry.is_clientbound(packet_id):
            raise Exception(f"Received clientbound packet {packet_id} on server")
        packet_type = self.packet_registry.get_packet_type(packet_id)
        packet = packet_type.read(json_obj)
        if type(packet) is not packet_type_actual:
            raise Exception(f"Packet type mismatch: Expected {packet_type_actual}, got {type(packet)}")
        self.packet_registry.handle_packet(packet_id, packet)


class ConnectedClient:
    def __init__(self, client_id: int, websocket: WebSocket):
        self.client_id = client_id
        self.websocket = websocket

    async def send_message(self, json_obj: dict):
        await self.websocket.send_text(json.dumps(json_obj))


manager = ConnectionManager()


@app.websocket("/ws/{client_id}")
async def websocket_endpoint(websocket: WebSocket, client_id: int):
    await manager.connect(client_id, websocket)
    try:
        while True:
            json_obj = await websocket.receive_json()
            packet_id = json_obj["id"]
            if packet_id is None:
                print(f"Received packet with no id: {json_obj} from client {client_id}")
                continue
            manager.handle_packet(packet_id, json_obj)
    except WebSocketDisconnect:
        manager.disconnect(websocket)
        await manager.broadcast(f"Client {client_id} disconnected")


async def handle_tell_others_to_read_database_c2s(packet: TellOthersToReadDatabaseC2SPacket):
    sender = manager.get_connected_client(packet.sender_id)
    if sender is None:
        raise Exception(f"Sender {packet.sender_id} not found")
    for recipient_id in packet.recipient_ids:
        recipient = manager.get_connected_client(recipient_id)
        if recipient is None:
            raise Exception(f"Recipient {recipient_id} not found")
        read_db_s2c_packet = ReadDatabaseS2CPacket(packet.db_entry)
        json_obj = {}
        read_db_s2c_packet.write(json_obj)
        await recipient.send_message(json)
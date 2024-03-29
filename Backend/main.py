import json
import logging

from fastapi import FastAPI, WebSocket, WebSocketDisconnect

from networking import PacketRegistry
from packets import TellOthersToReadDatabaseC2SPacket, ReadDatabaseS2CPacket

app = FastAPI()


class ConnectionManager:
    def __init__(self):
        self.connected_clients = {}
        self.packet_registry = PacketRegistry()

        self.packet_registry.register_packet(TellOthersToReadDatabaseC2SPacket,
                                             lambda packet: handle_tell_others_to_read_database_c2s(packet))
        self.packet_registry.register_packet(ReadDatabaseS2CPacket)

    async def connect(self, client_id: int, websocket: WebSocket):
        await websocket.accept()
        if client_id in self.connected_clients:
            logging.warning(f"Client {client_id} already connected")
            return
        self.connected_clients[client_id] = ConnectedClient(client_id, websocket)

    def disconnect(self, client_id: int):
        if client_id not in self.connected_clients:
            return
        self.connected_clients.pop(client_id)

    async def broadcast(self, message: str):
        for client_ids in self.connected_clients.keys():
            connected_client = self.connected_clients.get(client_ids)
            await connected_client.send_message(message)

    def get_connected_client(self, client_id: int):
        return self.connected_clients.get(client_id)

    async def handle_packet(self, packet_id, json_obj: dict):
        packet_type_actual = self.packet_registry.get_actual_type(packet_id)
        if packet_type_actual is None:
            raise Exception(f"Packet type {packet_id} is not registered")
        if self.packet_registry.is_clientbound(packet_id):
            raise Exception(f"Received clientbound packet {packet_id} on server")
        packet_type = self.packet_registry.get_packet_type(packet_id)
        packet = packet_type.read(json_obj)
        if type(packet) is not packet_type_actual:
            raise Exception(f"Packet type mismatch: Expected {packet_type_actual}, got {type(packet)}")
        await self.packet_registry.handle_packet(packet_id, packet)


class ConnectedClient:
    def __init__(self, client_id: int, websocket: WebSocket):
        self.client_id = client_id
        self.websocket = websocket

    async def send_message(self, json_obj: dict):
        logging.warning(f"Sending message to client {self.client_id}: {json_obj}")
        await self.websocket.send_text(json.dumps(json_obj))


manager = ConnectionManager()


@app.websocket("/ws/{client_id}")
async def websocket_endpoint(websocket: WebSocket, client_id: int):
    logging.warning(f"Client {client_id} connected")
    await manager.connect(client_id, websocket)
    try:
        while True:
            json_obj = await websocket.receive_json()
            packet_id = json_obj["id"]
            if packet_id is None:
                logging.info(f"Received packet with no id: {json_obj} from client {client_id}")
                continue
            await manager.handle_packet(packet_id, json_obj)
    except WebSocketDisconnect:
        manager.disconnect(client_id)
        logging.info(f"Client {client_id} disconnected")


async def handle_tell_others_to_read_database_c2s(packet: TellOthersToReadDatabaseC2SPacket):
    sender = manager.get_connected_client(packet.sender_id)
    if sender is None:
        raise Exception(f"Sender {packet.sender_id} not found")
    for recipient_id in packet.recipient_ids:
        recipient = manager.get_connected_client(recipient_id)
        if recipient is None:
            continue
        read_db_s2c_packet = ReadDatabaseS2CPacket(packet.db_entry)
        json_obj = {}
        read_db_s2c_packet.write(json_obj)
        await recipient.send_message(json_obj)

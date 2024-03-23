from fastapi import FastAPI, WebSocket, WebSocketDisconnect

app = FastAPI()


class ConnectionManager:
    def __init__(self):
        self.connected_clients = []

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


class ConnectedClient:
    def __init__(self, client_id: int, websocket: WebSocket):
        self.client_id = client_id
        self.websocket = websocket

    async def send_message(self, message: str):
        await self.websocket.send_text(message)


class CallbackMessage:
    def __init__(self, sender_id: int, target_ids: list, content: str):
        self.sender_id = sender_id
        self.target_ids = target_ids
        self.content = content

    @staticmethod
    def from_json(json):
        return CallbackMessage(json["sender_id"], json["target_ids"], json["content"])


manager = ConnectionManager()


@app.websocket("/ws/{client_id}")
async def websocket_endpoint(websocket: WebSocket, client_id: int):
    await manager.connect(client_id, websocket)
    try:
        while True:
            json = await websocket.receive_json()
            message = CallbackMessage.from_json(json)
            for target_id in message.target_ids:
                connected_client = manager.get_connected_client(target_id)
                if connected_client:
                    await connected_client.send_message(message.content)
    except WebSocketDisconnect:
        manager.disconnect(websocket)
        await manager.broadcast(f"Client {client_id} disconnected")


from typing import TypeVar, Generic, Type

T = TypeVar("T", bound="IPacket")


class PacketType(Generic[T]):
    def __init__(self, packet_id, deserializer=None):
        self._packet_id: str = packet_id
        self._deserializer = deserializer

    def get_packet_id(self):
        return self._packet_id

    def read(self, json) -> "IPacket":
        if self._deserializer is None:
            raise Exception(f"No deserializer provided for packet of type {self._packet_id}. It is likely clientbound")
        try:
            return self._deserializer(json)
        except Exception as e:
            raise Exception(f"Failed to deserialize packet of type {self._packet_id}: {e}")

    def is_clientbound(self):
        return self._deserializer is None


class IPacket:
    def __init__(self):
        pass

    @staticmethod
    def packet_type() -> PacketType:
        pass


class IClientboundPacket(IPacket):
    def __init__(self):
        super().__init__()

    def write(self, json):
        json["id"] = self.packet_type().get_packet_id()

    @staticmethod
    def packet_type() -> PacketType["IClientboundPacket"]:
        pass


class IServerboundPacket(IPacket):
    def __init__(self):
        super().__init__()

    @staticmethod
    def packet_type() -> PacketType["IServerboundPacket"]:
        pass


class PacketRegistry:
    def __init__(self):
        self._packets: dict[str, "PacketRegistry.PacketInfo"] = {}

    def register_packet(self, packet_reference: Type[IPacket], on_packet_received: callable(IPacket) = None):
        packet_type = packet_reference.packet_type()
        packet_id = packet_type.get_packet_id()
        if packet_id in self._packets:
            raise Exception(f"Packet type {packet_id} is already registered")
        if on_packet_received is not None and packet_type.is_clientbound():
            raise Exception(f"Handler provided for clientbound packet {packet_id}")
        if on_packet_received is None and not packet_type.is_clientbound():
            raise Exception(f"No handler provided for serverbound packet {packet_id}")
        self._packets[packet_id] = PacketRegistry.PacketInfo(packet_type, packet_reference, on_packet_received)

    def get_packet_type(self, packet_id: str) -> PacketType | None:
        if packet_id not in self._packets:
            return None
        return self._packets[packet_id].packet_type

    def get_actual_type(self, packet_id: str) -> Type | None:
        if packet_id not in self._packets:
            return None
        return self._packets[packet_id].actual_type

    def is_clientbound(self, packet_id: str) -> bool:
        return self._packets[packet_id].packet_type.is_clientbound()

    async def handle_packet(self, packet_id: str, packet: IPacket):
        if packet_id not in self._packets:
            raise Exception(f"Packet type {packet_id} is not registered")
        packet_info = self._packets[packet_id]
        await packet_info.on_packet_received(packet)

    class PacketInfo:
        def __init__(self, packet_type: PacketType, actual_type: Type, on_packet_received: callable(IPacket)):
            self.packet_type = packet_type
            self.actual_type = actual_type
            self.on_packet_received = on_packet_received

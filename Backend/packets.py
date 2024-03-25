from networking import IClientboundPacket, PacketType, IServerboundPacket


class ReadDatabaseS2CPacket(IClientboundPacket):
    def __init__(self, db_entry: str):
        super().__init__()
        self.db_entry = db_entry

    @staticmethod
    def packet_type() -> PacketType[IClientboundPacket]:
        return PacketType("read_database_s2c")

    def write(self, json):
        json["db_entry"] = self.db_entry


class TellOthersToReadDatabaseC2SPacket(IServerboundPacket):
    def __init__(self, json: dict):
        super().__init__()
        self.sender_id = json["sender_id"]
        self.recipient_ids = json["recipient_ids"]
        self.db_entry = json["db_entry"]

    @staticmethod
    def packet_type() -> PacketType[IServerboundPacket]:
        return PacketType("tell_others_to_read_database_c2s", TellOthersToReadDatabaseC2SPacket)

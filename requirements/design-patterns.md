## 2 Design Patterns

### 2.1 Singleton Pattern (Creational):
The first design pattern that we will implement in our project is singleton, as it will help facilitate the usage of user accounts in the discord clone. Information about the current user will need to be accessible in many places within the application, including but not limited to account verification and modification. Thus, creating manager-style singleton classes that allow for global access, modification and movement of account information will ensure cleaner code and safer data.

### 2.2 Strategy Pattern (Behavioral):
The second design pattern that we’ll implement into our project is strategy, which will make sending and receiving user messages easier. Sent messages must be encoded and later decoded once received, however the encoding and decoding process will differ based on the message type (text, image, ect.). Handling each message type’s encoding/decoding independently would produce large amounts of code bloat. Strategies will allow us to define general encode/decode interfaces that get implemented by classes with specific encode/decode algorithms, which removes unnecessary code variation outside the strategies.

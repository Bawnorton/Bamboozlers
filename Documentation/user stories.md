### Use Case 1: Messaging

1. a) **Sending Messages**
-   As a user, I want to send messages in a chat to communicate with other users.
    -   Acceptance Criteria: The user types a message and clicks 'send', the system stores the message in the database, and displays it in the chat for all participants.
1. b) **Deleting Messages**
-   As a user, I want to delete my messages, so that I can remove content I no longer wish to share.
    -   Acceptance Criteria: The user selects a message and chooses 'delete', the system removes the message from the database, and it disappears from the chat for all users.
1. c) **Editing Messages**
-   As a user, I want to edit my messages, so that I can correct errors or update information.
    -   Acceptance Criteria: The user selects a message and chooses 'edit', modifies it, and resubmits it. The system updates the message in the database and the chat.
1. d) **Pinning Messages**
-   As a user, I want to pin important messages, so that they are easily visible to all chat participants.
    -   Acceptance Criteria: User selects a message and chooses 'pin'. The system marks the message as pinned in the database and it is highlighted in the chat for all users.
1. e) **Messaging Non-Friends**
-   As a user, I want to only receive messages from friends so that strangers can’t contact me.
    -   Acceptance Criteria: If a user attempts to send a message to a non-friend, the message is not sent and the system displays an error message explaining the message cannot be sent.

### Use Case 2: Manage Friends

2. a) **Sending Friend Requests**
-   As a user, I want to send friend requests, so that I can connect with new people.
    -   Acceptance Criteria: The user selects a person and sends a friend request, the system stores this in the database and notifies the recipient.
2. b) **Responding to Friend Requests**
-   As a user, I want to accept or deny friend requests, so that I can expand my network.
    -   Acceptance Criteria: If the user accepts or denies a friend request, the system removes the request from the database and updates both users' friend lists accordingly.
2. c) **Editing Friend Messages**
-   As a user, I want to edit messages sent to friends, so that I can correct mistakes or update information.
    -   Acceptance Criteria: The user selects a message to a friend and edits it. The system updates the message in the database and chat.

### Use Case 3: Manage Groups

3. a) **Creating Groups**
-   As a user, I want to create groups with my friends, so that we can have shared conversations.
    -   Acceptance Criteria: The user creates a group and adds friends, the system creates a group identifier and notifies all added members.
3. b) **Joining Groups**
-   As a user, I want to join groups so I can communicate with all members of a group
    -   Acceptance Criteria: If the user accepts a group invitation, the system removes the invitation from the database and updates the group member list for all members in the group
3. c) **Leaving Groups**
- As a user, I want to leave groups so I no longer have to recieve messages that are irrelevant to me
      - Acceptance Criteria: If the user leaves the group, the system removes the memeber from the group and updates the group member list for all members in the group.

### Use Case 4: Manage Group Members

4. a) **Adding Members to Group**
-   As a group moderator, I want to add members to the group, so that new people can join our conversations.
    -   Acceptance Criteria: The moderator adds a new member, the system updates the group's database entry, and the new member can see the group chat.
4. b) **Removing Members from Group**
-   As a group moderator, I want to remove members from the group, so that we can maintain a relevant and appropriate member list.
    -   Acceptance Criteria: Moderator removes a member, the system updates the group's database entry, and the removed member loses access to the group chat.
4. c) **Assigning Permissions to Members**
-   As a group moderator, I want to assign permissions to members, so that they can help manage the group.
    -   Acceptance Criteria: Moderator assigns permissions to a member, the system updates this in the database, and the member receives admin visuals and capabilities in the group chat.

 ### Use Case 5: Registration

5. a) **Registering as a memeber**
- As a unregistered user, I want to register as a member so I can use the app.
      - Acceptance criteria: The user creates an account with their username, email and password and the server updates the database and the member receives access to the platforms functionalities.

### Use Case 6: Account Management
6. a) **Changing Username**
- As a user, I want to change my username to refelct my current identity.
   - Acceptance Requirements: The user creates a new username and the server updates the username in the database and in all friend lists.
6. b) **Changing Password**
- As a user, I want to change my password to make my account more secure.
    - Acceptance Criteria: The user creates a new password and the server updates the database.
6. c) **Reset Password**
- As a user, I want to reset my password so I can still access my account if I lose my password.
    -Acceptance Criteria: The user selects reset password, the server sends their registered email a reset password option, the user inputs a     new password and the server updates the database.     
  

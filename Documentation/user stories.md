### Use Case 1: Messaging

1. a) **Sending Messages**

- As a user, I want to send messages in a chat to communicate with other users.
    - Acceptance Criteria: The user types a message and clicks 'send', the system stores the message in the database,
      and displays it in the chat for all participants.

1. b) **Attaching Media**

- As a user, I want to attach media such as images to my messages so that I can communicate with better detail with other users.
    - Acceptance Criteria: The user attaches media to a message and clicks 'send', the system stores the media with the message in the database,
      and displays it in the chat for all participants.

1. c) **Deleting Messages**

- As a user, I want to delete my messages, so that I can remove content I no longer wish to share.
    - Acceptance Criteria: The user selects a message and chooses 'delete', the system removes the message from the
      database, and it disappears from the chat for all users.

1. d) **Editing Messages**

- As a user, I want to edit my messages, so that I can correct errors or update information.
    - Acceptance Criteria: The user selects a message and chooses 'edit', modifies it, and resubmits it. The system
      updates the message in the database and the chat.

1. e) **Pinning Messages**

- As a user, I want to pin important messages, so that they are easily visible to all chat participants.
    - Acceptance Criteria: User selects a message and chooses 'pin'. The system marks the message as pinned in the
      database and it is highlighted in the chat for all users.

1. f) **Messaging Non-Friends**

- As a user, I want to only receive messages from friends so that strangers can’t contact me.
    - Acceptance Criteria: If a user attempts to send a message to a non-friend, the message is not sent and the system
      displays an error message explaining the message cannot be sent.

### Use Case 2: Manage Friends

2. a) **Sending Friend Requests**

- As a user, I want to send friend requests, so that I can connect with new people.
    - Acceptance Criteria: The user selects a person and sends a friend request, the system stores this in the database
      and notifies the recipient.

2. b) **Responding to Friend Requests**

- As a user, I want to accept or deny friend requests, so that I can expand my network.
    - Acceptance Criteria: If the user accepts or denies a friend request, the system removes the request from the
      database and updates both users' friend lists accordingly.

2. c) **Removing Friends**

- As a user, I want to remove friends, so that I can restrict communication from people I no longer want to talk to.
    - Acceptance Criteria: The user selects a friend and removes them as friend, the system removes their friendship
      from the database and updates both users' friend lists accordingly.

2. d) **Blocking Users**

- As a user, I want to block another user who is harassing me, so that I can feel safe.
    - Acceptance Criteria: The user selects a person and blocks them, the system stores this in the database and
      the blocked user can no longer send friend requests to the user. The user will not see messages authored by the
      blocked user in any chats.

### Use Case 3: Manage Groups

3. a) **Creating Groups**

- As a user, I want to create groups with my friends, so that we can have shared conversations.
    - Acceptance Criteria: The user creates a group and adds friends, the system creates a group identifier and notifies
      all added members.

3. b) **Joining Groups**

- As a user, I want to join groups so I can communicate with all members of a group
    - Acceptance Criteria: If the user accepts a group invitation, the system removes the invitation from the database
      and updates the group member list for all members in the group

3. c) **Leaving Groups**

- As a user, I want to leave groups so I no longer have to recieve messages that are irrelevant to me
    - Acceptance Criteria: If the user leaves the group, the system removes the memeber from the group and updates the
      group member list for all members in the group.

### Use Case 4: Manage Group Members

4. a) **Invite Members to Group**

- As a group moderator, I want to invite members to the group, so that new people can join our conversations.
    - Acceptance Criteria: The moderator adds a new member, the system updates the group's database entry, and the new
    member can see the group chat.

4. b) **Removing Members from Group**

- As a group moderator, I want to remove members from the group, so that we can maintain a relevant and appropriate
  member list.
    - Acceptance Criteria: Moderator removes a member, the system updates the group's database entry, and the removed member
    loses access to the group chat.

4. c) **Deleting Messages from Users**

- As a group moderator, I want to be able to delete messages from the group chat to maintain a safe environment.
    - Acceptance Criteria: Moderator selects a message by a user in the group chat and selects 'delete', the system removes the message from the
      database, and it disappears from the chat for all users.

4. d) **Assigning Permissions to Members**

- As a group owner, I want to assign permissions to members, so that they can help manage the group.
    - Acceptance Criteria: Owner assigns permissions to a member, the system updates this in the database, and the
    member receives admin visuals and capabilities in the group chat.

4. e) **Removing Permissions from Members**

- As a group owner, I want to be able to revoke permissions from members, so that I can prevent abuse of power.
    - Acceptance Criteria: Owner revokes permissions from a moderator, the system updates this in the database, and the
    member no longer has admin visuals and capabilities in the group chat.

### Use Case 5: Account Management

5. a) **Log Into Account**

- As a user, I want to be able to log into my registered account, so that I can access the application.
    - Acceptance Criteria: The user is able to enter their username or email alongside their passphrase
      and the server authenticates their request to access their account.

5. b) **Log Out Of Account**

- As a user, I want to be able to log out of my account, so that I can end my session with the application.
    - Acceptance Criteria: The user is able to request that the system ends their authenticated session
      using the application.

5. c) **Changing Username**

- As a user, I want to change my username to reflect my current identity.
    - Acceptance Criteria: The user creates a new username and the server updates the username in the database and
      for all friends and group chats.

5. d) **Changing Avatar**

- As a user, I want to change my avatar to reflect my current identity.
    - Acceptance Criteria: The user uploads a new avatar and the server updates the avatar in the database and for
      all friends and group chats.

5. e) **Change Description**
- As a user, I want to change my description to keep my friends updated.
    - Acceptance Criteria: The user edits their current biography and the server updates it in the database and
      for all users who view their profile.

5. f) **Changing Password**

- As a user, I want to change my password to make my account more secure.
    - Acceptance Criteria: The user creates a new password and the server confirms their old password, sending a
      confirmation email to the user's registered email. The system then updates the user's password in the database.

5. g) **Reset Password**

- As a user, I want to reset my password so I can still access my account if I lose my password.
    - Acceptance Criteria: The user selects reset password, the server sends their registered email a reset password
      option, the user inputs a new password and the server updates the database.

5. h) **Change Email**

- As a user, I want to change my email so I can keep my account secure.
    - Acceptance Criteria: The user selects change email, enters their new email, the system confirms their password, 
      and then sends both their currently registered email and new email a confirmation link. The server then
      updates the database if the confirmation passes.

5. i) **Delete Account**

- As a user, I want to delete my account, so that I can stop from using the application.
    - Acceptance Criteria: The user selects delete account, and verifies their password. The system sends a confirmation
      email to their registered email address. The system removes the user's account from the database and the user's
      message history in chats is displayed via an anonymous placeholder avatar.

### Use Case 6: Registration

6. a) **Registering as a member**

- As a unregistered user, I want to register as a member so I can use the app.
    - Acceptance criteria: The user creates an account with their username, email and password and the server updates
      the database and the member receives access to the platforms functionalities.
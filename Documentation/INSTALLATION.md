## Step by Step Guide on how to install and deploy the project

## Prerequisites
- .NET (dotnet)
- PowerShell command line tool
- Docker
- Docker Compose

## Installing Prerequisites
1. ### Install .NET
<details>
<summary>MacOS</summary>

1. Install Homebrew
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```
2. Install .NET
```bash
brew install dotnet
```

</details>

<details>
<summary>Windows</summary>

1. Download the .NET SDK from the official website [here](https://dotnet.microsoft.com/download)
2. Run the installer
3. Follow the installation instructions
</details>

2. ### Install PowerShell
<details>
<summary>MacOS</summary>

1. Per step 1, you should have Homebrew installed
2. Install PowerShell
```bash
brew install powershell/tap/powershell
```

</details>

<details>
<summary>Windows</summary>

1. Open command prompt as an administrator
2. Run the following command
```bash
winget install --id Microsoft.PowerShell --source winget
```

</details>

3. ### Install Docker


1. Download the Docker Desktop installer for your operating system from the official website [here](https://www.docker.com/products/docker-desktop)
2. Run the installer
3. Follow the installation instructions
4. This will install Docker Engine, Docker CLI client, Docker Compose and more

## Installation
1. Clone the repository
```bash
git clone https://github.com/Bawnorton/Bamboozlers.git
```
2. Navigate to the project directory
```bash
cd Bamboozlers
```
3. Run the following command to build the project
```bash
dotnet build
```
4. Setup Playwright (Optional, required for testing)
```bash
pwsh App/bin/net8.0/playwright.ps1 install
```
5. Setup the database


1. Open Docker Desktop
2. Start the Docker container
<details>
<summary>MacOS ARM</summary>

```bash
docker-compose -f Docker/docker-compose-m1.yml up -d
```
</details>

<details>
<summary>Other</summary>

```bash
docker-compose -f Docker/docker-compose.yml up -d
```
</details>

6. Run the project
```bash
dotnet run --project App
```

7. Navigate to the following URL in your browser
```
http://localhost:5152
```

## Deployment (Optional)
1. Modify the target url in `App/Program.cs` on line 74 to your machine's local IP address
   - Or setup a callback for incoming connections to your machine's local IP address to localhost
2. Configure your router to port forward external TCP connections bound for WAN port 5152 to your machine's local IP address LAN port 5152
3. Configure your firewall to allow incoming connections 
4. Enable DMZ IPv4 and set the address to your machine's local IP address
5. Run the project
```bash
dotnet run --project App
```
6. Navigate to your router's public IP address in your browser with port 5152
```
eg: http://216.202.1.39:5152
```
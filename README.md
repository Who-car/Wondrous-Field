# README for "Чудесное Поле", не путать с "Поле Чудес"

## Overview

"Wondrous Field" is a competitive multiplayer word-guessing game designed to challenge players' vocabulary and quick-thinking skills. Developed using C# for both client and server sides, the game leverages TCP sockets and a custom data transfer protocol to ensure rapid response times and a seamless gaming experience.

## Technical Stack

- **Language**: C#
- **Networking**: TCP Sockets
- **Protocol**: ANTP (Aysulu-Niyaz-Transfer-Protocol)
- **UI Framework**: WPF

## Features

- **Multiplayer Gameplay**: Engage in fast-paced word-guessing battles with friends or other online players.
- **Session Management**: Create or join game sessions with unique codes for a private and secure gaming experience.
- **Real-time Communication**: Interact with other players using the integrated real-time chat system.

## Getting Started

1. **Installation**: Clone the repository to your local machine using `git clone https://github.com/kislovka-teach/network-app-v2-NikoImam.git`.
2. **Dependencies**: Ensure all necessary dependencies are installed and up-to-date.
3. **Configuration**: Configure the server and client settings as per your network setup.
4. **Compilation**: Build the solution in your preferred development environment.
5. **Execution**: Run the server application first, then start the client application to connect to the server.

## Usage

Players can host a game session by creating a new game or join an existing session by entering the provided game code. The gameplay involves guessing the hidden word by suggesting letters or attempting to solve the full word. Real-time chat is available for players to communicate during the game.

## Server and Client Interaction

The custom protocol over TCP sockets facilitates the exchange of game data, player inputs, and chat messages with minimal latency. This ensures a responsive user experience even when multiple sessions are active concurrently.

## System Requirements

- **Operating System**: Windows 7 or later.
- **.NET Framework**: .NET 6.0.
- **Network**: Reliable internet connection for multiplayer functionality.

## Extra promts

- We feel very sorry for git commit story(( Plz, try to understand that this work was being adjusted in last 4 fours before deadline. Sincerely apologizing, we promise to be better in future 🙏

We hope you enjoy playing "Wondrous Field" as much as we've enjoyed creating it.

Happy gaming!

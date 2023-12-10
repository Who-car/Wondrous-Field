﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using PackageHelper;
using ClientServerTransfer;

namespace Server
{
    public class TCPServer
    {

        readonly Socket _listener;
        readonly Dictionary<string, Session> _privateSessions;
        readonly Dictionary<string, Session> _freeSessions;

        public TCPServer(IPAddress ipAddress, int port)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(ipAddress, port));
            _privateSessions = new();
            _freeSessions = new();
        }

        public async Task RunServerAsync()
        {
            try
            {
                _listener.Listen();
                Console.WriteLine("...Сервер запущен...");
                do
                {
                    var clientSocket = await _listener.AcceptAsync();

                    //_clients.Add(clientSocket, null!);

                    _ = Task.Run(
                        async() => 
                            await ProcessClientSocketAsync(clientSocket));

                } while (true);
            }
            catch (Exception ex)
            {
                //TODO: журналирование
            }
            finally
            {
                _listener.Close();
                Console.WriteLine("...Сервер завершил работу...");
            }
        }

        async Task ProcessClientSocketAsync(Socket socket)
        {
            try
            {
                var query = await Package.GetFullContent(socket);

                if (query.Command!.Equals(Command.CreateSession))
                {
                    await CreateSession(Encoding.UTF8.GetString(query.Body!), socket);

                    while(socket.Connected)
                    {

                    }
                }
                else if (query.Command.Equals(Command.Join))
                {
                    await JoinToSession(Encoding.UTF8.GetString(query.Body!), socket, "");

                    while(socket.Connected)
                    {

                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                //TODO: Exception
                await socket.DisconnectAsync(false);
            }
        }

        async Task<bool> CreateSession(string name, Socket player)
        {
            try
            {
                Session session = new();
                session.AddPlayer(name, player);
                _privateSessions.Add(session.SessionId, session);
                await SendResponseToUser(player, await Serialiser.SerialiseToBytes(new ConnectionInfo { SessionId = "", IsSuccessfullJoin = true }));

                return true;
            }
            catch
            {
                return false;
            }
        }

        async Task<bool> JoinToSession(string name, Socket player, string sessionId)
        {
            try
            {
                if(sessionId != null)
                {
                    if (_privateSessions.ContainsKey(sessionId))
                    {
                        _privateSessions[sessionId].AddPlayer(name, player);
                        await SendResponseToUser(player, await Serialiser.SerialiseToBytes(new ConnectionInfo { SessionId = "", IsSuccessfullJoin = true }));
                    }
                }
                else
                {

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        async Task SendResponseToUser(Socket socket, byte[] content)
        {
            var packages = Package.GetPackages(content, Command.Post, QueryType.Response);

            foreach(var package in packages)
            {
                await socket.SendAsync(package, SocketFlags.None);
            }
        }
    }
}

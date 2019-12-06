using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Text.RegularExpressions;

namespace MeetingsScheduleV2
{
    class Client
    {
        public delegate int CreateRemoteAsyncDelegate(CreateCommand command);
        public delegate List<MeetingProposal> ListRemoteAsyncDelegate(ListCommand command);
        public delegate int JoinRemoteAsyncDelegate(JoinCommand command);
        public delegate int CloseRemoteAsyncDelegate(CloseCommand command);
        public delegate int WaitRemoteAsyncDelegate(WaitCommand command);


        static void Main(string[] args)
        {
            try
            {
                // Client Information
                string username = args[0];
                string client_url = args[1];
                string server_url = args[2];

                // Start Connection
                Regex r = new Regex(@"^(?<protocol>\w+)://[^/]+?:(?<port>\d+)?/",
                              RegexOptions.None, TimeSpan.FromMilliseconds(100));
                Match m = r.Match(client_url);
                int port = Int32.Parse(m.Result("${port}"));

                BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
                IDictionary props = new Hashtable();
                props["port"] = port;
                props["timeout"] = 5000; // in milliseconds
                TcpChannel channel = new TcpChannel(props, null, provider);
                //TcpChannel channel = new TcpChannel(port);

                ClientObject client = new ClientObject();
                client.setUsername(username);
                client.setUrl(client_url);
                client.setPreferedServerUrl(server_url);
                RemotingServices.Marshal(client, "ClientObject", typeof(ClientObject));

                ChannelServices.RegisterChannel(channel, false);

                // Read Commands
                int sequeneNumber = 0;
                InstructsParser parser = new InstructsParser();

                string clientScript = args[3];

                string[] lines = File.ReadAllLines(clientScript);

                foreach (string line in lines)
                {
                    if (args.Length == 5)
                    {
                        Console.WriteLine("Press any key to execute next step: " + line);
                        Console.ReadLine();
                    }

                    ServerInterface server = client.getServer(server_url);

                    if (server == null)
                    {
                        Console.WriteLine("Server is unreachable");
                        return;
                    }

                    server.addClient(client_url);

                    char[] delimiter = { ' ' };
                    string[] instructionParts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    string myId = username + "-" + client_url;
                    if (instructionParts[0] == "create")
                    {
                        CreateCommand command = parser.parseCreateCommand(instructionParts, myId);
                        command.setIssuerId(myId);
                        command.setSentByClient(true);
                        command.setSequenceNumber(sequeneNumber);
                        command.setTimestamp(DateTime.Now);
                        sequeneNumber++; 
                        Console.WriteLine(command.getType() + " - " + command.getSequenceNumber());

                        int res = -1;
                        while (res == -1)
                        {
                            try
                            {
                                res = server.execute(command);
                            }
                            catch (System.Net.Sockets.SocketException)
                            {
                                Console.WriteLine("Timeout: server " + client.getPreferedServerUrl() + " seems to be frozen, trying another server");
                                server = client.getServer("ANOTHER_SERVER");
                            }
                        }
                        
                        Console.WriteLine("Result: {0}", res);
                        

                        // gossip
                        List<string> clients = client.getClientsForGossip();
                        client.gossip(command.getMeetingProposal(), clients);
                    }
                    else if (instructionParts[0] == "list")
                    {
                        ListCommand command = parser.parseListCommand(instructionParts);
                        command.setIssuerId(myId);
                        command.setSentByClient(true);
                        command.setSequenceNumber(sequeneNumber);
                        command.setTimestamp(DateTime.Now);
                        sequeneNumber++; // lock it
                        Console.WriteLine(command.getType());

                        List<MeetingProposal> proposals = null;
                        
                        while(proposals == null)
                        {
                            try
                            {
                                proposals = server.execute(command);
                            }
                            catch (System.Net.Sockets.SocketException)
                            {
                                Console.WriteLine("Timeout: server " + client.getPreferedServerUrl() + " seems to be frozen, trying another server");
                                server = client.getServer("ANOTHER_SERVER");
                            }
                        }

                        client.listMeetings(proposals);
                    }
                    else if (instructionParts[0] == "join")
                    {
                        JoinCommand command = parser.parseJoinCommand(instructionParts);
                        command.setIssuerId(myId);
                        command.setSentByClient(true);
                        command.setSequenceNumber(sequeneNumber);
                        command.setTimestamp(DateTime.Now);
                        sequeneNumber++; // lock it
                        Console.WriteLine(command.getType());

                        int res = -1;
                        while (res == -1)
                        {
                            try
                            {
                                res = server.execute(command);
                            }
                            catch (System.Net.Sockets.SocketException)
                            {
                                Console.WriteLine("Timeout: server " + client.getPreferedServerUrl() + " seems to be frozen, trying another server");
                                server = client.getServer("ANOTHER_SERVER");
                            }
                        }

                        Console.WriteLine("Result: {0}", res);
                    }
                    else if (instructionParts[0] == "close")
                    {
                        CloseCommand command = parser.parseCloseCommand(instructionParts);
                        command.setIssuerId(myId);
                        command.setSentByClient(true);
                        command.setSequenceNumber(sequeneNumber);
                        command.setTimestamp(DateTime.Now);
                        sequeneNumber++; // lock it
                        Console.WriteLine(command.getType());

                        int res = -1;
                        while (res == -1)
                        {
                            try
                            {
                                res = server.execute(command);
                            }
                            catch (System.Net.Sockets.SocketException)
                            {
                                Console.WriteLine("Timeout: server " + client.getPreferedServerUrl() + " seems to be frozen, trying another server");
                                server = client.getServer("ANOTHER_SERVER");
                            }
                        }

                        Console.WriteLine("Result: {0}", res);
                    }
                    else if (instructionParts[0] == "wait")
                    {
                        WaitCommand command = parser.parseWaitCommand(instructionParts);
                        command.setIssuerId(myId);
                        command.setSentByClient(true);
                        command.setSequenceNumber(sequeneNumber);
                        command.setTimestamp(DateTime.Now);
                        sequeneNumber++; // lock it
                        Console.WriteLine(command.getType());

                        int res = -1;
                        while (res == -1)
                        {
                            try
                            {
                                res = server.execute(command);
                            }
                            catch (System.Net.Sockets.SocketException)
                            {
                                Console.WriteLine("Timeout: server " + client.getPreferedServerUrl() + " seems to be frozen, trying another server");
                                server = client.getServer("ANOTHER_SERVER");
                            }
                        }

                        Console.WriteLine("Result: {0}", res);
                    }
                    else
                    {
                        NotFoundCommand command = new NotFoundCommand();
                        Console.WriteLine(command.getType());
                    }
                }

                Console.ReadLine();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace);

                // diagnostic: to see the error before exiting
                System.Threading.Thread.Sleep(60000);
            }
        }
    }

    class ClientObject : MarshalByRefObject, ClientInterface
    {
        string username;
        string url;
        string preferedServerUrl;
        public delegate int PullRemoteAsyncDelegate(MeetingProposal meeting);

        public void setUsername(string username)
        {
            this.username = username;
        }
        public void setPreferedServerUrl(string server_url)
        {
            this.preferedServerUrl = server_url;
        }

        public string getPreferedServerUrl()
        {
            return this.preferedServerUrl;
        }

        public ServerInterface getServer(string server_url)
        {
            ServerInterface server = null;
            try
            {
                server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), server_url);
            }
            catch (Exception)
            {
                Console.WriteLine("trying new server");
                string serversList = "servers.txt";
                string[] serversURLs = File.ReadAllLines(serversList);
                foreach (string url in serversURLs)
                {
                    if (url != this.preferedServerUrl)
                    {
                        try
                        {
                            server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), url);
                            this.setPreferedServerUrl(url);
                            Console.WriteLine("the new server is {0}", url);
                            return server;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }
            return server;
        }

        public List<string> getClientsForGossip()
        {
            SortedSet<string> result = new SortedSet<string>();
            string serversList = "servers.txt";
            string[] serversURLs = File.ReadAllLines(serversList);
            foreach (string url in serversURLs)
            {
                try
                {
                    ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), url);
                    string suggested_client = server.getRandomClient();
                    result.Add(suggested_client);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            List<string> results_list = new List<string>();
            foreach(string client in result)
            {
                results_list.Add(client);
            }
            return results_list;
        }

        public void setUrl(string url)
        {
            this.url = url;
        }

        public void listMeetings(List<MeetingProposal> meetings)
        {
            foreach(MeetingProposal meeting in meetings)
            {
                string closed = "Open";
                if (meeting.isClosed())
                {
                    closed = "Closed";
                }
                if (meeting.isCancelled())
                {
                    closed = "Cancelled";
                }
                Console.WriteLine(meeting.getCoordinator() + " " + meeting.getTopic() + " - " + closed);
            }
        }

        public int status()
        {
            return 1;
        }

        // Gossiping
        List<string> servers = new List<string>();
        SortedSet<string> gossipedMeetings = new SortedSet<string>();

        public void setNodes()
        {
            string serversList = "servers.txt";
            string[] lines = File.ReadAllLines(serversList);
            foreach(string line in lines)
            {
                this.servers.Add(line);
            }
        }

        public void informServers()
        {
            this.setNodes();
            List<string> nodesToRemove = new List<string>();
            foreach(string url in this.servers)
            {
                ServerInterface server = null;
                try
                {
                    server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), url);
                    if(server != null)
                    {
                        server.addClient(this.url);
                    }
                    else
                    {
                        nodesToRemove.Add(url);
                    }
                }
                catch (Exception)
                {
                    nodesToRemove.Add(url);
                }
            }

            foreach(string node in nodesToRemove)
            {
                this.servers.Remove(node);
            }
        }

        public int pull(MeetingProposal meeting)
        {
            if (this.gossipedMeetings.Contains(meeting.getTopic()))
            {
                // Console.WriteLine(this.url + " stopped " + command.commandId());
                return 0;
            }

            List<string> invitees = meeting.getInvitees();
            if (invitees != null)
            {
                if (invitees.Contains(this.username))
                {
                    Console.WriteLine("You are invited to the meeting " + meeting.getTopic());
                }
            }
            else
            {
                Console.WriteLine("A meeting " + meeting.getTopic() + " was created");
            }

            this.gossipedMeetings.Add(meeting.getTopic());

            List<string> clients = this.getClientsForGossip();
            this.gossip(meeting, clients);

            return 1;
        }

        public int push(MeetingProposal meeting, string node)
        {
            // Console.WriteLine(node + " pushed  " + command.commandId());
            ClientInterface client = null;
            try
            {
                client = (ClientInterface)Activator.GetObject(typeof(ClientInterface), node);
                if (client != null)
                {
                    PullRemoteAsyncDelegate RemoteDel = new PullRemoteAsyncDelegate(client.pull);
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(meeting, null, null);
                    RemAr.AsyncWaitHandle.WaitOne();
                    int status = RemoteDel.EndInvoke(RemAr);
                    return status;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int gossip(MeetingProposal meeting, List<string> clients)
        {
            int len = clients.Count;
            Random rnd = new Random();

            int numberOfGossipMembers = 3;

            SortedSet<int> gossiped_members = new SortedSet<int>();

            for (int i = 0; i < numberOfGossipMembers; i++)
            {
                int r = rnd.Next(0, len);

                int nr_tries = 0;
                while (gossiped_members.Contains(r))
                {
                    r = rnd.Next(0, len);
                    nr_tries++;
                    if (nr_tries == len)
                    {
                        break;
                    }
                }

                this.push(meeting, clients[r]);
                gossiped_members.Add(r);
            }

            return 0;
        }
    }
}

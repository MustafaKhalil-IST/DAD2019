using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text.RegularExpressions;

namespace MeetingsScheduleV2
{
    class Server
    {
        static void Main(string[] args)
        {
            // server information
            string id = args[0];
            string url = args[1];
            int max_faults = Int32.Parse(args[2]);
            int min_delay = Int32.Parse(args[3]);
            int max_delay = Int32.Parse(args[4]);

            // establish connection
            Regex r = new Regex(@"^(?<protocol>\w+)://[^/]+?:(?<port>\d+)?/",
                          RegexOptions.None, TimeSpan.FromMilliseconds(100));
            Match m = r.Match(url);
            int port = Int32.Parse(m.Result("${port}"));

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            ServerObject server = new ServerObject(id, url, max_faults, min_delay, max_delay);
            RemotingServices.Marshal(server, "ServerObject", typeof(ServerObject));

            Console.WriteLine("<enter> to exit...");
            Console.ReadLine();
        }
    }

    class ServerObject : MarshalByRefObject, ServerInterface
    {
        // Private Attributes
        private string id;
        private string url;
        private int max_faults;
        private int min_delay;
        private int max_delay;

        private bool isFrozen = false;
        Dictionary<string, HashSet<MeetingProposal>> meetings = new Dictionary<string, HashSet<MeetingProposal>>();
        RoomsManager roomsManager = new RoomsManager();
        List<Command> frozenCommands = new List<Command>();

        // a server is aware of all servers and its clients
        SortedSet<string> servers = new SortedSet<string>();
        SortedSet<string> clients = new SortedSet<string>();

        // intra-servers communication
        public delegate int CreateRemoteAsyncDelegate(CreateCommand command);
        public delegate List<MeetingProposal> ListRemoteAsyncDelegate(ListCommand command);
        public delegate int JoinRemoteAsyncDelegate(JoinCommand command);
        public delegate int CloseRemoteAsyncDelegate(CloseCommand command);
        public delegate int WaitRemoteAsyncDelegate(WaitCommand command);

        public ServerObject(string id, string url, int max_faults, int min_delay, int max_delay)
        {
            this.id = id;
            this.url = url;
            this.max_faults = max_faults;
            this.min_delay = min_delay;
            this.max_delay = max_delay;

            string serversList = "servers.txt";
            string[] lines = File.ReadAllLines(serversList);
            foreach (string line in lines)
            {
                this.servers.Add(line);
            }
        }

        private MeetingProposal restoreLostMeeting(string topic)
        {
            Console.WriteLine("Restoring " + topic);
            string serversList = "servers.txt";
            string[] serversURLs = File.ReadAllLines(serversList);
            foreach (string url in serversURLs)
            {
                ServerObject server = (ServerObject)Activator.GetObject(typeof(ServerObject), url);
                if (server != null)
                {
                    MeetingProposal proposal = server.getMeetingByTopic(topic);
                    MeetingProposal proposal_copy = new MeetingProposal(proposal);

                    if (proposal_copy != null)
                    {
                        this.createMeeting(proposal_copy.getCoordinator(), proposal_copy);
                        return proposal_copy;
                    }
                }
            }
            return null;
        }

        private void createMeeting(string client_id, MeetingProposal proposal)
        {
            Monitor.Enter(this.meetings);
            proposal.setRoomsManager(this.roomsManager);

            if (!this.meetings.ContainsKey(client_id))
            {
                this.meetings[client_id] = new HashSet<MeetingProposal>();
            }

            bool already_exists = false;
            foreach (MeetingProposal meetingProposal in this.meetings[client_id])
            {
                if (meetingProposal.getTopic() == proposal.getTopic())
                {
                    already_exists = true;
                    break;
                }
            }

            if (!already_exists)
            {
                this.meetings[client_id].Add(proposal);
            }
            else
            {
                // merge two meetings
                MeetingProposal original = null;
                foreach (MeetingProposal meetingProposal in this.meetings[client_id])
                {
                    if (meetingProposal.getTopic() == proposal.getTopic())
                    {
                        original = meetingProposal;
                        this.meetings[client_id].Remove(meetingProposal);
                        break;
                    }
                }
                original = this.mergeTwoMeetings(original, proposal);
                this.meetings[client_id].Add(original);

            }
            Monitor.Exit(this.meetings);
        }

        // Private Methods
        private MeetingProposal mergeTwoMeetings(MeetingProposal proposal1, MeetingProposal proposal2)
        {
            Console.WriteLine("merging meetings");
            foreach (string participant in proposal1.getParticipants().Keys)
            {
                if (!proposal2.getParticipants().ContainsKey(participant))
                {
                    proposal2.addParticipant(participant, proposal1.getParticipants()[participant]);
                }
            }
            return proposal2;
        }

        private void delay()
        {
            Random random = new Random();
            int milliseconds = random.Next(this.min_delay, this.max_delay);
            Console.WriteLine("delay: " + milliseconds);
            Thread.Sleep(milliseconds);
        }

        public MeetingProposal getMeetingByTopic(string topic)
        {
            foreach (string client_id in this.meetings.Keys)
            {
                foreach (MeetingProposal proposal in this.meetings[client_id])
                {
                    if (proposal.getTopic() == topic)
                    {
                        return proposal;
                    }
                }
            }
            return null;
        }

        private void joinMeeting(string client_id, MeetingProposal proposal, List<Slot> desiredSlots)
        {
            Monitor.Enter(this.meetings);
            // client cannot join a meeting if it is closed or cancelled
            if (proposal.isClosed() || proposal.isCancelled())
            {
                return;
            }
            proposal.addParticipant(client_id, desiredSlots);
            Console.WriteLine(client_id + " joined " + proposal.getTopic());
            Monitor.Exit(this.meetings);
        }

        private MeetingProposal closeMeeting(string client_id, string topic)
        {
            Monitor.Enter(this.meetings);
            MeetingProposal proposal = this.getMeetingByTopic(topic);
            
            if(proposal == null)
            {
                return null;
            }

            proposal.close();
            Monitor.Exit(this.meetings);
            return proposal;
        }

        // Public Methods
        public int execute(CreateCommand command)
        {
            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);

                while (true)
                {
                    // freeze
                }

                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                this.createMeeting(command.getIssuerId(), command.getMeetingProposal());

                this.informOtherServers(command);

                return 1;
            }
        }

        public List<MeetingProposal> execute(ListCommand command)
        {
            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);

                while (true)
                {
                    // freeze
                }

                return new List<MeetingProposal>();
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());

                List<MeetingProposal> proposals = new List<MeetingProposal>();
                foreach (string client_id in this.meetings.Keys)
                {
                    foreach (MeetingProposal proposal in this.meetings[client_id])
                    {
                        // verify if client is invited
                        if (proposal.hasInvitedClient(client_id))
                        {
                            proposals.Add(proposal);
                        }
                    }
                }

                return proposals;
            }
        }

        public int execute(CloseCommand command)
        {
            //TODO: check causal consistency and total order

            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);

                while (true)
                {
                    // freeze
                }

                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                MeetingProposal meeting = this.closeMeeting(command.getIssuerId(), command.getTopic());

                if (meeting == null)
                {
                    meeting = this.restoreLostMeeting(command.getTopic());
                    
                    if (meeting == null)
                    {
                        Console.WriteLine("Meeting not found");
                        return 0;
                    }
                }

                meeting.setClosingTimestamp(command.getTimestamp());
                meeting.close();

                this.informOtherServers(command);

                return 1;
            }
        }

        public int execute(JoinCommand command)
        {

            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);

                while (true)
                {
                    // freeze
                }

                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + "-" +
                    command.getSequenceNumber() + " command from " + command.getIssuerId());

                MeetingProposal meeting = this.getMeetingByTopic(command.getTopic());

                if (meeting == null)
                {
                    Console.WriteLine("Meeting {0} not found, Contacting other servers ...", command.getTopic());
                    meeting = this.restoreLostMeeting(command.getTopic());

                    // Console.WriteLine("Meeting {0} not found, Waiting ...");
                    // Thread.Sleep(2000);

                    if (meeting == null)
                    {
                        Console.WriteLine("Meeting {0} not found", command.getTopic());
                        return 0;
                    }
                }

                if (meeting.isClosed() || meeting.isCancelled())
                {
                    // a client can join a closed meeting if it was joined before the closing momment
                    if (command.getTimestamp().CompareTo(meeting.getClosingTimestamp()) < 0)
                    {
                        meeting.open();
                        this.joinMeeting(command.getIssuerId(), meeting, command.getDesiredSlots());
                        meeting.close();
                    }
                }
                else
                {
                    this.joinMeeting(command.getIssuerId(), meeting, command.getDesiredSlots());
                }

                this.informOtherServers(command);

                return 1;
            }
        }

        public int execute(WaitCommand command)
        {
            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);

                while (true)
                {
                    // freeze
                }

                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                Thread.Sleep(command.getMilliseconds());
                return 0;
            }
        }

        public int execute(NotFoundCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            return 0;
        }

        public void addRoom(Room room)
        {
            Console.WriteLine("Room " + room.getID() + " Added");
            this.roomsManager.addRoom(room);
        }

        public void crash()
        {
            Environment.Exit(-1);
        }

        public int status()
        {
            Console.WriteLine("Status - Frozen: " + this.isFrozen);
            Console.WriteLine("Status - Rooms");
            foreach (string location in this.roomsManager.getRooms().Keys)
            {
                foreach (Room room in this.roomsManager.getRooms()[location])
                {
                    Console.WriteLine("Room " + room.getID() + " " + room.getLocation() + " " + room.getCapacity());
                }
            }

            Console.WriteLine("Status - Meetings");
            foreach (string client in this.meetings.Keys)
            {
                foreach (MeetingProposal meeting in this.meetings[client])
                {
                    Console.WriteLine("Meeting " + meeting.getTopic());
                    Console.WriteLine(" -- Coordinator " + meeting.getCoordinator());
                    if (meeting.getSelectedRoom() != null)
                    {
                        Console.WriteLine(" -- Selected Room " + meeting.getSelectedRoom().getLocation() + " " + meeting.getSelectedRoom().getID());
                    }
                    Console.WriteLine(" -- Participants");
                    foreach (string participant in meeting.getFinalParticipants())
                    {
                        Console.WriteLine(" ---- " + participant);
                    }
                    Console.WriteLine(" -- Is Closed: " + meeting.isClosed());
                    Console.WriteLine(" -- Is Cancelled: " + meeting.isCancelled());
                }
            }

            return 1;
        }

        public void unfreeze()
        {
            Console.WriteLine("Unfreeze");
            this.isFrozen = false;

            // avoid executing repeated commands
            SortedSet<Command> executedCommands = new SortedSet<Command>();

            for (int i = 0; i < this.frozenCommands.Count; i++)
            {
                Command command = this.frozenCommands[i];

                if(executedCommands.Contains(command))
                {
                    Console.WriteLine(command.commandId() + " is duplicated, will not be executed");
                    continue;
                }

                if (command.getType() == "CREATE")
                {
                    CreateCommand c = (CreateCommand)command;
                    this.execute(c);
                }
                else if (command.getType() == "LIST")
                {
                    ListCommand c = (ListCommand)command;
                    this.execute(c);
                }
                else if (command.getType() == "CLOSE")
                {
                    CloseCommand c = (CloseCommand)command;
                    this.execute(c);
                }
                else if (command.getType() == "JOIN")
                {
                    JoinCommand c = (JoinCommand)command;
                    this.execute(c);
                }
                else if (command.getType() == "WAIT")
                {
                    WaitCommand c = (WaitCommand)command;
                    this.execute(c);
                }

                executedCommands.Add(command);
            }
            this.frozenCommands = new List<Command>();
        }

        public void freeze()
        {
            Console.WriteLine("Freeze");
            this.isFrozen = true;
        }

        public void informOtherServers(Command command)
        {
            Console.WriteLine("Inform other servers");

            // passive replication strategy
            if (command.isSentByClient())
            {
                if (command.getType() == "CREATE" || command.getType() == "JOIN" || command.getType() == "CLOSE")
                {
                    foreach (string serverURL in this.servers)
                    {
                        // TODO Asynchronous calls maybe a better idea
                        if (serverURL != this.url)
                        {
                            try
                            {
                                ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), serverURL);
                                switch (command.getType())
                                {
                                    case "CREATE":
                                        command.setSentByClient(false);
                                        CreateRemoteAsyncDelegate RemoteDel = new CreateRemoteAsyncDelegate(server.execute);
                                        IAsyncResult RemAr = RemoteDel.BeginInvoke((CreateCommand)command, null, null);

                                        // server.execute((CreateCommand)command);
                                        break;
                                    case "JOIN":
                                        command.setSentByClient(false);
                                        JoinRemoteAsyncDelegate RemoteDel2 = new JoinRemoteAsyncDelegate(server.execute);
                                        IAsyncResult RemAr2 = RemoteDel2.BeginInvoke((JoinCommand)command, null, null);

                                        // server.execute((JoinCommand)command);
                                        break;
                                    case "CLOSE":
                                        command.setSentByClient(false);
                                        CloseRemoteAsyncDelegate RemoteDel3 = new CloseRemoteAsyncDelegate(server.execute);
                                        IAsyncResult RemAr3 = RemoteDel3.BeginInvoke((CloseCommand)command, null, null);

                                        // server.execute((CloseCommand)command);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine(serverURL + " FAULT");
                                // Faults tolerance TODO
                            }
                        }
                    }
                }
            }
        }

        // Gossip
        public List<string> suggestClientsForGossip(MeetingProposal meeting)
        {
            List<string> result = new List<string>();
            List<string> invitees = meeting.getInvitees();

            if (invitees != null)
            {
                foreach (string client in this.clients)
                {
                    foreach(string invitee in invitees)
                    { 
                        if (client.StartsWith(invitee))
                        {
                            result.Add(client);
                        }
                    }
                }
            }
            else
            {
                foreach (string client in this.clients)
                {
                    char[] delimiter = { '-' };
                    string[] clientInfo = client.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    result.Add(clientInfo[0]);
                }
            }

            return result;
        }

        public delegate int PullRemoteAsyncDelegate(MeetingProposal proposal);

        public void addClient(string client)
        {
            this.clients.Add(client);
            Console.WriteLine("Client " + client + " is connected");
        }

        public string getRandomClient()
        {
            Random rnd = new Random();
            List<string> temp = new List<string>();
            foreach(string client in this.clients)
            {
                temp.Add(client);
            }
            return temp[rnd.Next(0, temp.Count)];
        }
    }
}

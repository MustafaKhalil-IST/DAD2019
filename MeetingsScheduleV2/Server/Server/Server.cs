using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;
using System.Text.RegularExpressions;

namespace MeetingsScheduleV2
{
    class Server
    {
        static void Main(string[] args)
        {
            string id = args[0];
            string url = args[1];
            int max_faults = Int32.Parse(args[2]);
            int min_delay = Int32.Parse(args[3]);
            int max_delay = Int32.Parse(args[4]);

            Regex r = new Regex(@"^(?<protocol>\w+)://[^/]+?:(?<port>\d+)?/",
                          RegexOptions.None, TimeSpan.FromMilliseconds(100));
            Match m = r.Match(url);
            int port = Int32.Parse(m.Result("${port}"));

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);

            ServerObject server = new ServerObject(id, url, max_faults, min_delay, max_delay);
            RemotingServices.Marshal(server, "ServerObject", typeof(ServerObject));

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
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
        string[] serversURLs;

        public ServerObject(string id, string url, int max_faults, int min_delay, int max_delay)
        {
            this.id = id;
            this.url = url;
            this.max_faults = max_faults;
            this.min_delay = min_delay;
            this.max_delay = max_delay;

            string serversList = "servers.txt";
            this.serversURLs = File.ReadAllLines(serversList);
        }

        private void createMeeting(string client_id, MeetingProposal proposal)
        {
            proposal.setRoomsManager(this.roomsManager);
            
            if (!this.meetings.ContainsKey(client_id))
            {
                this.meetings[client_id] = new HashSet<MeetingProposal>();
            } 
            this.meetings[client_id].Add(proposal);
        }

        // Private Methods

        private void delay()
        {
            Random random = new Random();
            int milliseconds = random.Next(this.min_delay, this.max_delay);
            Console.WriteLine("delay: " + milliseconds);
            System.Threading.Thread.Sleep(milliseconds);
        }

        private MeetingProposal getMeetingByTopic(string topic)
        {
            foreach(string client_id in this.meetings.Keys)
            {
                foreach(MeetingProposal proposal in this.meetings[client_id])
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
            proposal.addParticipant(client_id, desiredSlots);
        }

        private void closeMeeting(string client_id, string topic)
        {
            foreach(MeetingProposal proposal in this.meetings[client_id])
            {
                if(proposal.getTopic() == topic)
                {
                    proposal.close();
                    break;
                }
            }
        }

        private void cancelMeeting(string client_id, string topic)
        {
            foreach (MeetingProposal proposal in this.meetings[client_id])
            {
                if (proposal.getTopic() == topic)
                {
                    proposal.cancel();
                }
            }
        }

        // Public Methods
        public int execute(CreateCommand command)
        {
            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);
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

        public int execute(ListCommand command)
        {
            // delay 
            this.delay();

            // maybe it should return list instead of int
            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);
                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());

                List<MeetingProposal> proposals = new List<MeetingProposal>();
                foreach (string client_id in this.meetings.Keys)
                {
                    foreach (MeetingProposal proposal in this.meetings[client_id])
                    {
                        proposals.Add(proposal);
                    }
                }

                char[] delimiter = { '-' };
                string[] client_info = command.getIssuerId().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                string client_url = client_info[1];

                ClientInterface client = (ClientInterface)Activator.GetObject(typeof(ServerInterface), client_url);
                if (client == null)
                    System.Console.WriteLine("Could not locate client " + command.getIssuerId());
                else
                {
                    client.listMeetings(proposals);
                }
                return 1;
            }
        }

        public int execute(CloseCommand command)
        {
            // delay 
            this.delay();

            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);
                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                this.closeMeeting(command.getIssuerId(), command.getTopic());

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
                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                this.joinMeeting(command.getIssuerId(), this.getMeetingByTopic(command.getTopic()), command.getDesiredSlots());

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
                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                System.Threading.Thread.Sleep(command.getMilliseconds());
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
                foreach(Room room in this.roomsManager.getRooms()[location])
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
                    Console.WriteLine(" -- Participants");
                    foreach(string participant in meeting.getParticipants().Keys)
                    {
                        Console.WriteLine(" ---- " + participant);
                    }
                    Console.WriteLine(" -- Is Closed: " + meeting.isClosed());
                }
            }

            return 1;
        }

        public void unfreeze()
        {
            Console.WriteLine("Unfreeze");
            this.isFrozen = false;

            for(int i = 0; i < this.frozenCommands.Count; i++)
            {
                Command command = this.frozenCommands[i];

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

                this.informOtherServers(command);
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
            if (command.isSentByClient())
            {
                if (command.getType() == "CREATE" || command.getType() == "JOIN" || command.getType() == "CLOSE")
                {
                    foreach (string serverURL in this.serversURLs)
                    {
                        // Asynchronous calls maybe a better idea
                        if (serverURL != this.url)
                        {
                            try
                            {
                                ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), serverURL);
                                switch (command.getType())
                                {
                                    case "CREATE":
                                        command.setSentByClient(false);
                                        server.execute((CreateCommand)command);
                                        break;
                                    case "JOIN":
                                        command.setSentByClient(false);
                                        server.execute((JoinCommand)command);
                                        break;
                                    case "CLOSE":
                                        command.setSentByClient(false);
                                        server.execute((CloseCommand)command);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(serverURL + " FAULT");
                                // Faults tolerance TODO
                            }
                        }
                    }
                }
            }
        }
    }
}

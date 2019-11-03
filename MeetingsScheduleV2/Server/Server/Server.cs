using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MeetingsScheduleV2
{
    class Server
    {
        static void Main(string[] args)
        {
            string id = args[0];
            int url = Int32.Parse(args[1]);
            int max_faults = Int32.Parse(args[2]);
            int min_delay = Int32.Parse(args[3]);
            int max_delay = Int32.Parse(args[4]);

            TcpChannel channel = new TcpChannel(url);
            ChannelServices.RegisterChannel(channel, false);

            ServerObject server = new ServerObject(id, url, max_faults, min_delay, max_delay);
            RemotingServices.Marshal(server, "ServerObject", typeof(ServerObject));

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    class ServerObject : MarshalByRefObject, ServerInterface
    {
        // Private Methods
        private string id;
        private int url;
        private int max_faults;
        private int min_delay;
        private int max_delay;

        private bool isFrozen = false;
        Dictionary<string, HashSet<MeetingProposal>> meetings = new Dictionary<string, HashSet<MeetingProposal>>();
        RoomsManager roomsManager = new RoomsManager();
        List<Command> frozenCommands = new List<Command>();

        public ServerObject(string id, int url, int max_faults, int min_delay, int max_delay)
        {
            this.id = id;
            this.url = url;
            this.max_faults = max_faults;
            this.min_delay = min_delay;
            this.max_delay = max_delay;
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
            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);
                return 0;
            }
            else 
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                this.createMeeting(command.getIssuerId(), command.getMeetingProposal());
                return 1;
            }
        }

        public int execute(ListCommand command)
        {
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
            if(this.isFrozen)
            {
                this.frozenCommands.Add(command);
                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                this.closeMeeting(command.getIssuerId(), command.getTopic());
                return 1;
            }
        }

        public int execute(JoinCommand command)
        {
            if (this.isFrozen)
            {
                this.frozenCommands.Add(command);
                return 0;
            }
            else
            {
                Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
                this.joinMeeting(command.getIssuerId(), this.getMeetingByTopic(command.getTopic()), command.getDesiredSlots());
                return 1;
            }
        }

        public int execute(WaitCommand command)
        {
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
            }
            this.frozenCommands = new List<Command>();
        }

        public void freeze()
        {
            Console.WriteLine("Freeze");
            this.isFrozen = true;
        }
    }
}

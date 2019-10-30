using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MeetingsSchedule
{
    class Server
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, false);

            ServerObject server = new ServerObject();
            RemotingServices.Marshal(server, "ServerObject", typeof(ServerObject));

            System.Console.WriteLine("<enter> to exit...");
            System.Console.ReadLine();
        }
    }

    class ServerObject : MarshalByRefObject, ServerInterface
    {
        // Private Methods
        Dictionary<string, HashSet<MeetingProposal>> meetings = new Dictionary<string, HashSet<MeetingProposal>>();

        private void createMeeting(string client_id, MeetingProposal proposal)
        {
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
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            this.createMeeting(command.getIssuerId(), command.getMeetingProposal());
            return 0;
        }

        public int execute(ListCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());

            List<MeetingProposal> proposals = new List<MeetingProposal>();
            foreach(string client_id in this.meetings.Keys)
            {
                foreach(MeetingProposal proposal in this.meetings[client_id])
                {
                    proposals.Add(proposal);
                }
            }

            char[] delimiter = { '-' };
            string[] client_info = command.getIssuerId().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            string port = client_info[1];

            ClientInterface client = (ClientInterface)Activator.GetObject(typeof(ServerInterface),
                                                                           "tcp://localhost:" + port + "/ClientObject");
            if (client == null)
                System.Console.WriteLine("Could not locate client " + command.getIssuerId());
            else
            {
                client.listMeetings(proposals);
            }
            return 0;
        }

        public int execute(CloseCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            this.closeMeeting(command.getIssuerId(), command.getTopic());
            return 0;
        }

        public int execute(JoinCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            this.joinMeeting(command.getIssuerId(), this.getMeetingByTopic(command.getTopic()), command.getDesiredSlots());
            return 0;
        }

        public int execute(WaitCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            System.Threading.Thread.Sleep(command.getMilliseconds());
            return 0;
        }

        public int execute(NotFoundCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            return 0;
        }

        public void crash()
        {
        }

        public void status()
        {
        }

        public void unfreeze()
        {
        }

        public void freeze()
        {
        }
    }
}

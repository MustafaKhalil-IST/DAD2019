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
        Dictionary<string, MeetingProposal> meetingsPerTopic = new Dictionary<string, MeetingProposal>();

        private void createMeeting(string client_id, MeetingProposal proposal)
        {
            this.meetings[client_id].Add(proposal);
            this.meetingsPerTopic.Add(proposal.getTopic(), proposal);
        }

        private MeetingProposal getMeetingByTopic(string topic)
        {
            return this.meetingsPerTopic[topic];
        }

        private void joinMeeting(string client_id, MeetingProposal proposal)
        {
            proposal.addParticipant(client_id);
        }

        private void closeMeeting(string client_id, string topic)
        {
            foreach(MeetingProposal proposal in this.meetings[client_id])
            {
                if(proposal.getTopic() == topic)
                {
                    proposal.close();
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
            // get client port
            // open channel
            // prepare list of meetings
            // send it
            List<MeetingProposal> proposals = new List<MeetingProposal>();
            foreach(string topic in this.meetingsPerTopic.Keys)
            {
                proposals.Add(this.meetingsPerTopic[topic]);
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
            this.joinMeeting(command.getIssuerId(), this.getMeetingByTopic(command.getTopic()));
            return 0;
        }

        public int execute(WaitCommand command)
        {
            Console.WriteLine("Recieved " + command.getType() + " command from " + command.getIssuerId());
            // sleep ??
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

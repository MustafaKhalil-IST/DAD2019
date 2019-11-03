using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingsSchedule
{
    class Client
    {
        private bool isFrozen = false;

        public delegate int RemoteAsyncDelegate(Command command);

        public static void OurRemoteAsyncCallBack(IAsyncResult ar)
        {
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + del.EndInvoke(ar));
            return;
        }

        static void Main(string[] args)
        {
            string username = args[0];
            int client_url = Int32.Parse(args[1]);
            string server_url = args[2];

            TcpChannel channel = new TcpChannel(client_url);
            ClientObject client = new ClientObject();
            RemotingServices.Marshal(client, "ClientObject", typeof(ClientObject));

            ChannelServices.RegisterChannel(channel, false);
            ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), server_url);
            if (server == null)
                // Contact another server : TODO
                System.Console.WriteLine("Could not locate server");
            else
            {
                System.Console.WriteLine("Found");
                InstructsParser parser = new InstructsParser();

                string clientScript = @"C:\Users\cash\MEIC\Development of Distributed Systems\DAD2019\MeetingsScheduleV1\" + args[2];

                string[] lines = File.ReadAllLines(clientScript);

                foreach (string line in lines)
                {
                    char[] delimiter = { ' ' };
                    string[] instructionParts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    string myId = username + "-" + client_url;
                    if (instructionParts[0] == "create")
                    {
                        CreateCommand command = parser.parseCreateCommand(instructionParts, myId);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());
                        server.execute(command);
                    } 
                    else if (instructionParts[0] == "list")
                    {
                        ListCommand command = parser.parseListCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());
                        server.execute(command);
                    }
                    else if (instructionParts[0] == "join")
                    {
                        JoinCommand command = parser.parseJoinCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());
                        server.execute(command);
                    }
                    else if (instructionParts[0] == "close")
                    {
                        CloseCommand command = parser.parseCloseCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());
                        server.execute(command);
                    }
                    else if (instructionParts[0] == "wait")
                    {
                        WaitCommand command = parser.parseWaitCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());
                        server.execute(command);
                    } else
                    {
                        NotFoundCommand command = new NotFoundCommand();
                        Console.WriteLine(command.getType());
                    }
                }
            }
            System.Console.ReadLine();
        }
    }

    class ClientObject : MarshalByRefObject, ClientInterface
    {
        public void addMeeting(MeetingProposal proposal)
        {
        }

        public MeetingProposal GetMeeting(string topic)
        {
            return null;
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

        public void crash()
        {
            Environment.Exit(-1);
        }
        public int status()
        {
            return 1;
        }
        public void unfreeze()
        {
        }
        public void freeze()
        {
        }
    }
}

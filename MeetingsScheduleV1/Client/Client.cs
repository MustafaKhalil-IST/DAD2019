using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;

namespace MeetingsSchedule
{
    class Client
    {
        public delegate int RemoteAsyncDelegate(Command command);

        public static void OurRemoteAsyncCallBack(IAsyncResult ar)
        {
            // Alternative 2: Use the callback to get the return value
            RemoteAsyncDelegate del = (RemoteAsyncDelegate)((AsyncResult)ar).AsyncDelegate;
            Console.WriteLine("\r\n**SUCCESS**: Result of the remote AsyncCallBack: " + del.EndInvoke(ar));
            return;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Please enter your id: ");
            string myId = Console.ReadLine();
            Console.WriteLine("Please enter your port number: ");
            int myPort = Int32.Parse(Console.ReadLine());
            myId += "-" + myPort;

            TcpChannel channel = new TcpChannel(myPort);
            ClientObject client = new ClientObject();
            RemotingServices.Marshal(client, "ClientObject", typeof(ClientObject));

            ChannelServices.RegisterChannel(channel, false);
            ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface),
                                                                           "tcp://localhost:8086/ServerObject");
            if (server == null)
                System.Console.WriteLine("Could not locate server");
            else
            {
                System.Console.WriteLine("Found");

                InstructsParser parser = new InstructsParser();

                string clientScript = args[0];

                string[] lines = File.ReadAllLines(clientScript);

                foreach (string line in lines)
                {
                    char[] delimiter = { ' ' };
                    string[] instructionParts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    if (instructionParts[0] == "create")
                    {
                        CreateCommand command = parser.parseCreateCommand(instructionParts);
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
                Console.WriteLine(meeting.getCoordinator() + " " + meeting.getTopic());
            }
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

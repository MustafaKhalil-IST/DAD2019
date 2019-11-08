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
using System.Text.RegularExpressions;

namespace MeetingsSchedule
{
    class Client
    {
        private bool isFrozen = false;

        public delegate int CreateRemoteAsyncDelegate(CreateCommand command);
        public delegate List<MeetingProposal> ListRemoteAsyncDelegate(ListCommand command);
        public delegate int JoinRemoteAsyncDelegate(JoinCommand command);
        public delegate int CloseRemoteAsyncDelegate(CloseCommand command);
        public delegate int WaitRemoteAsyncDelegate(WaitCommand command);

        static void Main(string[] args)
        {
            string myId = args[0];
            string url = args[1];
            string server_url = args[2];

            Regex r = new Regex(@"^(?<protocol>\w+)://[^/]+?:(?<port>\d+)?/",
                          RegexOptions.None, TimeSpan.FromMilliseconds(100));
            Match m = r.Match(url);
            int port = Int32.Parse(m.Result("${port}"));

            myId += "-" + url;

            TcpChannel channel = new TcpChannel(port);
            ClientObject client = new ClientObject();
            RemotingServices.Marshal(client, "ClientObject", typeof(ClientObject));

            ChannelServices.RegisterChannel(channel, false);
            ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), server_url);
            if (server == null)
                System.Console.WriteLine("Could not locate server");
            else
            {
                System.Console.WriteLine("Found");

                InstructsParser parser = new InstructsParser();

                string clientScript = @"C:\Users\cash\MEIC\Development of Distributed Systems\DAD2019\MeetingsScheduleV1\" + args[3];

                string[] lines = File.ReadAllLines(clientScript);

                foreach (string line in lines)
                {
                    char[] delimiter = { ' ' };
                    string[] instructionParts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                    if (instructionParts[0] == "create")
                    {
                        CreateCommand command = parser.parseCreateCommand(instructionParts, myId);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());

                        CreateRemoteAsyncDelegate RemoteDel = new CreateRemoteAsyncDelegate(server.execute);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, null, null);
                        RemAr.AsyncWaitHandle.WaitOne();
                        Console.WriteLine(RemoteDel.EndInvoke(RemAr));

                        // server.execute(command);
                    }
                    else if (instructionParts[0] == "list")
                    {
                        ListCommand command = parser.parseListCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());

                        ListRemoteAsyncDelegate RemoteDel = new ListRemoteAsyncDelegate(server.execute);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, null, null);
                        RemAr.AsyncWaitHandle.WaitOne();
                        List<MeetingProposal> proposals = RemoteDel.EndInvoke(RemAr);

                        // List<MeetingProposal> proposals = server.execute(command);

                        foreach (MeetingProposal meeting in proposals)
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
                    else if (instructionParts[0] == "join")
                    {
                        JoinCommand command = parser.parseJoinCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());

                        JoinRemoteAsyncDelegate RemoteDel = new JoinRemoteAsyncDelegate(server.execute);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, null, null);
                        RemAr.AsyncWaitHandle.WaitOne();
                        Console.WriteLine(RemoteDel.EndInvoke(RemAr));
                        // server.execute(command);
                    }
                    else if (instructionParts[0] == "close")
                    {
                        CloseCommand command = parser.parseCloseCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());

                        CloseRemoteAsyncDelegate RemoteDel = new CloseRemoteAsyncDelegate(server.execute);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, null, null);
                        RemAr.AsyncWaitHandle.WaitOne();
                        Console.WriteLine(RemoteDel.EndInvoke(RemAr));

                        //server.execute(command);
                    }
                    else if (instructionParts[0] == "wait")
                    {
                        WaitCommand command = parser.parseWaitCommand(instructionParts);
                        command.setIssuerId(myId);
                        Console.WriteLine(command.getType());

                        WaitRemoteAsyncDelegate RemoteDel = new WaitRemoteAsyncDelegate(server.execute);
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, null, null);
                        RemAr.AsyncWaitHandle.WaitOne();
                        Console.WriteLine(RemoteDel.EndInvoke(RemAr));

                        //server.execute(command);
                    }
                    else
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
            foreach (MeetingProposal meeting in meetings)
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

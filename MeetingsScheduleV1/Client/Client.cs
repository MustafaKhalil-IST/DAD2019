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
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);
            ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface),
                                                                           "tcp://localhost:8086/ServerObject");
            Console.WriteLine("Please enter your id: ");
            string myId = Console.ReadLine();
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

                    Command command = parser.parse(instructionParts);
                    command.setIssuerId(myId);

                    Console.WriteLine(command.getType());
                    server.execute(command);

                    /*
                    bool useCallback = true;

                    if (!useCallback)
                    {
                        // Alternative 1: asynchronous call without callback
                        // Create delegate to remote method
                        RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(server.execute);
                        // Call delegate to remote method
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, null, null);
                        // Wait for the end of the call and then explictly call EndInvoke
                        RemAr.AsyncWaitHandle.WaitOne();
                        Console.WriteLine(RemoteDel.EndInvoke(RemAr));
                    }
                    else
                    {
                        // Alternative 2: asynchronous call with callback
                        // Create delegate to remote method
                        RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(server.execute);
                        // Create delegate to local callback
                        AsyncCallback RemoteCallback = new AsyncCallback(Client.OurRemoteAsyncCallBack);
                        // Call remote method
                        IAsyncResult RemAr = RemoteDel.BeginInvoke(command, RemoteCallback, null);
                    }

                    // Console.WriteLine("executed with status: " + res);
                }*/

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

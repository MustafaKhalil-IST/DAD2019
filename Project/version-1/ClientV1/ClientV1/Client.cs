using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;

namespace MeetingScheduleV1
{
    class Client
    {
        static void Main(string[] args)
        {
            try
            {
                TcpChannel channel = new TcpChannel();
                ChannelServices.RegisterChannel(channel, false);
                ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface),
                                                                               "tcp://localhost:8086/ServerObject");
                string myId = "ID"; //TODO
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

                        server.execute(command);
                    }

                }
            }
            catch (Exception e)
            {

            }
            System.Console.ReadLine();
        }
    }

    class ClientObject : MarshalByRefObject, ClientInterface {
        public void addMeeting(MeetingProposal proposal) {
        }

        public MeetingProposal GetMeeting(string topic)
        {
            return null;
        }
        public void crash(){
        }
        public void status(){
        }
        public void unfreeze(){
        }
        public void freeze(){
        }
    }

}

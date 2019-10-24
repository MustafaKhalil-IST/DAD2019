using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;


namespace MeetingScheduleV1
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

    class ServerObject: MarshalByRefObject, ServerInterface {
        Dictionary<string, ClientInterface> clients;

        public void execute(Command command) {
            Executer executer = new Executer();
            executer.execute(command);
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

    class Executer {
        public void execute(JoinCommand command) {
        }

        public void execute(CloseCommand command) {
        }

        public void execute(WaitCommand command) {
        }

        public void execute(ListCommand command) {
        }

        public void execute(CreateCommand command) {
        }
    }
}

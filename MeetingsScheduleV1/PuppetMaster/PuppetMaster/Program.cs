using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MeetingsSchedule
{
    public class PuppetMasterObject : MarshalByRefObject
    {
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, false);

            PuppetMasterObject puppetMaster = new PuppetMasterObject();
            RemotingServices.Marshal(puppetMaster, "PuppetMasterObject", typeof(PuppetMasterObject));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PuppetMaster());
        }
    }
}

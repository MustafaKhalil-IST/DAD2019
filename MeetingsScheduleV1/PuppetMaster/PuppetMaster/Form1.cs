using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace MeetingsSchedule
{
    public partial class PuppetMaster : Form
    {
        private List<string> nodes;
        public PuppetMaster()
        {
            InitializeComponent();
            this.nodes = new List<string>();
        }

        private void execute(object sender, EventArgs e)
        {
            string instruction = this.instruction.Text;
            this.executeCommand(instruction);
        }

        private void executeScript(object sender, EventArgs e)
        {
            string location = this.script.Text;

            string[] lines = File.ReadAllLines(location);

            foreach(string instruction in lines)
            {
                this.executeCommand(instruction);
            }
        }

        private void executeCommand(string instruction) 
        {
            char[] delimiter = { ' ' };
            string[] instructionParts = instruction.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            if (instructionParts[0] == "Server")
            {
                Process.Start(@"..\..\..\..\Server.exe");
                this.nodes.Add("localhost:8086/ServerObject");
                this.results.Items.Add("Server added\n");
            }
            else if (instructionParts[0] == "Client")
            {
                string username = instructionParts[1];
                string port = instructionParts[2];
                string script = instructionParts[3];
                string args = username + " " + port + " " + script;
                Process.Start(@"..\..\..\..\Client.exe", args);
                this.nodes.Add("localhost:" + port + "/ClientObject");
                this.results.Items.Add("Client added\n");
            }
            else if (instructionParts[0] == "AddRoom")
            {

            }
            else if (instructionParts[0] == "Status")
            {

            }
            else if (instructionParts[0] == "AddRoom")
            {

            }
            else if (instructionParts[0] == "Crash")
            {

            }
            else if (instructionParts[0] == "Freeze")
            {

            }
            else if (instructionParts[0] == "Unfreeze")
            {

            }
            else if (instructionParts[0] == "Wait")
            {

            }
            else
            {
                this.errorMessage.Text = "error";
            }
        }

    }
}

﻿using System;
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
using System.Net.Sockets;


namespace MeetingsSchedule
{
 
    public partial class PuppetMaster : Form
    {
        private List<string> nodes;
        private Dictionary<string, string> idUrl = new Dictionary<string, string>();
        
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
                string id = instructionParts[1];
                string url = instructionParts[2];
                string max_faults = instructionParts[3];
                string min_delay = instructionParts[4];
                string max_delay = instructionParts[5];

                string args = id + " " + url + " " + max_faults + " " + min_delay + " " + max_delay;
                Process.Start(@"Server.exe", args);
                // To change 
                // Process.Start(@"C:\Users\cash\MEIC\Development of Distributed Systems\DAD2019\MeetingsScheduleV1\Server.exe");
                this.nodes.Add(url);
                this.idUrl[id] = url;
                this.results.Items.Add("Server added\n");
            }
            else if (instructionParts[0] == "Client")
            {
                string username = instructionParts[1];
                string url = instructionParts[2];
                string server_url = instructionParts[3];
                string script = instructionParts[4];
                string args = username + " " + url + " " + server_url + " " + script;
                Process.Start(@"Client.exe", args);
                // To change 
                // Process.Start(@"C:\Users\cash\MEIC\Development of Distributed Systems\DAD2019\MeetingsScheduleV1\Client.exe", args);
                this.nodes.Add(url);
                this.idUrl[username] = url;
                this.results.Items.Add("Client added\n");
            }
            else if (instructionParts[0] == "AddRoom")
            {
                string location = instructionParts[1];
                int capacity = Int32.Parse(instructionParts[2]);
                string roomId = instructionParts[3];
                Room room = new Room(roomId, location, capacity);
                ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface),
                                                                           "tcp://localhost:8086/ServerObject");
                server.addRoom(room);
                this.results.Items.Add("Room added\n");
            }
            else if (instructionParts[0] == "Status")
            {
                foreach(string node in this.nodes)
                {
                    if(node.EndsWith("ServerObject"))
                    {
                        this.results.Items.Add(node + " status: ");
                        try{
                            ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), node);
                            int res = server.status();
                            if(res == 1)
                            {
                                this.results.Items.Add("Alive");
                            }
                        }
                        catch(SocketException e){
                            this.results.Items.Add("Dead");
                        }
                        
                    }
                    if(node.EndsWith("ClientObject"))
                    {
                        this.results.Items.Add(node + " status: ");
                        try{
                            ClientInterface client = (ClientInterface)Activator.GetObject(typeof(ClientInterface), node);
                            int res = client.status();
                            if(res == 1)
                            {
                                this.results.Items.Add("Alive");
                            }
                        }
                        catch(SocketException e){
                            this.results.Items.Add("Dead");
                        }
                        
                    }
                }
            }
            else if (instructionParts[0] == "Crash")
            {
                string node = instructionParts[1];
                this.results.Items.Add("Crashing " + node);

                try
                {
                    ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), this.idUrl[node]);
                    server.crash();
                    this.results.Items.Add(node + " crashed");
                }
                catch (SocketException e)
                {
                    this.results.Items.Add(node + " Dead");
                }
            }
            else if (instructionParts[0] == "Freeze")
            {
                string node = instructionParts[1];
                ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), this.idUrl[node]);
                server.freeze();
            }
            else if (instructionParts[0] == "Unfreeze")
            {
                string node = instructionParts[1];
                ServerInterface server = (ServerInterface)Activator.GetObject(typeof(ServerInterface), this.idUrl[node]);
                server.unfreeze();
            }
            else if (instructionParts[0] == "Wait")
            {
                int milliseconds = Int32.Parse(instructionParts[1]);
                System.Threading.Thread.Sleep(milliseconds);
            }
            else
            {
                this.errorMessage.Text = "error";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingsSchedule
{
    [Serializable]
    public class MeetingProposal
    {
        private string coordinator;
        private string topic;
        private int min_attendees;
        private SortedSet<Slot> slots;
        private SortedSet<string> invitees;
        private bool closed;
        private bool cancelled;
        private Slot selectedSlot;
        private SortedSet<string> participants;

        public string Coordinator
        {
            get { return coordinator; }
            set { this.coordinator = value; }
        }

        public string Topic
        {
            get { return topic; }
            set { this.topic = value; }
        }

        public int MinAttendees
        {
            get { return min_attendees; }
            set { this.min_attendees = value; }
        }

        public bool Closed
        {
            get { return closed; }
            set { this.closed = value; }
        }

        public bool Cancelled
        {
            get { return cancelled; }
            set { this.cancelled = value; }
        }

        public Slot SelectedSlot
        {
            get { return selectedSlot; }
            set { this.selectedSlot = value; }
        }

        public SortedSet<string> Invitees
        {
            get { return invitees; }
            set { this.invitees = value; }
        }

        public SortedSet<string> Participants
        {
            get { return participants; }
            set { this.participants = value; }
        }

        public void addParticipant(string participant)
        {
            this.participants.Add(participant);
        }
    }

    [Serializable]
    public class Room
    {
        private string location;
        private int capacity;

        public string Location
        {
            get { return location; }
            set { this.location = value; }
        }

        public int Capacity
        {
            get { return capacity; }
            set { this.capacity = value; }
        }
    }

    [Serializable]
    public class Slot
    {
        private string location;
        private DateTime date;

        public string Location
        {
            get { return location; }
            set { this.location = value; }
        }

        public DateTime Date
        {
            get { return date; }
            set { this.date = value; }
        }
    }

    public interface ClientInterface
    {
        void addMeeting(MeetingProposal proposal);
        MeetingProposal GetMeeting(string topic);
        void crash();
        void status();
        void unfreeze();
        void freeze();
    }

    public interface ServerInterface
    {
        void execute(Command command);
        void crash();
        void status();
        void unfreeze();
        void freeze();
    }

    public class InstructsParser
    {
        public Command parse(string[] instruction)
        {
            if (instruction[0] == "create")
            {
                int min_attendees = Int32.Parse(instruction[1]);
                int n_slots = Int32.Parse(instruction[2]);

                List<Slot> slots = new List<Slot>();
                for (int i = 0; i < n_slots; i++) // i += 2
                {
                    Slot slot = new Slot();
                    slot.Location = instruction[i + 3];
                    slot.Date = DateTime.Parse(instruction[i + 4]);
                    slots.Add(slot);
                }
                return new Command("CREATE");
            }
            else if (instruction[0] == "list")
            {
                return new Command("LIST");
            }

            else if (instruction[0] == "join")
            {
                return new Command("JOIN");
            }

            else if (instruction[0] == "wait")
            {
                return new Command("WAIT");
            }

            return new Command("NOT FOUND");
        }
    }

    [Serializable]
    public class Command
    {
        private string issuerId;
        private string type;

        public Command(string type)
        {
            this.setType(type);
        }

        public string getIssuerId()
        {
            return issuerId;
        }

        public void setIssuerId(string issuerId)
        {
            this.issuerId = issuerId;
        }

        public string getType()
        {
            return type;
        }

        public void setType(string type)
        {
            this.type = type;
        }
    }
}

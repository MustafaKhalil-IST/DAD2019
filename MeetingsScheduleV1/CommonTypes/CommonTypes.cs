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
        private List<Slot> slots;
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

        public Slot(string location, DateTime date)
        {
            this.location = location;
            this.date = date;
        }

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
        int execute(CreateCommand command);
        int execute(ListCommand command);
        int execute(JoinCommand command);
        int execute(CloseCommand command);
        int execute(WaitCommand command);
        int execute(NotFoundCommand command);
        void crash();
        void status();
        void unfreeze();
        void freeze();
    }

    public class InstructsParser
    {
        public CreateCommand parseCreateCommand(string[] instruction)
        {
            string topic = instruction[1];
            int min_attendees = Int32.Parse(instruction[2]);
            int nr_slots = Int32.Parse(instruction[3]);
            int nr_invitees = Int32.Parse(instruction[4]);

            List<Slot> slots = new List<Slot>();

            for(int i = 0; i < nr_slots; i++)
            {
                string slot_info = instruction[i + 5];
                char[] delimiter = { ',' };
                string[] slot_infos = slot_info.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                string location = slot_infos[0];
                DateTime date = DateTime.Parse(slot_infos[1]);
                slots.Add(new Slot(location, date));
            }

            return new CreateCommand(topic, min_attendees, nr_slots, nr_invitees, slots);
        }

        public ListCommand parseListCommand(string[] instruction)
        {
            return new ListCommand();
        }

        public JoinCommand parseJoinCommand(string[] instruction)
        {
            string topic = instruction[1];
            return new JoinCommand(topic);
        }

        public WaitCommand parseWaitCommand(string[] instruction)
        {
            int milliseconds = Int32.Parse(instruction[1]);
            return new WaitCommand(milliseconds);
        }

        public CloseCommand parseCloseCommand(string[] instruction)
        {
            string topic = instruction[1];
            return new CloseCommand(topic);
        }
    }

    [Serializable]
    public abstract class Command
    {
        private string issuerId;
        private string type;

        public Command()
        {
        }

        public string getIssuerId()
        {
            return issuerId;
        }

        public void setIssuerId(string issuerId)
        {
            this.issuerId = issuerId;
        }

        public abstract string getType();
    }

    [Serializable]
    public class CreateCommand: Command
    {
        string topic;
        int min_attendees;
        int nr_slots;
        int nr_invitees;
        List<Slot> slots;

        public CreateCommand(string topic, int min_attendees, int nr_slots, int nr_invitees, List<Slot> slots)
        {
            this.topic = topic;
            this.min_attendees = min_attendees;
            this.nr_slots = nr_slots;
            this.nr_invitees = nr_invitees;
            this.slots = slots;
        }

        public string getTopic()
        {
            return this.topic;
        }

        public int getMinAttendees()
        {
            return this.min_attendees;
        }

        public int getNrSlots()
        {
            return this.nr_slots;
        }

        public int getNrInvitees()
        {
            return this.nr_invitees;
        }

        public List<Slot> getSlots()
        {
            return this.slots;
        }

        override
        public string getType()
        {
            return "CREATE " + this.topic + " " + this.nr_slots + " " + this.min_attendees + " " + this.nr_invitees;
        }
    }

    [Serializable]
    public class ListCommand : Command
    {
        override
        public string getType()
        {
            return "LIST";
        }
    }

    [Serializable]
    public class JoinCommand : Command
    {
        string topic;
        public JoinCommand(string topic)
        {
            this.topic = topic;
        }

        public string getTopic()
        {
            return this.topic;
        }

        override
        public string getType()
        {
            return "JOIN " + this.topic;
        }
    }

    [Serializable]
    public class CloseCommand : Command
    {
        string topic;
        public CloseCommand(string topic)
        {
            this.topic = topic;
        }

        public string getTopic()
        {
            return this.topic;
        }

        override
        public string getType()
        {
            return "CLOSE " + this.topic;
        }
    }

    [Serializable]
    public class WaitCommand : Command
    {
        int milliseconds;

        public WaitCommand(int milliseconds)
        {
            this.milliseconds = milliseconds;
        }

        public int getMilliseconds()
        {
            return this.milliseconds;
        }

        override
        public string getType()
        {
            return "WAIT " + this.milliseconds;
        }
    }

    [Serializable]
    public class NotFoundCommand : Command
    {
        override
        public string getType()
        {
            return "NOTFOUND";
        }
    }
}

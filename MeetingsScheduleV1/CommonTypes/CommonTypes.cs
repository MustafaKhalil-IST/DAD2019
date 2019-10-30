using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingsSchedule
{
    [Serializable]
    class RoomsManager
    {
        private Dictionary<string, List<Room>> rooms;

        public RoomsManager()
        {
            this.rooms = new Dictionary<string, List<Room>>();
            this.rooms.Add("Lisbon", new List<Room>() { new Room("R1", "Lisbon", 3), new Room("R2", "Lisbon", 3) });
            this.rooms.Add("Porto", new List<Room>() { new Room("R1", "Lisbon", 3), new Room("R2", "Lisbon", 3) });
        }

        public bool hasFreeRoomIn(string location, DateTime date)
        {
            foreach (Room room in this.rooms[location])
            {
                if (room.isFree(date))
                {
                    return true;
                }
            }
            return false;
        }
    }

    [Serializable]
    public class MeetingProposal
    {
        private string coordinator;
        private string topic;
        private int min_attendees;
        private List<Slot> slots;
        private List<string> invitees;
        private bool closed;
        private bool cancelled;
        private Slot selectedSlot;
        private Dictionary<string, List<Slot>> participants;
        private RoomsManager roomsManager;
        public MeetingProposal(string coordinator, string topic, int min_attendees, List<Slot> slots, List<string> invitees)
        {
            this.coordinator = coordinator;
            this.topic = topic;
            this.min_attendees = min_attendees;
            this.slots = slots;
            this.invitees = invitees;
            this.closed = false;
            this.cancelled = false;
            this.selectedSlot = null;
            this.participants = new Dictionary<string, List<Slot>>();
            this.roomsManager = new RoomsManager();
        }

        public string getCoordinator()
        {
            return this.coordinator;
        }

        public string getTopic()
        {
            return this.topic;
        }

        public List<Slot> getSlots()
        {
            return this.slots;
        }

        public Dictionary<string, List<Slot>> getParticipants()
        {
            return this.participants;
        }


        public void addParticipant(string participant, List<Slot> desiredSlots)
        {
            this.participants.Add(participant, desiredSlots);
        }

        public void cancel()
        {
            this.cancelled = true;
        }

        public void close()
        {
            Dictionary<Slot, int> interestingSlots = new Dictionary<Slot, int>();
            foreach(Slot slot in this.slots)
            {
                interestingSlots.Add(slot, 0);
            }
            
            foreach(Slot slot in this.slots)
            {
                if (this.roomsManager.hasFreeRoomIn(slot.getLocation(), slot.getDate()))
                {
                    // select room
                    foreach(string participant in this.participants.Keys)
                    {
                        if (this.participants[participant].Contains(slot))
                        {
                            interestingSlots[slot]++;
                        }
                    }
                }
            }

            int maxInterests = -1;
            foreach (Slot slot in interestingSlots.Keys)
            {
                if (interestingSlots[slot] > maxInterests)
                {
                    maxInterests = interestingSlots[slot];
                    this.selectedSlot = slot;
                }
            }

            if(maxInterests < this.min_attendees)
            {
                this.selectedSlot = null;
                this.cancel();
            }

            this.closed = true;
            // TODO exclude some participants if there is no capacity
        }

        public void selectSlot(Slot slot)
        {
            this.selectedSlot = slot;
        }
    }

    [Serializable]
    public class Room
    {
        private string id;
        private string location;
        private int capacity;
        private HashSet<DateTime> reserves;

        public Room(string id, string location, int capacity)
        {
            this.id = id;
            this.location = location;
            this.capacity = capacity;
            this.reserves = new HashSet<DateTime>();
        }

        public string getID()
        {
            return this.id;
        }

        public int getCapacity()
        {
            return this.capacity;
        }

        public string getLocation()
        {
            return this.location;
        }

        public void addReservation(DateTime date)
        {
            this.reserves.Add(date);
        }

        public bool isFree(DateTime date)
        {
            return this.reserves.Contains(date);
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

        public string getLocation()
        {
            return this.location;
        }

        public DateTime getDate()
        {
            return this.date;
        }

        public override bool Equals(Object obj)
        {
            Slot slot = (Slot)obj;
            return this.date == slot.getDate() && this.location == slot.getLocation();
        }
    }

    public interface ClientInterface
    {
        void addMeeting(MeetingProposal proposal);
        MeetingProposal GetMeeting(string topic);
        void listMeetings(List<MeetingProposal> meetings);
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

            List<string> invitees = new List<string>();
            if(nr_invitees == 0)
            {
                invitees = null;
            }

            else
            {
                invitees = new List<string>();
            }

            return new CreateCommand(topic, min_attendees, nr_slots, nr_invitees, slots, invitees);
        }

        public ListCommand parseListCommand(string[] instruction)
        {
            return new ListCommand();
        }

        public JoinCommand parseJoinCommand(string[] instruction)
        {
            string topic = instruction[1];
            int nr_desired_slots = Int32.Parse(instruction[2]);
            List<Slot> desiredSlots = new List<Slot>();
            for(int i = 0; i < nr_desired_slots; i++)
            {
                string desiredSlotInfo = instruction[i + 3];
                char[] delimiter = { ',' };
                string[] slot_infos = desiredSlotInfo.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                string location = slot_infos[0];
                DateTime date = DateTime.Parse(slot_infos[1]);
                Slot desiredSlot = new Slot(location, date);

                desiredSlots.Add(desiredSlot);
            }
            return new JoinCommand(topic, desiredSlots);
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
        List<string> invitees;
        MeetingProposal meeting;

        public CreateCommand(string topic, int min_attendees, int nr_slots, int nr_invitees, List<Slot> slots, List<string> invitees)
        {
            this.topic = topic;
            this.min_attendees = min_attendees;
            this.nr_slots = nr_slots;
            this.nr_invitees = nr_invitees;
            this.slots = slots;
            this.invitees = invitees;
            this.meeting = new MeetingProposal(this.getIssuerId(), this.topic, this.min_attendees, this.slots, this.invitees);
        }

        public MeetingProposal getMeetingProposal()
        {
            return this.meeting;
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
        List<Slot> desiredSlots;
        public JoinCommand(string topic, List<Slot> desiredSlots)
        {
            this.topic = topic;
            this.desiredSlots = desiredSlots;
        }

        public string getTopic()
        {
            return this.topic;
        }

        public List<Slot> getDesiredSlots()
        {
            return this.desiredSlots;
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

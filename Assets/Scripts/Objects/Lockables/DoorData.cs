namespace Creation
{
    public class DoorData : AbstractData
    {
        public int Id { get => id; }
        public int Level { get => level; }
        public int RoomNumber { get => roomNumber; }
        public int X { get => x; }
        public int Y { get => y; }
        public int WallIndex { get => wallIndex; }
        public bool IsEntrance { get => isEntrance; }
        public bool HasBell { get => hasBell; }
        public bool IsLocked { get => isLocked; }

        private int id;
        private int level;
        private int x;
        private int y;
        private int wallIndex;
        private bool isEntrance;
        private int roomNumber;
        private bool isLocked;
        private bool hasBell;
        private Lockable lockable;

        public DoorData(int level, int roomNumber, int x, int y, int wallIndex, bool isEntrance = false, bool hasBell = false)
        {
            this.level = level;
            this.roomNumber = roomNumber;
            this.x = x;
            this.y = y;
            this.wallIndex = wallIndex;
            this.isEntrance = isEntrance;
            this.hasBell = isEntrance && hasBell;
            isLocked = false;
        }

        public DoorData(int level, int roomNumber, int x, int y, int wallIndex, int id, bool isEntrance = false, bool hasBell = false)
            : this(level, roomNumber, x, y, wallIndex, isEntrance, hasBell)
        {
            this.id = id;
        }

        public void SetLockable(Lockable lockable)
        {
            this.lockable = lockable;
        }

        public void SetLocked(bool isLocked)
        {
            this.isLocked = isLocked;
        }

        public void Load()
        {
            
        }

        public void Save()
        {
            
        }
    }
}

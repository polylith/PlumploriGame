public class DigitalClockAlarmData : AbstractData
{
    private static string TimeToString(int time)
    {
        int h = time / 60;
        int m = time % 60;
        return (h < 10 ? "0" : "") + h
            + ":"
            + (m < 10 ? "0" : "") + m;
    }

    public int Time { get => time; }
    public bool IsActive { get; set; } = true;
    public bool IsOn { get; set; }
    public string TimeString { get => TimeToString(time); }

    private readonly int time;

    public DigitalClockAlarmData(int time)
    {
        this.time = time;
    }

    public override bool Equals(object obj)
    {
        if (null == obj || !(obj is DigitalClockAlarmData alarmdata))
            return false;

        return alarmdata.Time == Time;
    }

    public override int GetHashCode()
    {
        return Time;
    }

    public void Load()
    {
        
    }

    public void Save()
    {
        
    }
}

public interface IIPv4Device 
{
    public abstract IPv4Config IPv4Config { get; }

    public abstract void Send();

    public abstract void Receive();
}

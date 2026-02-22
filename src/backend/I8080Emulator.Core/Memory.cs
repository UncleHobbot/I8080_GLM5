namespace I8080Emulator.Core;

public class Memory
{
    private readonly byte[] _data;
    public const int Size = 65536;

    public Memory()
    {
        _data = new byte[Size];
    }

    public byte Read(ushort address) => _data[address];
    public void Write(ushort address, byte value) => _data[address] = value;
    
    public void Load(ushort address, byte[] data) => Array.Copy(data, 0, _data, address, data.Length);
    public byte[] Dump(ushort address, int length)
    {
        var result = new byte[length];
        Array.Copy(_data, address, result, 0, length);
        return result;
    }
    
    public void Clear() => Array.Clear(_data, 0, Size);
}

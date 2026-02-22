using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace I8080Emulator.CPM;

public class DiskDrive
{
    private readonly string _basePath;
    private readonly Dictionary<string, byte[]> _files = new();
    private readonly int _driveNumber;
    
    public const int SectorSize = 128;
    
    public DiskDrive(int driveNumber, string basePath)
    {
        _driveNumber = driveNumber;
        _basePath = basePath;
        Directory.CreateDirectory(basePath);
        LoadFiles();
    }
    
    private void LoadFiles()
    {
        foreach (var file in Directory.GetFiles(_basePath, "*.*"))
        {
            var name = Path.GetFileName(file).ToUpperInvariant();
            _files[name] = File.ReadAllBytes(file);
        }
    }
    
    public void SaveFile(string name, byte[] data)
    {
        var filePath = Path.Combine(_basePath, name.ToUpperInvariant());
        File.WriteAllBytes(filePath, data);
        _files[name.ToUpperInvariant()] = data;
    }
    
    public byte[]? GetFile(string name)
    {
        return _files.TryGetValue(name.ToUpperInvariant(), out var data) ? data : null;
    }
    
    public void DeleteFile(string name)
    {
        var filePath = Path.Combine(_basePath, name.ToUpperInvariant());
        if (File.Exists(filePath)) File.Delete(filePath);
        _files.Remove(name.ToUpperInvariant());
    }
    
    public IEnumerable<string> ListFiles() => _files.Keys;
}

using HKXPoserNG.Extensions;
using System.IO;

public class Annotation {
    public float time;
    public string text;

    public void Read(BinaryReader reader) {
        this.time = reader.ReadSingle();
        this.text = reader.ReadCString();
    }

    public void Write(BinaryWriter writer) {
        writer.Write(this.time);
        writer.WriteCString(this.text);
    }
}

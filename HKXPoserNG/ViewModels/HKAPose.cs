using System.IO;

namespace HKXPoserNG.ViewModels;

public class HKAPose
{
    public float time;
    public Transform[] transforms;
    public float[] floats;

    public void Read(BinaryReader reader, int numTransforms, int numFloats)
    {
        this.time = reader.ReadSingle();

        this.transforms = new Transform[numTransforms];
        for (int i = 0; i < numTransforms; i++)
        {
            Transform t = new Transform();
            t.Read(reader);
            this.transforms[i] = t;
        }

        this.floats = new float[numFloats];
        for (int i = 0; i < numFloats; i++)
        {
            this.floats[i] = reader.ReadSingle();
        }
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(this.time);

        for (int i = 0, len = this.transforms.Length; i < len; i++)
        {
            this.transforms[i].Write(writer);
        }

        for (int i = 0, len = this.floats.Length; i < len; i++)
        {
            writer.Write(this.floats[i]);
        }
    }
}

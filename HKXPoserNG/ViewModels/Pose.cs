using System.Collections.Generic;
using System.IO;

namespace HKXPoserNG.ViewModels;

public class Pose {
    public Pose() {
        transforms = [];
    }
    public Pose(float time, Transform[] transforms, float[] floats) {
        this.Time = time;
        this.transforms = transforms;
        this.floats = floats;
    }
    public Pose(BinaryReader reader, int numTransforms, int numFloats) {
        this.Time = reader.ReadSingle();

        this.transforms = new Transform[numTransforms];
        for (int i = 0; i < numTransforms; i++) {
            Transform t = new Transform(reader);
            this.transforms[i] = t;
        }

        this.floats = new float[numFloats];
        for (int i = 0; i < numFloats; i++) {
            this.floats[i] = reader.ReadSingle();
        }
    }

    public float Time { get; }

    private Transform[] transforms;
    public IReadOnlyList<Transform> Transforms => transforms;

    private float[] floats;
    public IReadOnlyList<float> Floats => floats;

    public void Write(BinaryWriter writer) {
        writer.Write(this.Time);

        for (int i = 0, len = this.transforms.Length; i < len; i++) {
            this.transforms[i].Write(writer);
        }

        for (int i = 0, len = this.floats.Length; i < len; i++) {
            writer.Write(this.floats[i]);
        }
    }
}

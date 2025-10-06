using HKX2;
using HKXPoserNG.Extensions;
using PropertyChanged.SourceGenerator;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class Animation {
    [Notify, AlsoNotify(nameof(CurrentPose))]
    private int currentFrame = 0;

    [Notify, AlsoNotify(nameof(FrameCountMinusOne))]
    private int frameCount = 0;
    public int FrameCountMinusOne => FrameCount - 1;

    [Notify]
    private float duration;

    internal Pose[]? poses;
    public IReadOnlyList<Pose> Poses => poses ?? [new()];

    public Pose CurrentPose => Poses[currentFrame];

    private Annotation[] annotations = Array.Empty<Annotation>();
    public IReadOnlyList<Annotation> Annotations => annotations;

    public int NumTransforms => poses?[0].Transforms.Count ?? 0;
    public int NumFloats => poses?[0].Floats.Count ?? 0;

    private Dictionary<Bone, int> boneMap = new();
    public IReadOnlyDictionary<Bone, int> BoneMap => boneMap;

    public void LoadFromHKX(FileInfo fileInfo) {
        string path_out_hct = Path.Combine(PathConstants.TempDirectory, fileInfo.Name);
        ExternalPrograms.HCT(fileInfo.FullName, path_out_hct);
        string name_no_ext = fileInfo.Name[..^4];
        string path_out_hkdump = Path.Combine(PathConstants.TempDirectory, $"{name_no_ext}.bin");
        ExternalPrograms.HKDump(path_out_hct, path_out_hkdump);
        LoadFromHKDump(path_out_hkdump);
        //LoadFromHKX2(fileInfo.FullName);
        CurrentFrame = 0;
    }

    private void LoadFromHKDump(string path) {
        using (Stream stream = File.OpenRead(path))
        using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default)) {
            string head = reader.ReadHeaderString();
            //TODO: throw exception
            uint version = reader.ReadUInt32();
            if (version != 0x01000200) {
                throw new NotSupportedException("Error: version mismatch! Abort.");
            }
            int nskeletons = reader.ReadInt32();
            if (nskeletons != 0) {
                throw new NotSupportedException($"Error: #skeletons should be 0 but {nskeletons}! Abort.");
            }
            int nanimations = reader.ReadInt32();
            if (nanimations != 1) {
                throw new NotSupportedException($"Error: #animations should be 1 but {nanimations}! Abort.");
            }
            /// Returns the number of original samples / frames of animation.
            FrameCount = reader.ReadInt32();
            /// The length of the animation cycle in seconds
            Duration = reader.ReadSingle();
            /// The number of bone tracks to be animated.
            int numTransforms = reader.ReadInt32();
            /// The number of float tracks to be animated
            int numFloats = reader.ReadInt32();
            /// Get a subset of the first 'maxNumTracks' transform tracks (all tracks from 0 to maxNumTracks-1 inclusive), and the first 'maxNumFloatTracks' float tracks of a pose at a given time.
            poses = new Pose[FrameCount];
            for (int i = 0; i < FrameCount; i++) {
                poses[i] = new Pose(reader, numTransforms, numFloats);
            }
            /// The annotation tracks associated with this skeletal animation.
            int numAnnotationTracks = reader.ReadInt32();
            int numAnnotations = reader.ReadInt32();
            annotations = new Annotation[numAnnotations];
            for (int i = 0; i < numAnnotations; i++) {
                annotations[i] = new Annotation();
                annotations[i].Read(reader);
            }
        }
        OnPropertyChanged(new(nameof(Poses)));
        OnPropertyChanged(new(nameof(Annotations)));
        OnPropertyChanged(new(nameof(NumTransforms)));
        OnPropertyChanged(new(nameof(NumFloats)));
    }

    //private void LoadFromHKX2(string path) {
    //    using (Stream stream = File.OpenRead(path)) {
    //        PackFileDeserializer deserializer = new();
    //        BinaryReaderEx reader = new(stream);
    //        hkRootLevelContainer hkRoot = (hkRootLevelContainer)deserializer.Deserialize(reader);
    //        hkaAnimationContainer hkaAnimationContainer = (hkaAnimationContainer)hkRoot.m_namedVariants[0]!.m_variant!;
    //        IEnumerable<string> names = hkaAnimationContainer.m_animations[0]!.m_annotationTracks.Select(t => t.m_trackName);
    //        int i = 0;
    //        foreach (string name in names) {
    //            Bone? bone;
    //            if (Skeleton.Instance.BoneDictionary.TryGetValue(name, out bone)) {
    //                boneMap[bone] = i;
    //                //Debug.WriteLine($"Bone '{name}' found in skeleton.");
    //            } else {
    //                //Debug.WriteLine($"Bone '{name}' not found in skeleton.");
    //                string side = name[^1..];
    //                if (side != "L" && side != "R") continue;
    //                int index = name.IndexOf('[');
    //                string name_new = name[..^2];
    //                name_new = name_new.Insert(index + 1, side);
    //                name_new = name_new.Insert(3, ' ' + side);
    //                if (Skeleton.Instance.BoneDictionary.TryGetValue(name_new, out bone)) {
    //                    boneMap[bone] = i;
    //                    //Debug.WriteLine($"Bone '{name_new}' found in skeleton.");
    //                } else {
    //                    Debug.WriteLine($"Bone '{name}' and '{name_new}' not found in skeleton.");
    //                }
    //            }
    //            i++;
    //        }
    //    }
    //}

    public void SaveToHKDump(string filename) {
        using (Stream stream = File.Create(filename))
            SaveToHKDump(stream);
    }

    public void SaveToHKDump(Stream stream) {
        using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default)) {
            string head = "hkdump File Format, Version 1.0.2.0";
            uint version = 0x01000200;
            int nskeletons = 0;
            int nanimations = 1;

            writer.WriteHeaderString(head);
            writer.Write(version);
            writer.Write(nskeletons);
            writer.Write(nanimations);

            Write(writer);
        }
    }

    public void Write(BinaryWriter writer) {
        writer.Write(this.FrameCount);
        writer.Write(this.Duration);

        writer.Write(this.NumTransforms);
        writer.Write(this.NumFloats);

        for (int i = 0; i < FrameCount; i++) {
            poses[i].Write(writer);
        }

        int numAnnotationTracks = this.NumTransforms; // why
        writer.Write(numAnnotationTracks);
        int numAnnotations = annotations.Length;
        writer.Write(numAnnotations);

        for (int i = 0; i < numAnnotations; i++) {
            annotations[i].Write(writer);
        }
    }
}

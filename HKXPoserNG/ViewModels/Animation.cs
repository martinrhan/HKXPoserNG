using HKXPoserNG.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace HKXPoserNG.ViewModels;

public class Animation {
    public int NumOriginalFrames { get; private set; }
    public float Duration { get; private set; }

    private Pose[]? poses;
    public IReadOnlyList<Pose> Poses => poses ?? Array.Empty<Pose>();

    private Annotation[]? annotations;
    public Annotation[]? Annotations => annotations ?? Array.Empty<Annotation>();

    public int NumTransforms => poses?[0].transforms.Length ?? 0;
    public int NumFloats => poses?[0].floats.Length ?? 0;

    public bool Load(string path) {
        using (Stream stream = File.OpenRead(path))
            return Load(stream);
    }
    public bool Load(Stream stream) {
        using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default)) {
            string head = reader.ReadHeaderString();
            //TODO: throw exception
            uint version = reader.ReadUInt32();
            if (version != 0x01000200) {
                Console.WriteLine("Error: version mismatch! Abort.");
                return false;
            }
            int nskeletons = reader.ReadInt32();
            if (nskeletons != 0) {
                Console.WriteLine("Error: #skeletons should be 0 but {0}! Abort.", nskeletons);
                return false;
            }
            int nanimations = reader.ReadInt32();
            if (nanimations != 1) {
                Console.WriteLine("Error: #animations should be 1 but {0}! Abort.", nanimations);
                return false;
            }
            Read(reader);
        }
        return true;
    }

    public void Read(BinaryReader reader) {
        /// Returns the number of original samples / frames of animation.
        this.NumOriginalFrames = reader.ReadInt32();
        /// The length of the animation cycle in seconds
    	this.Duration = reader.ReadSingle();
        /// The number of bone tracks to be animated.
        int numTransforms = reader.ReadInt32();
        /// The number of float tracks to be animated
        int numFloats = reader.ReadInt32();

        /// Get a subset of the first 'maxNumTracks' transform tracks (all tracks from 0 to maxNumTracks-1 inclusive), and the first 'maxNumFloatTracks' float tracks of a pose at a given time.

        this.poses = new Pose[NumOriginalFrames];
        for (int i = 0; i < NumOriginalFrames; i++) {
            this.poses[i] = new Pose();
            this.poses[i].Read(reader, numTransforms, numFloats);
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

    public void Save(string filename) {
        using (Stream stream = File.Create(filename))
            Save(stream);
    }

    public void Save(Stream stream) {
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
        writer.Write(this.NumOriginalFrames);
        writer.Write(this.Duration);

        writer.Write(this.NumTransforms);
        writer.Write(this.NumFloats);

        for (int i = 0; i < NumOriginalFrames; i++) {
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

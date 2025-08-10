using HKXPoserNG.Extensions;
using System.IO;

namespace HKXPoserNG.ViewModels;

public class HKASkeleton
{
    public string name;
    public short[] parentIndices;
    public HKABone[] bones;
    public Transform[] referencePose;
    public float[] referenceFloats;
    public string[] floatSlots;

    /// load skeleton.bin
    public void Load(string filename)
    {
        using (Stream stream = File.OpenRead(filename))
            Load(stream);
    }

    public void Load(Stream stream)
    {
        using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default))
        {
            string head = reader.ReadHeaderString();
            uint version = reader.ReadUInt32();
            // should be 0x01000200
            int nskeletons = reader.ReadInt32();
            // should be 1 or 2
            Read(reader);

            int nanimations = reader.ReadInt32();
            // should be 0
        }
    }

    public void Read(BinaryReader reader)
    {
        /// A user name to aid in identifying the skeleton 
        this.name = reader.ReadCString();

        /// Parent relationship
        int nparentIndices = reader.ReadInt32();
        this.parentIndices = new short[nparentIndices];
        for (int i=0; i<nparentIndices; i++)
        {
            this.parentIndices[i] = reader.ReadInt16();
        }

        /// Bones for this skeleton
        int nbones = reader.ReadInt32();
        this.bones = new HKABone[nbones];
        for (int i=0; i<nbones; i++)
        {
            HKABone bone = new HKABone();
            bone.Read(reader);
            this.bones[i] = bone;
        }

        for (short i=0; i<nbones; i++)
        {
            HKABone bone = this.bones[i];
            bone.idx = i;
            short parent_idx = this.parentIndices[i];
            if (parent_idx != -1)
            {
                HKABone parent = this.bones[parent_idx];
                bone.parent = parent;
                parent.children.Add(bone);
            }
        }

        // hide NPC Root
        this.bones[0].hide = true;

#if false
        // hide since Camera3rd
        if (nbones != 1)
        {
            bool hide = false;
            for (int i=1; i<nbones; i++)
            {
                hkaBone bone = this.bones[i];
                if (bone.parent == null)
                    hide = true;
                bone.hide = hide;
            }
        }
#endif

        /// The reference pose for the bones of this skeleton. This pose is stored in local space.
        int nreferencePose = reader.ReadInt32();
        this.referencePose = new Transform[nreferencePose];
        for (int i=0; i<nreferencePose; i++)
        {
            Transform t = new Transform();
            t.Read(reader);
            this.referencePose[i] = t;
        }

        for (int i=0; i<nbones; i++)
        {
            HKABone bone = this.bones[i];
            bone.local = this.referencePose[i];
            bone.patch = new Transform();
        }

        /// The reference values for the float slots of this skeleton. This pose is stored in local space.
        int nreferenceFloats = reader.ReadInt32();
        this.referenceFloats = new float[nreferenceFloats];
        for (int i=0; i<nreferenceFloats; i++)
        {
            this.referenceFloats[i] = reader.ReadSingle();
        }

        /// Floating point track slots. Often used for auxiliary float data or morph target parameters etc.
        /// This defines the target when binding animations to a particular rig.
        int nfloatSlots = reader.ReadInt32();
        this.floatSlots = new string[nfloatSlots];
        for (int i=0; i<nfloatSlots; i++)
        {
            this.floatSlots[i] = reader.ReadCString();
        }
    }
}

using HKXPoserNG.Extensions;
using System.Collections.Generic;
using System.IO;

namespace HKXPoserNG.ViewModels;

public class HKABone
{
    public string name;
    internal short idx;
    internal bool hide = false;
    internal HKABone parent = null;
    internal List<HKABone> children = new List<HKABone>();
    internal Transform local;
    internal Transform patch;

    public void Read(BinaryReader reader)
    {
        this.name = reader.ReadCString();
    }

    internal Transform GetWorldCoordinate()
    {
        Transform t = new Transform();
        HKABone bone = this;
        //int i = 0;
        while (bone != null)
        {
            //Console.WriteLine(" local loop idx {0} Ref {1}", i, node.self_ref);
            t = bone.local * bone.patch * t;
            bone = bone.parent;
            //i++;
        }
        return t;
    }
}

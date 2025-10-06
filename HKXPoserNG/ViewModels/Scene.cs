using HKXPoserNG.Mvvm;
using SingletonSourceGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKXPoserNG.ViewModels;

[Singleton]
public partial class Scene {
    public SimpleCommand MoveFocusPointToSelectedBoneCommand { get; }
}

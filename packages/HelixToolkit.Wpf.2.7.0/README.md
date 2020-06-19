<img src='https://avatars3.githubusercontent.com/u/8432523?s=200&v=4' width='64' />

# Helix Toolkit

**Helix Toolkit is a collection of 3D components for .NET Framework.**

[**HelixToolkit.WPF:**](/Source/HelixToolkit.Wpf) 
Adds variety of functionalities/models on the top of internal WPF 3D model (Media3D namespace). 

[**HelixToolkit.SharpDX.WPF:**](/Source/HelixToolkit.Wpf.SharpDX) 
3D Components and XAML/MVVM compatible Scene Graphs based on [SharpDX](https://github.com/sharpdx/SharpDX)(DirectX 11) for high performance usage.

[**HelixToolkit.UWP:**](/Source/HelixToolkit.UWP) 
3D Components and XAML/MVVM compatible Scene Graphs based on [SharpDX](https://github.com/sharpdx/SharpDX)(DirectX 11) for Universal Windows App.

[**HelixToolkit.SharpDX.Core:**](/Source/HelixToolkit.SharpDX.Core) 
3D Components and Scene Graphs based on [SharpDX](https://github.com/sharpdx/SharpDX)(DirectX 11) for netstandard and .NET Core.

[**HelixToolkit.SharpDX.Assimp:**](/Source/HelixToolkit.Wpf.SharpDX.Assimp) 
[Assimp.Net](https://bitbucket.org/Starnick/assimpnet/src/master/) 3D model importer/expoter support for HelixToolkit.SharpDX Components.

[**Examples:**](/develop/Source/Examples)
Please download full source code to run examples. Or download [compiled version](https://ci.appveyor.com/project/objorke/helix-toolkit/branch/develop/artifacts)

[![License: MIT](https://img.shields.io/github/license/helix-toolkit/helix-toolkit.svg?style=popout)](https://github.com/helix-toolkit/helix-toolkit/blob/develop/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/tmqafdk9p7o98gw7?svg=true)](https://ci.appveyor.com/project/objorke/helix-toolkit)
[![Release](https://img.shields.io/github/release/helix-toolkit/helix-toolkit.svg?style=popout)](https://www.nuget.org/packages?q=Helix-Toolkit)
[![Chat](https://img.shields.io/gitter/room/helix-toolkit/helix-toolkit.svg)](https://gitter.im/helix-toolkit/helix-toolkit)

Description         | Value
--------------------|-----------------------
Web page            | http://helix-toolkit.github.io/
Wiki                | https://github.com/helix-toolkit/helix-toolkit/wiki
Documentation       | http://helix-toolkit.readthedocs.io/
Chat                | https://gitter.im/helix-toolkit/helix-toolkit
Source repository   | http://github.com/helix-toolkit/helix-toolkit
Latest build        | http://ci.appveyor.com/project/objorke/helix-toolkit
Issue tracker       | http://github.com/helix-toolkit/helix-toolkit/issues
NuGet packages      | http://www.nuget.org/packages?q=HelixToolkit
MyGet feed          | https://www.myget.org/F/helix-toolkit
StackOverflow       | http://stackoverflow.com/questions/tagged/helix-3d-toolkit
Twitter             | https://twitter.com/hashtag/Helix3DToolkit

## Project Build

**Visual Studio 2017. Windows 10 SDK (Min Ver.10.0.10586.0).**

**Missing `.cso` error during the build?** Windows 10 SDK **Ver.10.0.10586.0** can be selected and installed using Visual Studio 2017 installer. If you already installed the higher SDK version, please change the target version in **HelixToolkit.Native.ShaderBuilder** property to the version installed on your machine.

## Notes

#### 1. Right-handed Cartesian coordinate system and row major matrix by default
HelixToolkit default is using right-handed Cartesian coordinate system, including Meshbuilder etc. To use left-handed Cartesian coordinate system (Camera.CreateLeftHandedSystem = true), user must manually correct the triangle winding order or IsFrontCounterClockwise in raster state description if using SharpDX. Matrices are row major by default.

#### 2. Performance [Topics](https://github.com/helix-toolkit/helix-toolkit/wiki/Tips-on-performance-optimization-(WPF.SharpDX-and-UWP)) for WPF.SharpDX and UWP.

#### 3. Following features are not supported currently on FeatureLevel 10 graphics card:
FXAA, Order Independant Transparent Rendering, Particle system, Tessellation.

#### 4. [Wiki](https://github.com/helix-toolkit/helix-toolkit/wiki) and useful [External Resources](https://github.com/helix-toolkit/helix-toolkit/wiki/External-References) on Computer Graphics.

## News
#### 2019-02-16
[v2.6.1](https://github.com/helix-toolkit/helix-toolkit/tree/release/2.6.1) releases are available on nuget. [Release Note](/CHANGELOG.md)
- [WPF](https://www.nuget.org/packages/HelixToolkit.Wpf/2.6.1)
- [WPF.SharpDX](https://www.nuget.org/packages/HelixToolkit.Wpf.SharpDX/2.6.1)
- [UWP](https://www.nuget.org/packages/HelixToolkit.UWP/2.6.1)
- [SharpDX.Core](https://www.nuget.org/packages/HelixToolkit.SharpDX.Core/2.6.1)
- [SharpDX.Assimp](https://www.nuget.org/packages/HelixToolkit.SharpDX.Assimp/2.6.1)

#### 2019-01-04
[v2.6.0](https://github.com/helix-toolkit/helix-toolkit/tree/release/2.6.0) releases are available on nuget. [Release Note](/CHANGELOG.md)
- [WPF](https://www.nuget.org/packages/HelixToolkit.Wpf/2.6.0)
- [WPF.SharpDX](https://www.nuget.org/packages/HelixToolkit.Wpf.SharpDX/2.6.0)
- [UWP](https://www.nuget.org/packages/HelixToolkit.UWP/2.6.0)
- [SharpDX.Core](https://www.nuget.org/packages/HelixToolkit.SharpDX.Core/2.6.0)
- [SharpDX.Assimp](https://www.nuget.org/packages/HelixToolkit.SharpDX.Assimp/2.6.0)

**Two new packages have been released on version 2.6.0.**
- `SharpDX.Core` is the base implementation for all HelixToolkit.SharpDX versions(WPF.SharpDX and UWP). It is implemented based on `netstandard` and can be used on other platforms such as WinForms and DotNetCore.
- `SharpDX.Assimp` ports [Assimp.Net](https://bitbucket.org/Starnick/assimpnet/src/master/) into HelixToolkit to support 3D model import/export for HelixToolkit.SharpDX versions. Currently import/export supports [SceneNode](https://github.com/helix-toolkit/helix-toolkit/wiki/Use-Element3D-or-SceneNode-under-WPF.SharpDX-or-UWP) only. For more details, please refer to [FileLoadDemo](/Source/Examples/WPF.SharpDX/FileLoadDemo) or [CoreTest](/Source/Examples/SharpDX.Core/CoreTest).


#### 2018-10-19
[V2.5.1](https://github.com/helix-toolkit/helix-toolkit/tree/hotfix/2.5.0) releases are available on nuget. [Release Note](/CHANGELOG.md)

**Hotfix: Wrong type cast causes null reference exception when uses custom ViewBox texture.** (SharpDX Version Only)
- [WPF](https://www.nuget.org/packages/HelixToolkit.Wpf/2.5.1)
- [WPF.SharpDX](https://www.nuget.org/packages/HelixToolkit.Wpf.SharpDX/2.5.1)
- [UWP](https://www.nuget.org/packages/HelixToolkit.UWP/2.5.1)

#### Changes (Please refer to [Release Note](https://github.com/helix-toolkit/helix-toolkit/blob/master/CHANGELOG.md) for details)

##### Note: 2.0 Breaking changes from version 1.x.x. (HelixToolkit.SharpDX only) see [ChangeLog](/CHANGELOG.md)

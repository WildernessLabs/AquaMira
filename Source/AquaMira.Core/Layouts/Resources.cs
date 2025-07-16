using Meadow.Foundation.Graphics;
using System.Collections.Generic;
using System.Reflection;

namespace AquaMira.Core;

public static class Resources
{
    private static readonly Dictionary<string, Image> _images = new();
    private static readonly string _assemblyName;

    public static Image Heart => GetImageResource("heart.bmp");
    public static Image Logo => GetImageResource("logo.bmp");
    public static Image LogoSmall => GetImageResource("logo-small.bmp");
    public static Image NetConnected => GetImageResource("net-connected.bmp");
    public static Image NetDisconnected => GetImageResource("net-disconnected.bmp");

    static Resources()
    {
        _assemblyName = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Name;
    }

    private static Image GetImageResource(string name)
    {
        if (!_images.ContainsKey(name))
        {
            _images.Add(name, Image.LoadFromResource($"{_assemblyName}.Assets.{name}"));
        }

        return _images[name];
    }
}

using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public interface Camera
{
    Matrix GetView();
    Matrix GetProjection();

    void OnClientSizeChanged(object sender, EventArgs e);

    static abstract string GetName();
}
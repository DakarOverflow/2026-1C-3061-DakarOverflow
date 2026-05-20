using System;
using System.Runtime.InteropServices;

namespace TGC.MonoGame.TP
{
    public static class Program
    {
        
        [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetHint(string name, string value);
        [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GetError();
        [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetRelativeMouseMode(int enabled);
    
        [DllImport("/home/pc/RiderProjects/2026-1C-3061-DakarOverflow/TGC.MonoGame.TP/bin/Debug/net8.0/runtimes/linux-x64/native/libSDL2-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_GetRelativeMouseMode();
        
        [STAThread]
        static void Main()
        {
            using (var game = new TGCGame())
                game.Run();
        }
    }
}

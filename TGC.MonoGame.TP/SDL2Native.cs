using System;
using System.Runtime.InteropServices;

internal static class Sdl2Native
{
    private enum SDL_bool
    {
        SDL_FALSE = 0,
        SDL_TRUE = 1
    }

    private delegate int SetRelativeMouseModeDelegate(SDL_bool enabled);
    private delegate IntPtr GetErrorDelegate();
    private delegate int GetRelativeMouseModeDelegate();
    private delegate uint GetRelativeMouseStateDelegate(out int x, out int y);

    private static readonly SetRelativeMouseModeDelegate _setRelativeMouseMode;
    private static readonly GetErrorDelegate _getError;
    private static readonly GetRelativeMouseModeDelegate _getRelativeMouseMode;
    private static readonly GetRelativeMouseStateDelegate _getRelativeMouseState;

    static Sdl2Native()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _setRelativeMouseMode = SDL_SetRelativeMouseMode_Windows;
            _getError = SDL_GetError_Windows;
            _getRelativeMouseMode = SDL_GetRelativeMouseMode_Windows;
            _getRelativeMouseState = SDL_GetRelativeMouseState_Windows;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _setRelativeMouseMode = SDL_SetRelativeMouseMode_Linux;
            _getError = SDL_GetError_Linux;
            _getRelativeMouseMode = SDL_GetRelativeMouseMode_Linux;
            _getRelativeMouseState = SDL_GetRelativeMouseState_Linux;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _setRelativeMouseMode = SDL_SetRelativeMouseMode_Mac;
            _getError = SDL_GetError_Mac;
            _getRelativeMouseMode = SDL_GetRelativeMouseMode_Mac;
            _getRelativeMouseState = SDL_GetRelativeMouseState_Mac;
        }
        else
        {
            throw new PlatformNotSupportedException(
                $"SDL2 is not configured for {RuntimeInformation.OSDescription}."
            );
        }
    }

    public static void SetRelativeMouseMode(bool enabled)
    {
        SDL_bool value = enabled ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE;

        int result = _setRelativeMouseMode(value);

        if (result != 0)
        {
            throw new InvalidOperationException(
                $"SDL_SetRelativeMouseMode failed: {GetError()}"
            );
        }
    }

    public static bool GetRelativeMouseMode()
    {
        return _getRelativeMouseMode() != 0;
    }

    public static uint GetRelativeMouseState(out int x, out int y)
    {
        return _getRelativeMouseState(out x, out y);
    }

    private static string GetError()
    {
        IntPtr errorPtr = _getError();

        return Marshal.PtrToStringAnsi(errorPtr) ?? "Unknown SDL error.";
    }

    [DllImport("SDL2.dll", EntryPoint = "SDL_SetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_SetRelativeMouseMode_Windows(SDL_bool enabled);

    [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_SetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_SetRelativeMouseMode_Linux(SDL_bool enabled);

    [DllImport("libSDL2-2.0.0.dylib", EntryPoint = "SDL_SetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_SetRelativeMouseMode_Mac(SDL_bool enabled);

    [DllImport("SDL2.dll", EntryPoint = "SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetError_Windows();

    [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetError_Linux();

    [DllImport("libSDL2-2.0.0.dylib", EntryPoint = "SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr SDL_GetError_Mac();

    [DllImport("SDL2.dll", EntryPoint = "SDL_GetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_GetRelativeMouseMode_Windows();

    [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_GetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_GetRelativeMouseMode_Linux();

    [DllImport("libSDL2-2.0.0.dylib", EntryPoint = "SDL_GetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
    private static extern int SDL_GetRelativeMouseMode_Mac();

    [DllImport("SDL2.dll", EntryPoint = "SDL_GetRelativeMouseState", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint SDL_GetRelativeMouseState_Windows(out int x, out int y);

    [DllImport("libSDL2-2.0.so.0", EntryPoint = "SDL_GetRelativeMouseState", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint SDL_GetRelativeMouseState_Linux(out int x, out int y);

    [DllImport("libSDL2-2.0.0.dylib", EntryPoint = "SDL_GetRelativeMouseState", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint SDL_GetRelativeMouseState_Mac(out int x, out int y);
}
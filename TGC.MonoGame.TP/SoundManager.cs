using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public class SoundManager
{
    private static SoundManager _instance;

    public static SoundManager Initialize(ContentManager content)
    {
        if (_instance == null)
        {
            _instance = new SoundManager(content);
        }
        return _instance;
    }

    public static SoundManager GetInstance()
    {
        if (_instance == null)
        {
            throw new InvalidOperationException("SoundManager no fue inicializado");
        }
        return _instance;
    }




    private SoundEffect _motorSound;
    private SoundEffectInstance _motorSoundInstance;
    private bool _motorStarted;

    private SoundEffect _brakeSound;
    private SoundEffectInstance _brakeSoundInstance;

    private SoundEffect _recolectarMoneda;
    private SoundEffectInstance _recolectarMonedaInstance;

    private SoundEffect _reparacionAuto;
    private SoundEffectInstance _reparacionAutoInstance;

    private SoundEffect _cargarCombustible;
    private SoundEffectInstance _cargarCombustibleInstance;

    private SoundEffect _explosion;
    private SoundEffect _golpe;

    public SoundEffectInstance MotorSoundInstance => _motorSoundInstance;
    public SoundEffectInstance BrakeSoundInstance => _brakeSoundInstance;

    
    private Song _backgroundSong;
    
    private SoundEffect _motorStartSound;
    private SoundEffectInstance _motorStartInstance;
    private bool _waitingForStartComplete;
    public SoundManager(ContentManager content)
    {
        // Motor (no iniciar automáticamente para evitar sonido en el menú)
        _motorSound = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "motor_auto");
        _motorSoundInstance = _motorSound.CreateInstance();
        _motorSoundInstance.IsLooped = true;

        _brakeSound = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "auto_frenando");
        _brakeSoundInstance = _brakeSound.CreateInstance();

        _motorStartSound = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "arranqueMotor");
        _motorStartInstance = _motorStartSound.CreateInstance();
        _motorStartInstance.Volume = 1f;
        
        _backgroundSong = content.Load<Song>(AssetPaths.ContentFolderSounds + "cancionFondo");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.5f;
        MediaPlayer.Play(_backgroundSong);

        _recolectarMoneda = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "efectoMoneda");
        _recolectarMonedaInstance = _recolectarMoneda.CreateInstance();
        _reparacionAuto = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "efectoReparacionAuto");
        _reparacionAutoInstance = _reparacionAuto.CreateInstance();
        _cargarCombustible = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "efectoCargaCombustible");
        _cargarCombustibleInstance = _cargarCombustible.CreateInstance();

        _explosion = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "choque");
        _golpe = content.Load<SoundEffect>(AssetPaths.ContentFolderSounds + "raspon");
    }


    public void SonarCargaCombustible()
    {
        _cargarCombustibleInstance.Play();
    }

    public void SonarReparacionAuto()
    {
       _reparacionAutoInstance.Play();
    }

    public void SonarRecoleccionMoneda()
    {
        _recolectarMoneda.Play();
    }

    public void SonarExplosion()
    {
        _explosion.Play();
    }

    public void SonarGolpe()
    {
        _golpe.Play();
    }

    public void StartMotorSound()
    {
        if (_motorStarted)
        {
            return;
        }

        _motorStarted = true;
        if (_motorStartInstance != null)
        {
            _motorStartInstance.Volume = 1.0f;
            _motorStartInstance.Play();
            _waitingForStartComplete = true;
        }
        else if (_motorSoundInstance.State != SoundState.Playing)
        {
            _motorSoundInstance.Play();
        }
    }

    public void StopMotorSound()
    {
        if (_motorSoundInstance.State == SoundState.Playing)
        {
            _motorSoundInstance.Stop();
        }

        _motorStarted = false;
    }

    public void Update(GameTime gameTime)
    {
        if (_waitingForStartComplete && _motorStartInstance != null)
        {
            if (_motorStartInstance.State != SoundState.Playing)
            {
                _waitingForStartComplete = false;
                if (_motorSoundInstance.State != SoundState.Playing)
                {
                    _motorSoundInstance.Play();
                }
            }
        }
    }
}

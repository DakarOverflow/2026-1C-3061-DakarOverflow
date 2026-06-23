using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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




    private SoundEffect _recolectarMoneda;
    private SoundEffectInstance _recolectarMonedaInstance;

    private SoundEffect _reparacionAuto;
    private SoundEffectInstance _reparacionAutoInstance;

    private SoundEffect _cargarCombustible;
    private SoundEffectInstance _cargarCombustibleInstance;

    private SoundEffect _explosion;
    private SoundEffect _golpe;

    public SoundManager(ContentManager content)
    {
        //TODO: Traer los sonidos del auto acá 

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
    
}
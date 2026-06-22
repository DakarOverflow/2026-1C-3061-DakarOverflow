using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TGC.MonoGame.TP;

public enum VehicleType
{
    Light,
    Medium,
    Heavy
}

public class Vehicle
{
    // Atributos
    private readonly CustomModel _model;
    public Vector3 Position;
    public float RotationY;
    private const float ModelRotationOffset = MathHelper.Pi;

    // Stats según tipo de vehículo
    private readonly VehicleStats _stats;

    public float _speed;
    private float _currentAcceleration;

    // Tipo de vehículo
    public VehicleType Type { get; }

    // Combustible, salud y puntaje
    public float CurrentFuel { get; private set; }
    public float CurrentHealth { get; private set; }
    public int Score { get; private set; }

    // Colisiones
    public BoundingBox BoundingBox { get; private set; }

    public float FrictionCoefficient { get; set; } = 1f;

    public const float ScaleFactor = 0.5f;

    private readonly Vector3 _boundingBoxHalfSize = new Vector3(100f, 50f, 100f) * ScaleFactor;
    private readonly Vector3 _boundingBoxOffset = new Vector3(0f, 40f, 0f) * ScaleFactor;

    // Constructor
    public Vehicle(CustomModel model, Vector3 initialPosition, VehicleStats stats, VehicleType type)
    {
        _model = model;
        Position = initialPosition;
        _stats = stats;
        Type = type;
        RotationY = 0f;
        _speed = 0f;
        _currentAcceleration = 0f;

        // Se inicializan los medidores en su máximo en base a los stats del tipo de vehículo seleccionado.
        CurrentFuel = _stats.FuelCapacity;
        CurrentHealth = _stats.MaxHealth;
        Score = 0;

        // --- PREPARACIÓN PARA COLISIONES ---
        // Inicializar el atributo de colisiones.
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keyboard = Keyboard.GetState();

        // Si el auto se queda sin nafta o salud, el auto no debería poder seguir
        if (CurrentFuel <= 0 || CurrentHealth <= 0)
        {
            _speed = 0;
            return; // TODO: Manejar Game Over en TGCGame
        }
        // Consumo de combustible progresivo
        CurrentFuel -= _stats.FuelConsumption * deltaTime;


        // =========================
        // ACELERAR / FRENAR
        // =========================

        // acelera más al comienzo pero acelera menos cuando ya tiene velocidad
        float normalizedSpeed = Math.Abs(_speed) / _stats.MaxSpeed;
        float accelFactor = MathHelper.Clamp(1f - normalizedSpeed, 0f, 1f);
        float lerpFactor = MathHelper.Clamp(_stats.AccelerationRate * deltaTime, 0f, 1f);

        // =========================
        // FRENAR
        // =========================

        if (keyboard.IsKeyDown(Keys.S))
        {
            // contemplada lógica para baja suave de la aceleración al frenar
            _currentAcceleration = MathHelper.Lerp(
                _currentAcceleration,
                0f,
                lerpFactor
            );


            _speed -= _stats.BrakeForce * FrictionCoefficient * deltaTime;
        }
        else
        {
            float targetAcceleration = _stats.MaxAcceleration * accelFactor;

            _currentAcceleration = MathHelper.Lerp(
                _currentAcceleration,
                targetAcceleration,
                lerpFactor
            );


            _speed += _currentAcceleration * FrictionCoefficient * deltaTime;
        }

        // =========================
        // LIMITES VELOCIDAD
        // =========================

        _speed = MathHelper.Clamp(_speed, _stats.MinSpeed, _stats.MaxSpeed * MathHelper.Clamp(FrictionCoefficient, 0.1f, 1f));

        // =========================
        // GIRAR
        // =========================

        // Gira menos mientras más velocidad tiene el vehículo
        float speedFactor = Math.Abs(_speed) / _stats.MaxSpeed;
        float turnMultiplier = MathHelper.Lerp(
            1f,
            0.4f,
            speedFactor
        );

        float currentTurnSpeed = _stats.TurnSpeed * turnMultiplier * FrictionCoefficient;

        if (Math.Abs(_speed) > 5f)
        {
            float steeringDirection = _speed >= 0f ? 1f : -1f;

            if (keyboard.IsKeyDown(Keys.A))
            {
                RotationY += currentTurnSpeed * steeringDirection * deltaTime;
            }

            if (keyboard.IsKeyDown(Keys.D))
            {
                RotationY -= currentTurnSpeed * steeringDirection * deltaTime;
            }
        }

        // =========================
        // AVANZAR
        // =========================

        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(RotationY));
        Position += forward * _speed * deltaTime;

        // Colisiones
        UpdateBoundingBox();
    }

    public void UpdateSound(SoundEffectInstance motorSound, SoundEffect breakingEffect)
    {
        // Si esta desacelerando reproducimos el sonido del freno
        if (this._currentAcceleration < 0f && Keyboard.GetState().IsKeyDown(Keys.S))
        {
            breakingEffect.Play();
        }

        //Ajustamos el sonido del motor según la aceleración actual
        motorSound.Pitch = MathHelper.Lerp(-0.8f, 0.8f, this._currentAcceleration/200f);
        motorSound.Volume = MathHelper.Clamp(MathHelper.Lerp(0.3f, 0.8f, this._currentAcceleration), 0f, 1f);
    }

    // ==========================================
    // MÉTODOS PARA INTERACTUAR CON COLECCIONABLES
    // ==========================================

    // Agrega combustible al tanque del vehiculo. Si llega al maximo no se agrega mas.
    public void AddFuel(float amount)
    {
        CurrentFuel += amount;
        if (CurrentFuel > _stats.FuelCapacity) CurrentFuel = _stats.FuelCapacity;
    }

    // Repara el vehiculo sumandole puntos de salud. Si llega al maximo de salud no se suma mas.
    public void RepairDamage(float amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > _stats.MaxHealth) CurrentHealth = _stats.MaxHealth;
    }

    // Suma puntos al puntaje del jugador en formato entero
    public void AddScore(float amount)
    {
        Score += (int)amount;
    }

    // ==========================================
    // MÉTODOS PARA LAS COLISIONES DEL VEHÍCULO
    // ==========================================
    private void UpdateBoundingBox()
    {
        BoundingBox = new BoundingBox(Position - _boundingBoxHalfSize + _boundingBoxOffset
            , Position + _boundingBoxHalfSize + _boundingBoxOffset);
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if(CurrentHealth < 0) CurrentHealth = 0;
    }

    public void CollisionImpact(float damage, float speedMultiplier)
    {
        TakeDamage(damage);

        speedMultiplier *= speedMultiplier;
    }
    // ==========================================
    // MÉTODOS PARA DIBUJAR EL VEHÍCULO
    // ==========================================
    public Matrix GetWorld()
    {
        return
            Matrix.CreateRotationY(RotationY) *
            Matrix.CreateTranslation(Position);
    }
    public Matrix GetVisualWorld()
    {
        return
            Matrix.CreateScale(ScaleFactor) *
            Matrix.CreateRotationY(RotationY + ModelRotationOffset) *
            Matrix.CreateTranslation(Position);
    }
    public void Draw(GameTime gameTime, Camera camera)
    {
        _model.Draw(
            GetVisualWorld(),
            camera.GetView(),
            camera.GetProjection()
        );
    }
}
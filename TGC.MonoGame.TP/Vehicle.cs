using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
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
    private const float Friction = 150f;

    // Tipo de vehículo
    public VehicleType Type { get; }

    // Combustible, salud y puntaje
    public float CurrentFuel { get; private set; }
    public float CurrentHealth { get; private set; }
    public int Score { get; private set; }

    // --- PREPARACIÓN PARA COLISIONES ---
    // TODO: definir atributo

    // Constructor
    public Vehicle(CustomModel model, Vector3 initialPosition, VehicleStats stats, VehicleType type)
    {
        _model = model;
        Position = initialPosition;
        _stats = stats;
        Type = type;
        RotationY = 0f;
        _speed = 0f;

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
        // ACELERAR
        // =========================

        // TODO: El enunciado dice "El auto del jugador acelerará automáticamente, dejando solo a control del jugador el giro y el freno". Es decir, el auto debe acelerar automáticamnete.
        // TODO: BORRAR EL USO DE LA TECLA W PARA ACELERAR

        if (keyboard.IsKeyDown(Keys.W))
        {
            if (_speed < 0f) _speed += _stats.BrakeForce * deltaTime;
            else _speed += _stats.Acceleration * deltaTime;
        }
        else
        {
            // Desaceleracion natural
            if (_speed > 0f) _speed -= Friction * deltaTime;
            else if (_speed < 0f) _speed += Friction * deltaTime;
            else if (_speed == 0f) _speed = 0;
        }

        // =========================
        // FRENAR
        // =========================

        if (keyboard.IsKeyDown(Keys.S))
        {
            _speed -= _stats.BrakeForce * deltaTime;
        }

        // =========================
        // LIMITES VELOCIDAD
        // =========================

        _speed = MathHelper.Clamp(_speed, _stats.MinSpeed, _stats.MaxSpeed);

        // =========================
        // GIRAR
        // =========================

        if (_speed > 5f)
        {
            float steeringDirection = _speed >= 0f ? 1f : -1f;

            if (keyboard.IsKeyDown(Keys.A))
            {
                RotationY += _stats.TurnSpeed * steeringDirection * deltaTime;
            }

            if (keyboard.IsKeyDown(Keys.D))
            {
                RotationY -= _stats.TurnSpeed * steeringDirection * deltaTime;
            }
        }

        // =========================
        // AVANZAR
        // =========================

        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(RotationY));
        Position += forward * _speed * deltaTime;

        // --- PREPARACIÓN PARA COLISIONES ---
        // Actualizar el atributo de colisiones respecto a la posicion del vehiculo
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
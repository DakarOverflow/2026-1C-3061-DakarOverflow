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
    private readonly CustomModel _bodyModel;
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
    public OrientedBoundingBox OBB { get; private set; }

    public const float ScaleFactor = 0.5f;

    private readonly Vector3 _boundingBoxHalfSize = new Vector3(100f, 50f, 100f) * ScaleFactor;
    private readonly Vector3 _boundingBoxOffset = new Vector3(0f, 40f, 0f) * ScaleFactor;


    // Ruedas
    private Wheel  _frontLeftWheel;
    private Wheel  _frontRightWheel;
    private Wheel  _backLeftWheel;
    private Wheel  _backRightWheel;
    private float _wheelSpin;
    private float _wheelSteeringAngle;

    private bool _exploded;

    // Constructor
    public Vehicle(CustomModel bodyModel, CustomModel wheelModel, Vector3 initialPosition, VehicleStats stats, VehicleType type, Vector3 frontLeftWheelPosition, Vector3 backLeftWheelPosition)
    {
        _bodyModel = bodyModel;
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

        _frontLeftWheel = new Wheel(wheelModel, frontLeftWheelPosition, false, ScaleFactor);   //Vector3(-40f,0f,60f)

        _frontRightWheel = new Wheel(wheelModel, frontLeftWheelPosition * new Vector3(-1f,1f,1f), true, ScaleFactor);

        _backLeftWheel = new Wheel(wheelModel, backLeftWheelPosition, false, ScaleFactor);

        _backRightWheel = new Wheel(wheelModel, backLeftWheelPosition * new Vector3(-1f,1f,1f), true, ScaleFactor);

        // --- PREPARACIÓN PARA COLISIONES ---
        // Inicializar el atributo de colisiones.
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keyboard = Keyboard.GetState();

        // Si el auto se queda sin nafta o salud, el auto no debería poder seguir
        if (CurrentFuel <= 0)
        {
            _speed = 0;
            return;
        }
        if(CurrentHealth <= 0) 
        {
            Explode();
            _speed = 0;
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

        if (keyboard.IsKeyDown(Keys.S) && !_exploded)
        {
            // contemplada lógica para baja suave de la aceleración al frenar
            _currentAcceleration = MathHelper.Lerp(
                _currentAcceleration,
                0f,
                lerpFactor
            );

            _speed -= _stats.BrakeForce * deltaTime;
        }
        else if(!_exploded)
        {
            float targetAcceleration = _stats.MaxAcceleration * accelFactor;

            _currentAcceleration = MathHelper.Lerp(
                _currentAcceleration,
                targetAcceleration,
                lerpFactor
            );

            _speed += _currentAcceleration * deltaTime;
        }


        _wheelSpin += _speed * deltaTime * 0.01f;

        _frontLeftWheel.SpinRotation = _wheelSpin;
        _frontRightWheel.SpinRotation = _wheelSpin;
        _backLeftWheel.SpinRotation = _wheelSpin;
        _backRightWheel.SpinRotation = _wheelSpin;

        _frontLeftWheel.VehicleRotation = RotationY + ModelRotationOffset;
        _frontRightWheel.VehicleRotation = RotationY + ModelRotationOffset;
        _backLeftWheel.VehicleRotation = RotationY + ModelRotationOffset;
        _backRightWheel.VehicleRotation = RotationY + ModelRotationOffset;

        // =========================
        // LIMITES VELOCIDAD
        // =========================

        _speed = MathHelper.Clamp(_speed, _stats.MinSpeed, _stats.MaxSpeed);

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

        float currentTurnSpeed = _stats.TurnSpeed * turnMultiplier;

        if (Math.Abs(_speed) > 5f)
        {
            float steeringDirection = _speed >= 0f ? 1f : -1f;

            if (keyboard.IsKeyDown(Keys.A) && !_exploded)
            {
                RotationY += currentTurnSpeed * steeringDirection * deltaTime;
                _wheelSteeringAngle = MathHelper.Lerp(_wheelSteeringAngle, MathHelper.ToRadians(25f), 0.1f);
            }

            else if (keyboard.IsKeyDown(Keys.D) && !_exploded)
            {
                RotationY -= currentTurnSpeed * steeringDirection * deltaTime;
                _wheelSteeringAngle = MathHelper.Lerp(_wheelSteeringAngle, MathHelper.ToRadians(-25f), 0.1f);
            }
            else
            {
                _wheelSteeringAngle = MathHelper.Lerp(_wheelSteeringAngle, 0f, 0.1f);
            }
        }

        _frontLeftWheel.SteeringRotation = _wheelSteeringAngle;
        _frontRightWheel.SteeringRotation = _wheelSteeringAngle;

        // =========================
        // AVANZAR
        // =========================

        Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(RotationY));
        Position += forward * _speed * deltaTime;

        // =========================
        // ACTUALIZAR RUEDAS
        // =========================
        Matrix vehicleWorld = GetVisualWorld();

        _frontLeftWheel.Update(gameTime, vehicleWorld);
        _frontRightWheel.Update(gameTime, vehicleWorld);
        _backLeftWheel.Update(gameTime, vehicleWorld);
        _backRightWheel.Update(gameTime, vehicleWorld);

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
        Vector3 center = Position + _boundingBoxOffset;
        Matrix orientation = Matrix.CreateRotationY(RotationY);
        OBB = new OrientedBoundingBox(center, _boundingBoxHalfSize, orientation);

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

        _speed *= speedMultiplier;
    }


    private void Explode()
    {
        if(_exploded) return;

        _exploded = true;

        Random r = new();

        _frontLeftWheel.Detach(new Vector3(r.Next(-500,500), 800, r.Next(-500,500)));
        _frontRightWheel.Detach(new Vector3(r.Next(-500,500), 800, r.Next(-500,500)));
        _backLeftWheel.Detach(new Vector3(r.Next(-500,500), 800, r.Next(-500,500)));
        _backRightWheel.Detach(new Vector3(r.Next(-500,500), 800, r.Next(-500,500)));
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
    public void SetShadowMap(Texture2D shadowMap, Matrix lightViewProjection)
    {
        _bodyModel.SetShadowMap(shadowMap, lightViewProjection);
        _frontLeftWheel.SetShadowMap(shadowMap, lightViewProjection);
        _frontRightWheel.SetShadowMap(shadowMap, lightViewProjection);
        _backLeftWheel.SetShadowMap(shadowMap, lightViewProjection);
        _backRightWheel.SetShadowMap(shadowMap, lightViewProjection);
    }

    public void DrawDepth(Matrix lightViewProjection)
    {
        _bodyModel.DrawDepth(GetVisualWorld(), lightViewProjection);
        _frontLeftWheel.DrawDepth(lightViewProjection);
        _frontRightWheel.DrawDepth(lightViewProjection);
        _backLeftWheel.DrawDepth(lightViewProjection);
        _backRightWheel.DrawDepth(lightViewProjection);
    }

    public void Draw(GameTime gameTime, Camera camera)
    {
        _bodyModel.Draw(
            GetVisualWorld(),
            camera.GetView(),
            camera.GetProjection());
        
        _frontLeftWheel.Draw(camera);
        _frontRightWheel.Draw(camera);
        _backLeftWheel.Draw(camera);
        _backRightWheel.Draw(camera);
    }
}
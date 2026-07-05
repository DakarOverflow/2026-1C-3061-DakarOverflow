using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public class Obstacle : WorldObject
{
    public OrientedBoundingBox OBB { get; private set; }

    public float Damage { get; }

    public float SpeedMultiplier { get; }

    public bool IsFatalOnFrontalCollision { get; }

    public bool IsActive { get; private set; }

    private readonly Vector3 _hitboxSize;

    private readonly Vector3 _hitboxOffset;

    private readonly float _rotationY;

    private readonly float _forwardSpeed;

    // La orientación del hitbox es fija: es la misma rotación con la que se construyó el obstáculo
    private readonly Matrix _hitboxOrientation;

    public Vector3 Position { get; private set; }

    public Obstacle(
        CustomModel model,
        Matrix world,
        Vector3 position,
        Vector3 hitboxSize,
        Vector3 hitboxOffset,
        float damage,
        float speedMultiplier,
        bool isFatalOnFrontalCollision = false,
        float rotationY = 0f,
        float forwardSpeed = 0f
    ) : base(model, world)
    {
        Position = position + hitboxOffset;

        _hitboxSize = hitboxSize;

        _hitboxOffset = hitboxOffset;

        Damage = damage;

        SpeedMultiplier = speedMultiplier;

        IsFatalOnFrontalCollision = isFatalOnFrontalCollision;

        _rotationY = rotationY;

        _forwardSpeed = forwardSpeed;

        _hitboxOrientation = Matrix.CreateRotationY(_rotationY);

        IsActive = true;

        UpdateOBB();
    }

    private void UpdateOBB()
    {
        Vector3 half = _hitboxSize / 2f;

        OBB = new OrientedBoundingBox(Position, half, _hitboxOrientation);
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        if (_forwardSpeed != 0f)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // OJO: misma rotación (sin negar) que se usa para orientar el modelo y el hitbox
            Vector3 forward = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(_rotationY));
            Vector3 displacement = forward * _forwardSpeed * deltaTime;

            Position += displacement;

            setWorld(World * Matrix.CreateTranslation(displacement));
        }

        UpdateOBB();
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
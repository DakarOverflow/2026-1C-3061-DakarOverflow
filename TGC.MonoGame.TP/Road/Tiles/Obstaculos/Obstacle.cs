using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP;

public class Obstacle : WorldObject
{
    public BoundingBox BoundingBox { get; private set; }

    public float Damage { get; }

    public float SpeedMultiplier { get; }

    public bool IsActive { get; private set; }

    private readonly Vector3 _hitboxSize;

    private readonly Vector3 _hitboxOffset;

    public Vector3 Position { get; }

    public Obstacle(
        CustomModel model,
        Matrix world,
        Vector3 position,
        Vector3 hitboxSize,
        Vector3 hitboxOffset,
        float damage,
        float speedMultiplier
    ) : base(model, world)
    {
        Position = position;

        _hitboxSize = hitboxSize;

        _hitboxOffset = hitboxOffset;

        Damage = damage;

        SpeedMultiplier = speedMultiplier;

        IsActive = true;

        UpdateBoundingBox();
    }

    private void UpdateBoundingBox()
    {
        Vector3 half = _hitboxSize / 2f;
        half += _hitboxOffset;

        BoundingBox = new BoundingBox(
            Position - half,
            Position + half
        );
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsActive)
            return;

        UpdateBoundingBox();
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
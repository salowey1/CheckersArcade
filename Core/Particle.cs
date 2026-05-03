public class Particle
{
	public Vector2 Position;
	public Vector2 Velocity;
	public float Life;
	public float MaxLife;
	public Color Tint;
	public float Size;
	public float Rotation;
	public float RotationSpeed;
	public bool IsActive;

	public void Reset() => IsActive = false;
}
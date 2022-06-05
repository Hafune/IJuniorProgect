namespace Core
{
    public interface IAnimationEvent
    {
        public bool OnGround { get; }
        public float HorizontalVelocity { get; }
    }
}
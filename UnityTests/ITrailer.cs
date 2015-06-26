namespace UnityTests
{
    public interface ITrailer
    {
        IWheel FirstWheel { get; }
        IWheel SecondWheel { get; }
    }

    public class Trailer : ITrailer
    {
        public IWheel FirstWheel { get; private set; }
        public IWheel SecondWheel { get; private set; }

        public Trailer(IWheel firstWheel, IWheel secondWheel)
        {
            FirstWheel = firstWheel;
            SecondWheel = secondWheel;
        }
    }
}

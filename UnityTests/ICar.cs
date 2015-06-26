namespace UnityTests
{
    public interface ICar
    {
        IWheel FirstWheel { get; }
        IWheel SecondWheel { get; }
        IWheel ThirdWheel { get; }
        IWheel FourthWheel { get; }

        ITrailer Trailer { get; }
    }

    public class Car : ICar
    {
        public IWheel FirstWheel { get; private set; }
        public IWheel SecondWheel { get; private set; }
        public IWheel ThirdWheel { get; private set; }
        public IWheel FourthWheel { get; private set; }

        public ITrailer Trailer { get; private set; }

        public Car(
            IWheel firstWheel,
            IWheel secondWheel,
            IWheel thirdWheel,
            IWheel fourthWheel,
            ITrailer trailer
            )
        {
            FirstWheel = firstWheel;
            SecondWheel = secondWheel;
            ThirdWheel = thirdWheel;
            FourthWheel = fourthWheel;
            Trailer = trailer;
        }
    }
}

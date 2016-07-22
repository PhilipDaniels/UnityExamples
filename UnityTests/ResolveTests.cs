using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnityTests
{
    /// <summary>
    /// Demonstrate how to perform different types of customized resolution in Unity.
    /// These techniques are useful when you need to go further than simple registration-by-convention,
    /// for example they can be used to ensure that particular instances are configured
    /// with particular types or concrete objects.
    /// See https://msdn.microsoft.com/en-us/library/dn507420%28v=pandp.30%29.aspx
    ///
    /// General points:
    ///   1. We don't care about PropertyOverride because we only use constructor injection.
    ///   2. There exist convenience overloads called "DependencyOverrides" and "ParameterOverrides"
    ///      which allow multiple overrides to be specified, but they are not demonstrated here.
    ///      Usage is obvious.
    /// </summary>
    [TestClass]
    public class ResolveTests
    {
        UnityContainer container;

        [TestInitialize]
        public void InitializeContainer()
        {
            // Initialise the container with defaults. Car is our composition root.
            container = new UnityContainer();
            container.RegisterType(typeof(ICar), typeof(Car));
            container.RegisterType(typeof(IWheel), typeof(DefaultWheel));
            container.RegisterType(typeof(ITrailer), typeof(Trailer));
        }

        [TestMethod]
        public void WhenMultipleNamedRegistrationsExistForTheSameType_ReturnedInstancesAreDifferent()
        {
            // https://msdn.microsoft.com/en-us/library/ff660853(v=pandp.20).aspx
            // and http://sharpfellows.com/post/Unity-IoC-Container-
            container.RegisterType<ICar, Car>("first car");
            container.RegisterType<ICar, Car>("second second");

            var c1 = container.Resolve<ICar>("first car");
            var c2 = container.Resolve<ICar>("second car");
            Assert.AreNotSame(c1, c2);
        }

        /// <summary>
        /// This is the "normal behaviour" when asking the container to resolve a type.
        /// </summary>
        [TestMethod]
        public void WhenNoOverridesAreSpecified_AllWheelsAreResolvedToTheDefaultWheelType()
        {
            var car = container.Resolve<ICar>();

            Assert.AreEqual(Names.DefaultWheelName, car.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.SecondWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.ThirdWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.FourthWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.Trailer.SecondWheel.Name);

            AssertAllWheelsDifferentObjects(car);
        }

        /// <summary>
        /// When an IWheel is needed, use an OverrideWheel.
        /// Applies to the entire object graph.
        /// </summary>
        [TestMethod]
        public void ForDependencyOverrideByTypeAndWithoutTypeRestriction_AllWheelsAreResolvedToTheOverrideWheelType()
        {
            var car = container.Resolve<ICar>(new DependencyOverride<IWheel>(typeof(OverrideWheel)));

            Assert.AreEqual(Names.OverrideWheelName, car.FirstWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.SecondWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.ThirdWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.FourthWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.Trailer.SecondWheel.Name);

            AssertAllWheelsDifferentObjects(car);
        }

        /// <summary>
        /// When an IWheel is needed, use an OverrideWheel, but only do that on the Trailer.
        /// TODO: Does not work with ITrailer.
        /// </summary>
        [TestMethod]
        public void ForDependencyOverrideByTypeAndWithTrailerTypeRestriction_CarWheelsAreDefaultAndTrailerWheelsAreOverridden()
        {
            var car = container.Resolve<ICar>(new DependencyOverride<IWheel>(typeof(OverrideWheel)).OnType<Trailer>());

            Assert.AreEqual(Names.DefaultWheelName, car.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.SecondWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.ThirdWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.FourthWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.Trailer.SecondWheel.Name);

            AssertAllWheelsDifferentObjects(car);
        }

        /// <summary>
        /// Instead of asking the container to generate an instance, we can supply one.
        /// The container holds a reference to it and uses it in singleton-fashion.
        /// </summary>
        [TestMethod]
        public void ForDependencyOverrideByInstanceAndWithoutTypeRestriction_AllWheelsAreResolvedToMyInstance()
        {
            const string WheelName = "My Wheel";

            var myWheel = new DefaultWheel() { Name = WheelName };
            // But you would normally use OnType.
            var car = container.Resolve<ICar>(new DependencyOverride<IWheel>(myWheel));

            Assert.AreEqual(WheelName, car.FirstWheel.Name);
            Assert.AreEqual(WheelName, car.SecondWheel.Name);
            Assert.AreEqual(WheelName, car.ThirdWheel.Name);
            Assert.AreEqual(WheelName, car.FourthWheel.Name);
            Assert.AreEqual(WheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(WheelName, car.Trailer.SecondWheel.Name);

            Assert.AreSame(myWheel, car.FirstWheel);
            Assert.AreSame(myWheel, car.SecondWheel);
            Assert.AreSame(myWheel, car.ThirdWheel);
            Assert.AreSame(myWheel, car.FourthWheel);
            Assert.AreSame(myWheel, car.Trailer.FirstWheel);
            Assert.AreSame(myWheel, car.Trailer.SecondWheel);
        }

        /// <summary>
        /// When a parameter is named "firstWheel", use an instance of OverrideWheel.
        /// This applies throughout the entire group, so it can be dangerous if parameter
        /// names are the same when you aren't expecting it. It is best to use an OnType
        /// restriction.
        /// </summary>
        [TestMethod]
        public void ForParameterOverrideByTypeAndWithoutTypeRestriction_AllWheelsWithSameNameAreOverridden()
        {
            var car = container.Resolve<ICar>(new ParameterOverride("firstWheel", typeof(OverrideWheel)));

            Assert.AreEqual(Names.OverrideWheelName, car.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.SecondWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.ThirdWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.FourthWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.Trailer.SecondWheel.Name);

            AssertAllWheelsDifferentObjects(car);
        }

        /// <summary>
        /// This is the variety of ParameterOverride you should normally use, i.e.
        /// one that includes an OnType restriction.
        /// </summary>
        [TestMethod]
        public void ForParameterOverrideByTypeAndWithTrailerTypeRestriction_AllWheelsWithSameNameAreOverridden()
        {
            var car = container.Resolve<ICar>(new ParameterOverride("firstWheel", typeof(OverrideWheel)).OnType<Trailer>());

            Assert.AreEqual(Names.DefaultWheelName, car.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.SecondWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.ThirdWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.FourthWheel.Name);
            Assert.AreEqual(Names.OverrideWheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.Trailer.SecondWheel.Name);

            AssertAllWheelsDifferentObjects(car);
        }

        [TestMethod]
        public void ForParameterOverrideByInstanceAndWithoutTypeRestriction_AllWheelsWithSameNameAreOverridden()
        {
            const string WheelName = "My Wheel";

            var myWheel = new DefaultWheel() { Name = WheelName };
            // But you would normally use OnType.
            var car = container.Resolve<ICar>(new ParameterOverride("firstWheel", myWheel));

            Assert.AreEqual(WheelName, car.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.SecondWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.ThirdWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.FourthWheel.Name);
            Assert.AreEqual(WheelName, car.Trailer.FirstWheel.Name);
            Assert.AreEqual(Names.DefaultWheelName, car.Trailer.SecondWheel.Name);


            Assert.AreSame(myWheel, car.FirstWheel);
            Assert.AreSame(myWheel, car.Trailer.FirstWheel);
            AssertAllDifferentObjects(car.SecondWheel, car.ThirdWheel, car.FourthWheel, car.Trailer.SecondWheel);
        }






        /// <summary>
        /// Helper assertion.
        /// </summary>
        void AssertAllWheelsDifferentObjects(ICar car)
        {
            AssertAllDifferentObjects(car.FirstWheel, car.SecondWheel, car.ThirdWheel, car.FourthWheel, car.Trailer.FirstWheel, car.Trailer.SecondWheel);
        }

        /// <summary>
        /// Helper assertion.
        /// </summary>
        void AssertAllDifferentObjects(params object[] parameters)
        {
            var prms = parameters.ToArray();

            for (int i = 0; i < prms.Length; i++)
            {
                for (int j = 0; j < prms.Length; j++)
                {
                    if (i != j)
                        Assert.AreNotSame(prms[i], prms[j]);
                }
            }
        }
    }
}

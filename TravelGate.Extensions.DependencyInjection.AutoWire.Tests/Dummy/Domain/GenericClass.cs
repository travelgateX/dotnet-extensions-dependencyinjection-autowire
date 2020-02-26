namespace TravelGate.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Domain
{

    public class GenericClass<T> where T : class
    {
        public override string ToString()
        {
            return typeof(T).FullName;
        }
    }

}

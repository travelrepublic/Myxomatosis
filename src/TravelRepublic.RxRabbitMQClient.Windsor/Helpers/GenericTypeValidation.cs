using System;
using System.Linq;

namespace TravelRepublic.RxRabbitMQClient.Windsor.Helpers
{
    public class GenericTypeValidation
    {
        public Type GetGenericParameter(Type baseType, Type genericInterface)
        {
            if (!genericInterface.IsGenericType) throw new ArgumentException("Expected a generic type", "genericInterface");
            if (genericInterface.GetGenericArguments().Count() != 1) throw new ArgumentException("Expected a generic type with 1 generic parameter", "genericInterface");

            var matchingInterface = baseType.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericArguments().Count() == 1)
                .Where(i => i.GetGenericTypeDefinition() == genericInterface.GetGenericTypeDefinition())
                .SingleOrDefault();

            if (matchingInterface == null)
            {
                throw new Exception(string.Format("Expected {0} to implement {1}", baseType, genericInterface));
            }

            return matchingInterface.GetGenericArguments()[0];
        }

        public bool ValidateGenericParameter(Type baseType, Type genericInterface, Type expectedGenericParameter)
        {
            var calculatedType = GetGenericParameter(baseType, genericInterface);
            return expectedGenericParameter == calculatedType;
        }
    }
}
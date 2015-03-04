using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Core;
using Castle.Core.Configuration;
using Castle.Core.Internal;
using Castle.Core.Logging;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Registration;
using TravelRepublic.RxRabbitMQClient.Configuration;
using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Message;
using TravelRepublic.RxRabbitMQClient.Serialization;
using TravelRepublic.RxRabbitMQClient.Windsor.Attributes;
using TravelRepublic.RxRabbitMQClient.Windsor.Helpers;

namespace TravelRepublic.RxRabbitMQClient.Windsor
{
    public class RxRabbitClientFacility : IFacility
    {
        internal static string QueueNameKey = "Queue_Name";
        internal static string MessageTypeKey = "Queue_Name2";
        internal static string IsEnumerableMessageType = "IsEnum";
        internal static string IsService = "Isservice";
        private readonly List<Func<IConnectionConfigBuilder, IConnectionConfigBuilder>> _customizations;
        private readonly Dictionary<Type, IList<Type>> _interceptingTypes;
        private ISerializer _configuredSerializer;
        private ComponentRegistration<IObservableConnection> _connectionRegistration;
        private IKernel _kernel;

        #region Constructors

        public RxRabbitClientFacility()
        {
            _configuredSerializer = DefaultSerializer.Instance;
            _customizations = new List<Func<IConnectionConfigBuilder, IConnectionConfigBuilder>>();
            _interceptingTypes = new Dictionary<Type, IList<Type>>();
        }

        #endregion

        #region IFacility Members

        public void Init(IKernel kernel, IConfiguration facilityConfig)
        {
            _kernel = kernel;
            _kernel.ComponentModelCreated += ComponentModelCreated;

            _connectionRegistration = Component.For<IObservableConnection>()
                .UsingFactoryMethod(
                    () =>
                    {
                        return ObservableConnectionFactory.Create(
                            c =>
                            {
                                foreach (var customization in _customizations)
                                {
                                    customization.Invoke(c);
                                }
                            });
                    })
                .NamedAutomatically(Guid.NewGuid().ToString());
            _kernel.Register(_connectionRegistration);

            _kernel.Register(Component.For<ISerializer>().Instance(_configuredSerializer));

            foreach (var interceptor in _interceptingTypes)
            {
                _kernel.Register(Component.For(interceptor.Value.ToArray()).ImplementedBy(interceptor.Key).LifeStyle.Singleton);
            }
        }

        public void Terminate()
        {
        }

        #endregion

        private void ComponentModelCreated(ComponentModel model)
        {
            if (model.ExtendedProperties.Contains(QueueNameKey))
            {
                var messageType = (Type) model.ExtendedProperties[MessageTypeKey];
                var isEnumerableMessage = (bool) model.ExtendedProperties[IsEnumerableMessageType];

                if (isEnumerableMessage)
                {
                    var subscriptionHostType = typeof (BatchSubscriptionHost<>).MakeGenericType(messageType);
                    var configs = (IBatchSubscriptionConfig[]) model.ExtendedProperties[QueueNameKey];
                    foreach (var config in configs)
                    {
                        _kernel.Register(
                            Component.For<ISubscription>().NamedAutomatically("Subscriber_" + Guid.NewGuid()).ImplementedBy(subscriptionHostType)
                                .DependsOn(Dependency.OnComponent("connection", _connectionRegistration.Name))
                                .DependsOn(Dependency.OnValue("config", config))
                                .DependsOn(Dependency.OnComponent("handler", model.Name))
                                .LifestyleTransient());
                    }
                }
                else
                {
                    var subscriptionHostType = typeof (SubscriptionHost<>).MakeGenericType(messageType);
                    var configs = (ISubscriptionConfig[]) model.ExtendedProperties[QueueNameKey];
                    foreach (var config in configs)
                    {
                        _kernel.Register(
                            Component.For<ISubscription>().NamedAutomatically("Subscriber_" + Guid.NewGuid()).ImplementedBy(subscriptionHostType)
                                .DependsOn(Dependency.OnComponent("connection", _connectionRegistration.Name))
                                .DependsOn(Dependency.OnValue("config", config))
                                .DependsOn(Dependency.OnComponent("handler", model.Name))
                                .LifestyleTransient());
                    }
                }
            }

            if (model.ExtendedProperties.Contains(IsService))
            {
                foreach (var propertyInfo in model.Implementation.GetProperties())
                {
                    var rabbitMessageHandlerAttribute = propertyInfo.GetCustomAttribute<RabbitMessageHandlerAttribute>();

                    /**
                     * TODO: This needs tidying up and using IL generation instead of reflection
                     * Only works for single message handlers
                     * */

                    var lambdaProperty = Expression.Parameter(model.Implementation, "entity");
                    var property = Expression.Property(lambdaProperty, propertyInfo);
                    var func = typeof (Func<,>).MakeGenericType(model.Implementation, propertyInfo.PropertyType);
                    var @delegatelambda = Expression.Lambda(func, property, lambdaProperty);
                    var @delegate = @delegatelambda.Compile();

                    var handlerComponentName = "Handler_" + Guid.NewGuid();

                    var messageType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    var handlerType = typeof (IRabbitMessageHandler<>).MakeGenericType(messageType);
                    var hand = Component.For(handlerType).UsingFactoryMethod(k => @delegate.DynamicInvoke(k.Resolve(model.Implementation))).Named(handlerComponentName);

                    var subscriptionHostType = typeof (SubscriptionHost<>).MakeGenericType(messageType);

                    var config = rabbitMessageHandlerAttribute.GetConfig();

                    var com = Component.For<ISubscription>().NamedAutomatically("Subscriber_" + Guid.NewGuid())
                        .ImplementedBy(subscriptionHostType)
                        .DependsOn(Dependency.OnComponent("connection", _connectionRegistration.Name))
                        .DependsOn(Dependency.OnValue("config", config))
                        .DependsOn(Dependency.OnComponent("handler", handlerComponentName))
                        .LifestyleTransient();

                    _kernel.Register(hand);
                    _kernel.Register(com);
                }
            }
        }

        public RxRabbitClientFacility WithServer(string host)
        {
            _customizations.Add(c => c.WithHostName(host));
            return this;
        }

        public RxRabbitClientFacility WithServer(string host, string virtualHost)
        {
            _customizations.Add(c => c.WithHostName(host).WithVirtualHost(virtualHost));
            return this;
        }

        public RxRabbitClientFacility WithCredentials(string userName, string password)
        {
            _customizations.Add(c => c.WithUserName(userName).WithPassword(password));
            return this;
        }

        public RxRabbitClientFacility UseLoggingFacility()
        {
            _customizations.Add(c => c.WithLogger(new CastleCoreLogger(_kernel.Resolve<ILogger>())));
            return this;
        }

        public RxRabbitClientFacility AddCustomSerializer<T>(ISerializer serializer)
        {
            _configuredSerializer = serializer;
            _customizations.Add(c => c.UsingSerializer(serializer));
            return this;
        }

        #region Nested type: x

        public class x : IGenericImplementationMatchingStrategy
        {
            #region IGenericImplementationMatchingStrategy Members

            public Type[] GetGenericArguments(ComponentModel model, CreationContext context)
            {
                throw new NotImplementedException();
            }

            #endregion

            public bool Supports(Type service, ComponentModel component)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }

    public static class RxRabbitClientFacilityExtensions
    {
        public static IRegistration HandleMessagesAuto<THandler>(this ComponentRegistration<THandler> registration)
            where THandler : class
        {
            var typeValidator = new GenericTypeValidation();
            var messageType = typeValidator.GetGenericParameter(registration.Implementation, typeof (IRabbitMessageHandler<>));

            registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.MessageTypeKey).Eq(messageType));

            if (messageType.IsGenericType && messageType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            {
                //Batch messaging

                registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.IsEnumerableMessageType).Eq(true));

                var config = registration.Implementation.GetAttribute<RabbitMessageBatchHandlerAttribute>().GetConfig();
                registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.QueueNameKey).Eq(config));

                var realMessageType = messageType.GetGenericArguments()[0];
                registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.MessageTypeKey).Eq(realMessageType));
            }
            else
            {
                registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.IsEnumerableMessageType).Eq(false));

                var config = registration.Implementation.GetAttribute<RabbitMessageHandlerAttribute>().GetConfig();
                registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.QueueNameKey).Eq(config));

                registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.MessageTypeKey).Eq(messageType));
            }

            return registration;
        }

        public static IRegistration MessageService<TMessageService>(this ComponentRegistration<TMessageService> registration) where TMessageService : class
        {
            var rabbitMessageServiceAttribute = registration.Implementation.GetAttribute<RabbitMessageServiceAttribute>();
            if (rabbitMessageServiceAttribute == null)
                throw new Exception("");

            registration.ExtendedProperties(Property.ForKey(RxRabbitClientFacility.IsService).Eq(true));
            return registration;
        }
    }
}
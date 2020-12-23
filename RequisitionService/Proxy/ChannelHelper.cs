using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
namespace GEP.Cumulus.P2P.Req.Service.Proxy
{
    [ExcludeFromCodeCoverage]
    public class ChannelHelper
    {
    }

    [ExcludeFromCodeCoverage]
    public static class GepServiceManager
    {
        private static volatile GepServiceFactory _manager;

        public static GepServiceFactory GetInstance
        {
            get
            {
                return _manager ?? (_manager = new GepServiceFactory());
            }
        }

        public static void ResetInstance()
        {
            if (_manager != null)
                _manager.Dispose();
            _manager = null;
        }
    }

    public class GepServiceFactory : IDisposable
    {
        private static readonly Dictionary<Type, ChannelFactory> Factories = new Dictionary<Type, ChannelFactory>();
        private static readonly object SyncRoot = new object();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual T CreateChannel<T>(string endpointAddress) where T : class
        {
            var readerQuotas = new XmlDictionaryReaderQuotas
            {
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxDepth = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue,
                MaxStringContentLength = int.MaxValue
            };
            var binding = new NetTcpBinding(SecurityMode.None)
            {
                MaxBufferSize = int.MaxValue,
                MaxConnections = 100,
                MaxReceivedMessageSize = int.MaxValue,
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromMinutes(10),
                CloseTimeout = TimeSpan.FromMinutes(10),
                ListenBacklog = int.MaxValue,
                ReaderQuotas = readerQuotas
            };
            return CreateChannel<T>(binding, endpointAddress);
        }

        public virtual T CreateChannel<T>(Binding binding, string endpointAddress) where T : class
        {
            var local = GetFactory<T>(binding, endpointAddress).CreateChannel();
            // ((IClientChannel)local).Faulted += ChannelFaulted;
            return local;
        }

        protected virtual ChannelFactory<T> GetFactory<T>(string endpointConfigurationName, string endpointAddress)
            where T : class
        {
            lock (SyncRoot)
            {
                ChannelFactory factory;
                if (!Factories.TryGetValue(typeof(T), out factory))
                {
                    factory = CreateFactoryInstance<T>(endpointConfigurationName, endpointAddress);
                    Factories.Add(typeof(T), factory);
                }
                return (factory as ChannelFactory<T>);
            }
        }

        protected virtual ChannelFactory<T> GetFactory<T>(Binding binding, string endpointAddress) where T : class
        {
            lock (SyncRoot)
            {
                ChannelFactory factory;
                if (!Factories.TryGetValue(typeof(T), out factory))
                {
                    factory = CreateFactoryInstance<T>(binding, endpointAddress);
                    Factories.Add(typeof(T), factory);
                }
                return (factory as ChannelFactory<T>);
            }
        }

        private static ChannelFactory CreateFactoryInstance<T>(string endpointConfigurationName, string endpointAddress)
        {
            ChannelFactory factory = !string.IsNullOrEmpty(endpointAddress) ? new ChannelFactory<T>(endpointConfigurationName, new EndpointAddress(endpointAddress)) : new ChannelFactory<T>(endpointConfigurationName);
            //factory.Faulted += FactoryFaulted;
            factory.Open();
            return factory;

        }

        private static ChannelFactory CreateFactoryInstance<T>(Binding binding, string endpointAddress)
        {
            ChannelFactory factory = !string.IsNullOrEmpty(endpointAddress) ? new ChannelFactory<T>(binding, new EndpointAddress(endpointAddress)) : new ChannelFactory<T>(binding);
            // factory.Faulted += FactoryFaulted;
            factory.Open();
            return factory;

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            lock (SyncRoot)
            {
                foreach (var factory in Factories.Keys.Select(type => Factories[type]))
                {
                    try
                    {
                        if (factory.State != CommunicationState.Faulted)
                        {
                            factory.Close();
                            continue;
                        }
                        else
                        {
                            factory.Abort();
                            continue;
                        }
                    }
                    catch (CommunicationException)
                    {
                        factory.Abort();
                        continue;
                    }
                    catch (TimeoutException)
                    {
                        factory.Abort();
                        continue;
                    }
                }
                Factories.Clear();
            }
        }
    }

}
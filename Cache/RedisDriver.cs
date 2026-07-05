using SMNETCORE.Common;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace SMNETCORE.Cache.Redis
{
   

    public class ConnectionMultiplexerBagItem
    {
        public ConnectionMultiplexer Connection { get; set; }
        public int MaxPool { get; set; }
        public int Index { get; set; }
    }
    public class ConnectionMultiplexerBag
    {
        public ConcurrentBag<ConnectionMultiplexerBagItem> Connection { get; set; }
        public int Size => Connection.IsValid() ? Connection.Count : 0;
        public int MaxPool { get; set; }
    }

    public class RedisDriver
    {
        public ConfigurationOptions Config { get; set; }
        public String RedisCacheServer { get; set; }
        public String RedisSecreetPassword { get; set; }
        public int RedisCachePort { get; set; }
        public int MaxPool { get; set; }
        public RedisDriver()
        {
            RedisCacheServer = AppSettings.RedisCacheServer;
            RedisCachePort = AppSettings.RedisCachePort;
            RedisSecreetPassword = AppSettings.RedisSecreetPassword;
            MaxPool = 50;
            Config = new ConfigurationOptions()
            {
                EndPoints = { { RedisCacheServer, RedisCachePort } },
                AllowAdmin = true,
                ConnectTimeout = 10000,
                ReconnectRetryPolicy = new LinearRetry(5000),
                KeepAlive = 15,
                ConfigCheckSeconds = 15,
                ConnectRetry = 30,
                SyncTimeout = 10000

            };
            if (!string.IsNullOrEmpty(RedisSecreetPassword))
            {
                Config.Password = RedisSecreetPassword;

            }
            Connect();
        }

        public RedisDriver(ConfigurationOptions _config)
        {
            RedisCacheServer = AppSettings.RedisCacheServer;
            RedisCachePort = AppSettings.RedisCachePort;
            MaxPool = 50;
            Config = _config;
            Connect();
        }
        public RedisDriver(ConfigurationOptions _config, int maxPool)
        {
            MaxPool = maxPool;

            RedisCacheServer = AppSettings.RedisCacheServer;
            RedisCachePort = AppSettings.RedisCachePort;
            MaxPool = 50;
            Config = _config;
            Connect();
        }

        public RedisDriver(int maxPool) : this()
        {
            MaxPool = maxPool;
            RedisCacheServer = AppSettings.RedisCacheServer;
            RedisCachePort = AppSettings.RedisCachePort;
            RedisSecreetPassword = AppSettings.RedisSecreetPassword;
            MaxPool = 50;
            Config = new ConfigurationOptions()
            {
                EndPoints = { { RedisCacheServer, RedisCachePort } },
                AllowAdmin = true,
                ConnectTimeout = 10000,
                ReconnectRetryPolicy = new LinearRetry(5000),
                KeepAlive = 15,
                ConfigCheckSeconds = 15,
                ConnectRetry = 30,
                SyncTimeout = 10000

            };
            if (!string.IsNullOrEmpty(RedisSecreetPassword))
            {
                Config.Password = RedisSecreetPassword;

            }
            Connect();

        }

        private static Lazy<ConnectionMultiplexerBag> _lazyConnectionPool;
        [ThreadStatic]
        private static Lazy<ConnectionMultiplexerBagItem> _lazyConnection;

        public static object InitPoolBag = new object();
        public static object ModifyPoolBag = new object();
        public void Connect(int trying = 0)
        {
            try
            {
                if (_lazyConnection != null && _lazyConnection.IsValueCreated && _lazyConnection.Value.Connection.IsConnected) return;

                if (_lazyConnectionPool == null)
                {
                    lock (InitPoolBag)
                    {
                        _lazyConnectionPool = new Lazy<ConnectionMultiplexerBag>(() =>
                        {
                            Thread.Sleep(100);
                            return new ConnectionMultiplexerBag()
                            {
                                MaxPool = MaxPool,
                                Connection = new ConcurrentBag<ConnectionMultiplexerBagItem>()
                            };
                        }, isThreadSafe: true);
                        Parallel.For(0, _lazyConnectionPool.Value.MaxPool - 1, (i, state) =>
                        {
                            _lazyConnectionPool.Value.Connection.Add(new ConnectionMultiplexerBagItem()
                            {
                                Connection = ConnectionMultiplexer.Connect(Config),
                                MaxPool = MaxPool,
                                Index = i
                            });
                        });
                        _lazyConnectionPool.Value.Connection = _lazyConnectionPool.Value.Connection.OrderBy(x => x.Index).EnumToConcurrentBag();
                    }
                }

                
                var indexToPict = new Random().Next(0, _lazyConnectionPool.Value.Connection.Count() - 1);
                var selectedPool = _lazyConnectionPool.Value.Connection.Count() - 1 < indexToPict ? null : _lazyConnectionPool.Value.Connection.ElementAt(indexToPict);
                if(selectedPool == null)
                {
                    var itemConnection = ConnectionMultiplexer.Connect(Config);
                    selectedPool = new ConnectionMultiplexerBagItem()
                    {
                        Connection = itemConnection,
                        MaxPool = MaxPool,
                        Index = indexToPict
                    };
                    _lazyConnectionPool.Value.Connection.Add(selectedPool);
                }

                if(trying > 0)
                {
                    selectedPool.Connection = ConnectionMultiplexer.Connect(Config);
                }
                _lazyConnection = new Lazy<ConnectionMultiplexerBagItem>(() =>
                {
                    return selectedPool;
                }, isThreadSafe: true);
                
                
                if (_lazyConnection.IsValueCreated && !_lazyConnection.Value.Connection.IsConnected && trying < 5)
                {
                    _lazyConnection = null;
                    Connect(trying++);
                }
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.Common);
                if (trying < 5) Connect(trying++);
            }
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                if (_lazyConnection == null)
                {
                    Connect();
                }
                return _lazyConnection.Value.Connection;
            }
        }

        public IServer Server
        {
            get
            {
                return Connection.GetServer(RedisCacheServer, RedisCachePort);
            }
        }
    }
}

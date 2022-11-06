using SP.StudioCore.Array;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Cache.Redis
{
    public class RedisManager
    {
        private static Dictionary<string, ConnectionMultiplexer> instance;
        private static readonly object locker = new object();


        public RedisManager(string connection)
        {
            lock (locker)
            {
                if (instance == null) instance = new Dictionary<string, ConnectionMultiplexer>();

                if (!instance.ContainsKey(connection))
                {
                    ConfigurationOptions opt = ConfigurationOptions.Parse(connection);
                    opt.SyncTimeout = int.MaxValue;
                    opt.AllowAdmin = true;
                    instance.Add(connection, ConnectionMultiplexer.Connect(opt));
                }

            }
        }

        public ConnectionMultiplexer Instance(string connection)
        {
            return instance.Get(connection);
        }

        public virtual IDatabase NewExecutor(string connection, int db = -1)
        {
            return this.Instance(connection).GetDatabase(db);
        }
    }
}

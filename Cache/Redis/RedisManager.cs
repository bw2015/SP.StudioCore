using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Cache.Redis
{
    public class RedisManager
    {
        private static ConnectionMultiplexer instance;
        private static readonly object locker = new object();


        public RedisManager(string connection)
        {
            lock (locker)
            {
                if (instance == null)
                {
                    ConfigurationOptions opt = ConfigurationOptions.Parse(connection);
                    opt.SyncTimeout = int.MaxValue;
                    opt.AllowAdmin = true;
                    instance = ConnectionMultiplexer.Connect(opt);
                }
            }
        }

        public ConnectionMultiplexer Instance()
        {
            return instance;
        }

        public virtual IDatabase NewExecutor(int db = -1)
        {
            return this.Instance().GetDatabase(db);
        }
    }
}

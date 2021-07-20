using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using SP.StudioCore.LinkTrack;

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
            return new LinkTrackWarp(this.Instance().GetDatabase(db));
            //if (dbCache.ContainsKey(db))
            //{
            //    return dbCache[db];
            //}
            //else
            //{
            //    IDatabase database = this.Instance().GetDatabase(db);
            //    lock (locker)
            //    {
            //        if (!dbCache.ContainsKey(db)) dbCache.Add(db, database);
            //    }
            //    return database;
            //}
        }
    }
}

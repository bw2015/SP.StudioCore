using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SP.StudioCore.Enums;
using SP.StudioCore.Security;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.StudioCore.Cache.Redis
{
    public abstract class RedisCacheBase
    {
        /// <summary>
        /// Redis缓存操作数据库对象
        /// </summary>
        private readonly RedisManager db;

        public RedisCacheBase(string connectionString)
        {
            this.db = new RedisManager(connectionString);
        }

        protected IConnectionMultiplexer Connection()
        {
            return this.db.Instance();
        }

        /// <summary>
        /// 获取Database
        /// </summary>
        /// <param name="db">库</param>
        /// <returns></returns>
        protected IDatabase NewExecutor()
        {
            return this.db.NewExecutor(this.DB_INDEX);
        }

        /// <summary>
        /// 默认的数据库
        /// </summary>
        protected virtual int DB_INDEX => -1;

        #region ======== TOKEN单点授权  ========

        /// <summary>
        /// 创建一个与Value有关的密钥（前2位使用value的MD5值），后续拼接随机Guid的值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Guid CreateNewGuid(int id)
        {
            string md5 = Encryption.toMD5(id.ToString()).Substring(0, 2);
            string guid = Guid.NewGuid().ToString("N").Substring(2);
            return Guid.Parse(string.Concat(md5, guid));
        }

        /// <summary>
        /// 获取Token所在的表
        /// </summary>
        /// <param name="KEY"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected string GetTokenKey(string KEY, Guid token)
        {
            return $"{KEY}:{token.ToString("N").Substring(0, 2).ToUpper()}";
        }

        /// <summary>
        /// 获取ID对应Token所在的表
        /// </summary>
        /// <param name="KEY"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected string GetTokenKey(string KEY, int id)
        {
            string md5 = Encryption.toMD5(id.ToString()).Substring(0, 2);
            return $"{KEY}:{md5}";
        }

        /// <summary>
        /// KEY = KEY:MD5(value).sub(0,2)
        /// </summary>
        /// <param name="KEY"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string GetTokenKey(string KEY, string value)
        {
            string md5 = Encryption.toMD5(value).Substring(0, 2);
            return $"{KEY}:{md5}";
        }

        /// <summary>
        /// 产生一个Token，并且让旧Token失效
        /// </summary>
        /// <param name="KEY"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual Guid SaveToken(string KEY, int id)
        {
            Guid newToken = this.CreateNewGuid(id);

            //#1 通过ID找到Token
            string IdKey = $"{KEY}:ID";
            RedisValue oldToken = this.NewExecutor().HashGet(IdKey, id);

            //#2 批量操作删除 TOKEN->ID   更新 ID->TOKEN    添加 TOKEN->ID
            string tokenKey = this.GetTokenKey(KEY, id);
            IBatch batch = this.NewExecutor().CreateBatch();
            if (!oldToken.IsNull) batch.HashDeleteAsync(tokenKey, oldToken);
            batch.HashSetAsync(IdKey, id, newToken.GetRedisValue());
            batch.HashSetAsync(tokenKey, newToken.GetRedisValue(), id);
            batch.Execute();
            return newToken;
        }

        /// <summary>
        /// 删除Token
        /// </summary>
        /// <param name="KEY">前缀KEY</param>
        /// <param name="id">要删除的ID</param>
        protected virtual void RemoveToken(string KEY, int id)
        {
            //#1 通过ID找到Token
            string IdKey = $"{KEY}:ID";
            RedisValue token = this.NewExecutor().HashGet(IdKey, id);

            //#2 批量操作删除 ID->TOKEN  TOKEN->ID
            string tokenKey = this.GetTokenKey(KEY, id);
            IBatch batch = this.NewExecutor().CreateBatch();
            batch.HashDeleteAsync(IdKey, id);
            if (!token.IsNull) batch.HashDeleteAsync(tokenKey, token);
            batch.Execute();
        }

        /// <summary>
        /// 根据token得到ID
        /// </summary>
        /// <param name="KEY">前缀KEY</param>
        /// <param name="token">Token内容</param>
        /// <returns></returns>
        protected virtual int GetTokenID(string KEY, Guid token)
        {
            string tokenKey = this.GetTokenKey(KEY, token);
            RedisValue value = this.NewExecutor().HashGet(tokenKey, token.GetRedisValue());
            if (value.IsNull) return 0;
            return value.GetRedisValue<int>();
        }

        #endregion

        #region ========  自增列读取  ========

        /// <summary>
        /// 获取自增列
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual int GetIdentity(string key)
        {
            return (int)this.NewExecutor().StringIncrement(key);
        }

        /// <summary>
        /// 保存初始化的自增列
        /// </summary>
        /// <param name="key"></param>
        /// <param name="id"></param>
        protected virtual void SaveIdentity(string key, int id)
        {
            this.NewExecutor().StringSet(key, id.GetRedisValue());
        }

        #endregion

        #region ========  消息队列写入和读取  ========

        /// <summary>
        /// 写入消息队列（右进）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected virtual void SaveQueue(string key, RedisValue value)
        {
            if (value.IsNull) return;
            this.NewExecutor().ListRightPush(key, value);
        }

        /// <summary>
        /// 写入实体对象的消息队列，序列化成JSON（右进）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="model"></param>
        protected virtual void SaveQueue<T>(string key, T model)
        {
            this.NewExecutor().ListRightPush(key, JsonConvert.SerializeObject(model).GetRedisValue());
        }

        /// <summary>
        /// 读取消息队列，返回实体类（左出）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual IEnumerable<RedisValue> GetQueue(string key)
        {
            while (true)
            {
                RedisValue value = this.NewExecutor().ListLeftPop(key);
                if (value.IsNull) break;
                yield return value;
            }
        }

        /// <summary>
        /// 读取消息队列，返回实体类（左出）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> GetQueue<T>(string key)
        {
            foreach (RedisValue value in this.GetQueue(key))
            {
                yield return JsonConvert.DeserializeObject<T>(value.GetRedisValue<string>());
            }
        }

        #endregion

        #region ========  多语种列的保存  ========

        /// <summary>
        /// 保存多语种标题内容
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="tranlsate"></param>
        protected virtual void SaveLanguage(IBatch batch, string key, string name, Dictionary<Language, string> tranlsate)
        {
            if (tranlsate == null) return;
            batch.SaveLanguage(key, tranlsate, name);
        }

        #endregion

        #region ========  异步任务（Job）  ========

        /// <summary>
        /// 任务列队
        /// </summary>
        private const string JOB = "JOB:";

        /// <summary>
        /// 任务进度
        /// </summary>
        private const string JOB_PROGRESS = "JOBPROGRESS:";

        /// <summary>
        /// 任务总数
        /// </summary>
        private const string JOB_TOTAL = "Total";

        /// <summary>
        /// 当前进度
        /// </summary>
        private const string JOB_COUNT = "Count";

        /// <summary>
        /// 创建Job任务列表（只新建，如果已经存在则返回false）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <param name="list"></param>
        protected virtual bool SaveJob<T>(string jobName, IEnumerable<T> list)
        {
            string key = $"{JOB}{jobName}";
            string progress = $"{JOB_PROGRESS}{jobName}";
            if (this.NewExecutor().KeyExists(key)) return false;
            IBatch batch = this.NewExecutor().CreateBatch();
            foreach (T t in list)
            {
                batch.ListRightPushAsync(key, t.GetRedisValue());
            }
            batch.HashSetAsync(progress, new[]
            {
                new HashEntry(JOB_TOTAL,list.Count()),
                new HashEntry(JOB_COUNT,0)
            });
            batch.Execute();
            return true;
        }

        /// <summary>
        /// 取出任务，并且更新进度
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobName"></param>
        /// <returns></returns>
        protected virtual IEnumerable<T> ExecuteJob<T>(string jobName)
        {
            string key = $"{JOB}{jobName}";
            string progress = $"{JOB_PROGRESS}{jobName}";
            while (true)
            {
                RedisValue value = this.NewExecutor().ListLeftPop(key);
                if (value.IsNull)
                {
                    this.NewExecutor().KeyDelete(progress);
                    break;
                }
                this.NewExecutor().HashIncrement(progress, JOB_COUNT);
                yield return value.GetRedisValue<T>();
            }
        }

        /// <summary>
        /// 获取当前任务进度
        /// </summary>
        /// <param name="jobName"></param>
        /// <returns>是否完成任务</returns>
        protected virtual bool GetJobProgress(string jobName, out int total, out int count)
        {
            string progress = $"{JOB_PROGRESS}{jobName}";
            total = this.NewExecutor().HashGet(progress, JOB_TOTAL).GetRedisValue<int>();
            count = this.NewExecutor().HashGet(progress, JOB_COUNT).GetRedisValue<int>();
            return total > count;
        }

        #endregion

        #region ========  本机缓存  ========

        #endregion
    }

    public abstract class RedisCacheBase<T> : RedisCacheBase where T : RedisCacheBase, new()
    {
        protected RedisCacheBase(string connectionString) : base(connectionString)
        {
        }

        private static T _intance;
        /// <summary>
        /// 单例模式
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (_intance == null) _intance = new T();
            return _intance;
        }
    }
}
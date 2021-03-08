using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SP.StudioCore.Services
{
    public struct ConsoleModel
    {
        public ConsoleModel(string type, int count, int time, string message)
        {
            this.Type = type;
            this.Count = count;
            this.Time = time;
            this.Message = message;
            this.CreateAt = DateTime.Now;
        }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type;

        /// <summary>
        /// 影响行数
        /// </summary>
        public int Count;

        /// <summary>
        /// 处理时间（毫秒）
        /// </summary>
        public int Time;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateAt;

        public override string ToString()
        {
            return string.Format("[{0}]{1}({2}行受影响，耗时{3}ms)", this.Type, this.Message, this.Count, this.Time);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}

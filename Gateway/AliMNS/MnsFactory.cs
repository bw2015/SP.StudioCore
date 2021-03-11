using System;
using System.Collections.Generic;
using System.Text;
using Aliyun.MNS;
using SP.StudioCore.Gateway.AliMNS.Interface;

namespace SP.StudioCore.Gateway.AliMNS
{
    /// <summary>
    /// MNS 工厂
    /// </summary>
    public class MnsFactory
    {

        private string _accessKeyId;

        private string _secretAccessKey;

        private string _endpoint;

        private static IMNS client;


        public MnsFactory(string accessKeyId, string secretAccessKey, string endpoint)
        {
            this._accessKeyId = accessKeyId;
            this._secretAccessKey = secretAccessKey;
            this._endpoint = endpoint;
            client = getInstance();
        }

        private IMNS getInstance()
        {
            if (client == null)
            {
                client = new MNSClient(_accessKeyId, _secretAccessKey, _endpoint);
            }

            return client;
        }

        /// <summary>
        /// 创建主题
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        public IMessageMns CreateTopic(string topicName)
        {

            IMessageMns iMessageMns = new TopicAgent(client, topicName);
            return iMessageMns;
        }

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        public IMessageMns CreateQueue(string topicName)
        {

            IMessageMns iMessageMns = new QueueAgent(topicName);
            return iMessageMns;
        }
    }



}

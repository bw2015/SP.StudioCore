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


        public MnsFactory(string accessKeyId = "LTAI4Fv8hnfMCJ1GzLJB61wn",
            string secretAccessKey = "mhr2puLfvrWqU3aoMBM5XzlKiUlOLd",
            string endpoint = "http://1502656352964375.mns.cn-hangzhou.aliyuncs.com/")
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
        /// 
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        public IMessageMns CreateTopic(string topicName)
        {

            IMessageMns iMessageMns = new TopicAgent(client, topicName);
            return iMessageMns;
        }

        public IMessageMns CreateQueue(string topicName)
        {

            IMessageMns iMessageMns = new QueueAgent(topicName);
            return iMessageMns;
        }
    }

 

}

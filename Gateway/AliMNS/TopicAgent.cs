using System;
using System.Collections.Generic;
using System.Text;
using Aliyun.MNS;
using SP.StudioCore.Gateway.AliMNS.Interface;

namespace SP.StudioCore.Gateway.AliMNS
{
    /// <summary>
    /// 主题推送实现类
    /// </summary>
    public class TopicAgent : IMessageMns
    {
        private Topic topic;
        public TopicAgent(IMNS client, string topicName)
        {
            topic = client.GetNativeTopic(topicName);
        }

        public string Send(string body)
        {
            try
            {
                // 1.1 如果是推送到邮箱，还需要生成PublishMessageRequest并设置MessageAttributes
                var response = topic.PublishMessage(body);
                return response.MessageId;
                //   Console.WriteLine("PublishMessage succeed! " + response.MessageId);
            }
            catch (MNSException me)
            {
                return me.Message;
                // 3. 可能因为网络错误等原因导致PublishMessage失败，这里CatchException并做对应处理
                //   Console.WriteLine("PublishMessage Failed! ErrorCode: " + me.ErrorCode);
            }
            catch (Exception ex)
            {
                return ex.Message;
                //  Console.WriteLine("PublishMessage failed, exception info: " + ex.Message);
            }

        }
    }
}

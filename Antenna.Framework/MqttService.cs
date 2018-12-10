using Antenna.Model;
using Antenna.MQTT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Antenna.Framework
{
    //public delegate void PublicationReceivedHandler(string topic,int? mtp, string ctt);
    /// <summary>
    /// MQTT服务类
    /// </summary>
    /// 创建者：赵雪峰 20160912
    public class MqttService
    {
        #region 单例模式（线程安全）
        private volatile static MqttService _instance = null;
        private static readonly object lockObj = new object();
        private MqttService() { }
        public static MqttService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockObj)
                    {
                        if (_instance == null)
                            _instance = new MqttService();
                    }
                }
                return _instance;
            }
        }
        #endregion
        public delegate void PublicationReceivedHandler(string topic, string jsonContent);
        public event PublicationReceivedHandler MessageReceived;

        public event EventHandler ReconnectedMqtt;
        //Timer pingTimer;
        public short KeepAlive = 60;
        MQTTClient Client;
        /// <summary>
        /// 连接MQTT服务
        /// </summary>
        /// <param name="token">对应MQTT的客户端ID</param>
        /// <returns></returns>
        public bool Connect(ref string errMsg)
        {
            try
            {
                Client = new MQTTClient(GlobalVariable.ConfigEntity.MQTT_Host, int.Parse(GlobalVariable.ConfigEntity.MQTT_Port));
                if (Client.Connect(GlobalVariable.LoginOutput.userId + "/" + (int)GlobalVariable.OSType.PC, GlobalVariable.ConfigEntity.MQTT_UserName, GlobalVariable.ConfigEntity.MQTT_Password))
                {
                    #region 发心跳消息
                    //Heartbeat heartbeat = new Heartbeat();
                    //heartbeat.ctt.token = token;
                    //pingTimer = new Timer((state) =>
                    //{

                    //    Publish<Heartbeat>("heartbeat", heartbeat);
                    //    //Client.Publish("heartbeat", byteArray, Qos.AtLeastOnce);
                    //}, null, 0, KeepAlive * 1000);
                    #endregion
                    Client.MessageReceived += OnMessageReceived;
                    Client.ReconnectedMqtt += OnReconnectedMqtt;
                    LogHelper.WriteDebug("MQTT连接成功:" + GlobalVariable.ConfigEntity.MQTT_Host + ":" + int.Parse(GlobalVariable.ConfigEntity.MQTT_Port));
                    return true;
                }
                else
                {
                    errMsg = "消息服务器连接异常";
                    LogHelper.WriteDebug(string.Format("MQTT连接失败:[{0}:{1}] {2}", GlobalVariable.ConfigEntity.MQTT_Host, GlobalVariable.ConfigEntity.MQTT_Port, errMsg));
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = "消息服务器连接异常";
                LogHelper.WriteDebug(string.Format("MQTT连接失败:[{0}:{1}] {2}", GlobalVariable.ConfigEntity.MQTT_Host, GlobalVariable.ConfigEntity.MQTT_Port, errMsg));
                return false;
            }
        }

        public bool IsConnected
        {
            get
            {
                if (Client == null)
                {
                    return false;
                }
                else
                {
                    return Client.IsConnected;
                }
            }
        }
        private void OnReconnectedMqtt(object sender, EventArgs e)
        {
            if (ReconnectedMqtt != null)
                ReconnectedMqtt(null, null);
        }
        private void OnMessageReceived(string topic, byte[] payload)
        {
            if (MessageReceived != null)
            {
                byte[] decompressPayload = DataConverter.GzipDecompress(payload);
                string finalJson = DataConverter.AESDecrypt(System.Text.Encoding.UTF8.GetString(decompressPayload), GlobalVariable.AES_Key, GlobalVariable.AES_IV);
                //string mtpStr = string.Empty;        
                //string ctt = string.Empty;
                //string errMsg = string.Empty;
                //DataConverter.GetValueByJsonKey("mtp", finalJson, ref mtpStr, ref errMsg);
                //DataConverter.GetValueByJsonKey("ctt", finalJson, ref ctt, ref errMsg);
                //int mtp;
                //if(int.TryParse(mtpStr,out mtp ))
                //{
                //    MessageReceived(topic, mtp, ctt);
                //}
                //else
                //{
                //    MessageReceived(topic, null, ctt);
                //}
                LogHelper.WriteDebug(string.Format("收到MQTT消息：topic={0},content={1}", topic, finalJson));
                if (MessageReceived != null)
                    MessageReceived(topic, finalJson);

            }
            //DisplayInfo("topic=" + topic + ",content=" + content);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            try
            {
                #region 先取消订阅
                List<string> topics = new List<string>();
                if (Client.Subscriptions.Count > 0)
                {
                    foreach (Subscription subscription in Client.Subscriptions)
                    {
                        topics.Add(subscription.TopicName);
                    }
                    string errMsg = string.Empty;
                    UnSubscribe(topics, ref errMsg);
                }
                #endregion
                Client.MessageReceived -= OnMessageReceived;
                Client.ReconnectedMqtt -= OnReconnectedMqtt;
                Client.Disconnect();
                Client.Dispose();
                LogHelper.WriteDebug("MQTT连接断开");
            }
            catch (Exception e)
            {
                LogHelper.WriteError("MQTT连接断开失败"+e.Message);
            }
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool Subscribe(List<string> topics, ref string errMsg)
        {
            if (!Client.IsConnected)
            {
                errMsg = "消息服务器连接异常。";
                return false;
            }
            try
            {
                Dictionary<string, QoS> subscriptions = new Dictionary<string, QoS>();
                foreach (string topic in topics)
                {
                    subscriptions.Add(topic, QoS.AcknowledgeDelivery);
                }
                Client.Subscriptions.Add(subscriptions);
                LogHelper.WriteDebug("MQTT主题订阅成功：topics=" + string.Join(",", topics.ToArray()));
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                LogHelper.WriteDebug("MQTT主题订阅失败：" + errMsg);
                return false;
            }
            //subscriptions.Add("10086", QoS.AcknowledgeDelivery); //用户信息相关，组织架构更新(公司代码,暂时写死用10086)
            //subscriptions.Add(userId, QoS.AcknowledgeDelivery);//讨论组新增
            //subscriptions.Add(groupId, QoS.AcknowledgeDelivery)//订阅讨论组ID（讨论组删除，讨论组成员更新，讨论组基本信息更新）
            //subscriptions.Add("message_ack", QoS.AcknowledgeDelivery);//消息回执格式
            //subscriptions.Add(token, QoS.AcknowledgeDelivery);//踢出用户登录
            //subscriptions.Add("zxfTopic111", QoS.AcknowledgeDelivery);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topics"></param>
        /// <returns></returns>
        public bool UnSubscribe(List<string> topics, ref string errMsg)
        {
            if (!Client.IsConnected)
            {
                errMsg = "消息服务器连接异常。";
                return false;
            }
            try
            {
                //unSubscriptions.Add("zxfTopic111");
                Client.Subscriptions.Remove(topics);
                LogHelper.WriteDebug("MQTT取消订阅成功：topics=" + string.Join(",", topics.ToArray()));
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                LogHelper.WriteDebug("MQTT取消订阅失败：" + errMsg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 发布消息(实体作为参数)
        /// </summary>
        /// <typeparam name="T">泛型实体</typeparam>
        /// <param name="topic">主题</param>
        /// <param name="entity">内容实体</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>发布是否成功</returns>
        public bool Publish<T>(string topic, T entity, ref string errMsg,NullValueHandling nullValueHandling = NullValueHandling.Include)
        {
            string json = string.Empty;
            if (!DataConverter.SerializeJson(entity, ref json, ref errMsg, nullValueHandling))
            {
                return false;
            }
            return Publish(topic, json, ref errMsg);
        }

        /// <summary>
        /// 发布消息(JSON字符串作为参数)
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="jsonContent">JSON字符串</param>
        /// <param name="errMsg">错误提示</param>
        /// <returns>发布是否成功</returns>
        private bool Publish(string topic, string jsonContent, ref string errMsg)
        {
            if (!Client.IsConnected)
            {
                errMsg = "消息服务器连接异常。";
                LogHelper.WriteError("[MqttService_Publish]" + errMsg);
                return false;
            }
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                string finalJson = DataConverter.AESEncrypt(jsonContent, GlobalVariable.AES_Key, GlobalVariable.AES_IV);
                byte[] gzipCompressJson = DataConverter.GzipCompress(System.Text.Encoding.UTF8.GetBytes(finalJson));
                Client.Publish(topic, gzipCompressJson, QoS.AcknowledgeDelivery, false);
                stopWatch.Stop();
                if (topic != "heartbeat")//屏蔽掉发心跳的日志
                {
                    LogHelper.WriteDebug(string.Format("发布MQTT消息成功({0}毫秒)：topic={1},content={2}", stopWatch.Elapsed.TotalMilliseconds, topic, jsonContent));
                }
                return true;
                //Client.Publish("zxfTopic111", "ZXFTOPIC111", QoS.AcknowledgeDelivery, false);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                LogHelper.WriteError("发布MQTT消息失败：" + errMsg + "," + ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 发布消息(实体作为参数)
        /// </summary>
        /// <typeparam name="T">泛型实体</typeparam>
        /// <param name="topic">主题</param>
        /// <param name="entity">内容实体</param>
        /// <param name="errMsg">错误信息</param>
        /// <returns>发布是否成功</returns>
        //private bool Publish<T>(string topic, T entity)
        //{
        //    string errMsg = string.Empty;
        //    string json = string.Empty;
        //    if (!DataConverter.SerializeJson(entity, ref json, ref errMsg))
        //    {
        //        return false;
        //    }
        //    return Publish(topic, json, ref errMsg);
        //}
    }
}

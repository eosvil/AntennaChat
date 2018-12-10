﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SDK.Service.Mqtt
{
    internal class PublishHeaderData : HeaderData
    {
        private MQTTString m_topic;

        public MQTTString TopicName 
        {
            get { return m_topic; }
            set
            {
                m_topic = Uri.EscapeUriString(value);
            }
        }
        public ushort? MessageID { get; set; }

        public override byte[] Serialize()
        {
            var data = new List<byte>(TopicName.Value.Length);
            data.AddRange(TopicName.Serialize());

            if (MessageID.HasValue)
            {
                //Modify by 赵雪峰 20160909
                //data.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(MessageID.Value)));
                data.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)MessageID.Value)));
           }

            return data.ToArray();
        }
    }
}
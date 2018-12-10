﻿using OpenNETCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDK.Service.Mqtt
{
    internal class Subscription
    {
        public string TopicName { get; private set; } = string.Empty;
        public QoS QoS { get; private set; }

        public Subscription(string topic)
            : this(topic, QoS.FireAndForget)
        {
        }

        internal Subscription()
        {
        }

        public Subscription(string topic, QoS qos)
        {
            Validate
                .Begin()
                .IsNotNullOrEmpty(topic)
                .Check();

            TopicName = topic;
            QoS = qos;
        }

        internal byte[] Serialize()
        {
            //var name = TopicName.Replace("+", "%2B").Replace("#", "%23");
            var name = TopicName;
            var data = new List<byte>(name.Length + 3);

            data.AddRange(((MQTTString)name).Serialize());
            data.Add((byte)QoS);

            return data.ToArray();
        }
    }
}
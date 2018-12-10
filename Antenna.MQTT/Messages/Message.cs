﻿using OpenNETCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Antenna.MQTT
{
    internal abstract class Message
    {
        internal FixedHeader FixedHeader { get; private set; }        
        public IVariableHeader VariableHeader { get; set; }

        internal Message(FixedHeader header)
        {
            Validate
                .Begin()
                .IsNotNull(header)
                .Check();

            FixedHeader = header;
        }

        public Message(MessageType type)
        {
            FixedHeader = new FixedHeader();

            FixedHeader.MessageType = type;
        }

        public Message(MessageType type, QoS qos, bool retain, bool duplicateDelivery)
        {
            FixedHeader = new FixedHeader();

            FixedHeader.MessageType = type;
            FixedHeader.QoS = qos;
            FixedHeader.Retain = retain;
            FixedHeader.DuplicateDelivery = duplicateDelivery;
        }

        public QoS QoS
        {
            get { return FixedHeader.QoS; }
        }

        public byte[] Serialize()
        {
            var data = new List<byte>();
            
            var payload = this.Payload;
            var length = payload == null ? 0 : payload.Length;

            byte[] variableHeader = null;
            if (VariableHeader != null)
            {
                variableHeader = VariableHeader.Serialize();
                length += variableHeader.Length;
            }

            FixedHeader.RemainingLength = length;

            data.AddRange(FixedHeader.Serialize());
            if(variableHeader != null)
            {
                data.AddRange(variableHeader);
            }

            if (payload != null)
            {
                data.AddRange(payload);
            }

            return data.ToArray();
        }

        public abstract byte[] Payload { get; }
    }
}

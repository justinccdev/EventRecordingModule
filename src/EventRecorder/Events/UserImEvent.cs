/*
 * Copyright (c) Contributors
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using OpenMetaverse;

namespace EventRecorder
{
    public class UserImEvent : UserEvent
    {
        public UUID ReceiverId { get; set; }
        public string ReceiverName { get; set; }
        public bool IsReceiverGroup { get; set; }
        public string Text { get; set; }

        public UserImEvent() : base() {}

        public UserImEvent(
            UUID userId, string userName, UUID receiverId, string receiverName, bool isReceiverGroup, 
            string text, string gridId, string regionName)
            : this(userId, userName, receiverId, receiverName, isReceiverGroup, text, gridId, regionName, DateTime.Now)
        {}

        public UserImEvent(
            UUID userId, string userName, UUID receiverId, string receiverName, bool isReceiverGroup, 
            string text, string gridId, string regionName, DateTime datetime)
            : base(userId, userName, "im", gridId, regionName, datetime)
        {
            ReceiverId = receiverId;
            ReceiverName = receiverName;
            IsReceiverGroup = isReceiverGroup;
            Text = text;
        }

        public override bool Record(IRecorder recorder)
        {
            return recorder.RecordEvent(this);
        }
    }
}
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
    public abstract class UserEvent : IEvent
    {
        public UUID UserId { get; set; }

        public string UserName 
        { 
            get { return m_userName; }
            set 
            { 
                if (value.Length > EventRecordingModuleConstants.UserNameMaxSize)
                    value = value.Substring(0, EventRecordingModuleConstants.UserNameMaxSize);

                m_userName = value;
            }
        }
        private string m_userName;

        public string EventType { get; set; }

        public string RegionName 
        { 
            get { return m_regionName; }
            set 
            { 
                if (value.Length > EventRecordingModuleConstants.RegionNameMaxSize)
                    value = value.Substring(0, EventRecordingModuleConstants.RegionNameMaxSize);

                m_regionName = value;
            }
        }
        private string m_regionName;

        public DateTime DateTime { get; set; }
        public string GridId { get; set; }

        public UserEvent() {}

        public UserEvent(UUID userId, string userName, string eventType, string gridId, string regionName, DateTime datetime)
        {
            UserId = userId;
            UserName = userName;
            EventType = eventType;
            GridId = gridId;
            RegionName = regionName;
            DateTime = datetime;
        }

        public abstract bool Record(IRecorder recorder);
                
        public override string ToString()
        {
            return string.Format("{0} for {1} {2}", "IM", UserName, UserId);
        }
    }   
}
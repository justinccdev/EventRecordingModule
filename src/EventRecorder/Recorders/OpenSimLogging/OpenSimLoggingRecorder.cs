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
using System.Reflection;
using log4net;
using Nini.Config;
using OpenSim.Region.Framework.Scenes;

namespace EventRecorder
{
    /// <summary>
    /// This targets records events to OpenSimulator's internal log file
    /// </summary>
    public class OpenSimLoggingRecorder : IRecorder
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name { get { return "OpenSimLog"; } }

        public bool IsRunning { get; private set; }

        public void Initialise(IConfigSource configSource) {}

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public bool RecordEvent(UserChatEvent ev)
        {
            m_log.DebugFormat(
                "[EVENT RECORDER]: Notified of avatar {0} {1} chat {2} {3} \"{4}\" from {5} in {6}",
                ev.UserName, ev.UserId, ev.ChatType, ev.Channel, ev.Text, ev.Origin, ev.RegionName);

            return true;
        }

        public bool RecordEvent(UserImEvent ev)
        {
            m_log.DebugFormat(
                "[EVENT RECORDER]: Notified of avatar {0} {1} IM to {2} {3} {4} \"{5}\" in {6}",
                ev.UserName, ev.UserId, ev.IsReceiverGroup ? "group" : "user", ev.ReceiverId, ev.ReceiverName, ev.RegionName);

            return true;
        }

        public bool RecordEvent(UserRegionEvent ev)
        {
            m_log.DebugFormat(
                "[EVENT RECORDER]: Notified of avatar {0} {1} {2} event in scene {3}", 
                ev.UserName, ev.UserId, ev.EventType, ev.RegionName);

            return true;
        }
    }
}
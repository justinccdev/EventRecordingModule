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
using Nini.Config;
using OpenSim.Region.Framework.Scenes;

namespace EventRecorder
{
    public interface IRecorder
    {
        /// <summary>
        /// Name of this recorder
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Signals whether the recorder is running.
        /// </summary>
        /// <remarks>Descendents should signal this on start and stop as appropriate.</remarks>
        bool IsRunning { get; }

        /// <summary>
        /// Initialise the recorder
        /// </summary>
        /// <param name="configSource">Configuration parameters.</param>
        void Initialise(IConfigSource configSource);

        /// <summary>
        /// Start the recorder.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the recorder.
        /// </summary>
        void Stop();

        /// <summary>
        /// Record a user region event.
        /// </summary>
        /// <param name="ev"></param>
        bool RecordEvent(UserChatEvent ev);

        /// <summary>
        /// Record a user chat event.
        /// </summary>
        /// <param name="ev"></param>
        bool RecordEvent(UserRegionEvent ev);

        /// <summary>
        /// Record a user instant message event.
        /// </summary>
        /// <remarks>
        /// This includes instant messages to both other users and to groups.
        /// </remarks>
        /// <param name="ev"></param>
        bool RecordEvent(UserImEvent ev);
    }
}
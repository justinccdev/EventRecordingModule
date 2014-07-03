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
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using log4net;
using OpenSim.Framework;
using OpenSim.Framework.Monitoring;
using OpenSim.Region.Framework.Scenes;

namespace EventRecorder
{
    public class QueueingRecorder : IRecorder
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public bool IsRunning { get; private set; }

        /// <summary>
        /// The decorated recorder (http://en.wikipedia.org/wiki/Decorator_pattern) which we'll use to write events
        /// that we pull off the queue.
        /// </summary>
        private IRecorder m_decoratedRecorder;

        /// <summary>
        /// Events are queued here before they are finally written.
        /// </summary>
        private BlockingCollection<UserRegionEvent> m_eventWriteQueue 
            = new BlockingCollection<UserRegionEvent>(new ConcurrentQueue<UserRegionEvent>());

        private CancellationTokenSource m_cancelSource = new CancellationTokenSource();

        /// <summary>
        /// Used to signal that we are ready to complete close.
        /// </summary>
        private ManualResetEvent m_finishedWritingAfterStop = new ManualResetEvent(false);

        public QueueingRecorder(IRecorder decoratedRecorder)
        {
            m_decoratedRecorder = decoratedRecorder;
        }

        public void Start()
        {
            lock (this)
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                m_finishedWritingAfterStop.Reset();

                Watchdog.StartThread(RecordEventsFromQueue, "EventRecorder", ThreadPriority.Lowest, true, false);               
            }
        }

        private void RecordEventsFromQueue()
        {
            try
            {
                while (IsRunning || m_eventWriteQueue.Count > 0)
                {
                    UserRegionEvent ev = m_eventWriteQueue.Take(m_cancelSource.Token);
//                    Console.WriteLine("Sleeping");
//                    Thread.Sleep(30000);
//                    Console.WriteLine("Finished Sleeping");

                    RecordUserRegionEventFromQueue(ev);
                }
            }
            catch (OperationCanceledException)
            {
            }

            m_finishedWritingAfterStop.Set();
        }

        private void RecordUserRegionEventFromQueue(UserRegionEvent ev)
        {
            m_decoratedRecorder.RecordUserRegionEvent(ev);
        }

        public bool RecordUserRegionEvent(UserRegionEvent ev)
        {
            m_eventWriteQueue.Add(ev);

            return true;
        }

        public void Stop()
        {
            lock (this)
            {
                try
                {
                    if (!IsRunning)
                        return;

                    IsRunning = false;

                    // The code below has to cover four scenarios regarding the event recording loop
                    // Scenario 1: loop is waiting for an event with nothing on the queue - need to cancel the wait
                    // Scenario 2: loop is processing events - need to signal exit and wait for processing to complete

                    int eventsLeft = m_eventWriteQueue.Count;
//                    Console.WriteLine("eventsLeft {0}", eventsLeft);
                    if (eventsLeft <= 0)
                        m_cancelSource.Cancel();
                    else 
                        m_log.InfoFormat("[EVENT RECORDER]: Waiting to write {0} events after stop.", eventsLeft);

                    m_finishedWritingAfterStop.WaitOne();
                }
                finally
                {
                    m_cancelSource.Dispose();
                }
            }
        }
    }
}
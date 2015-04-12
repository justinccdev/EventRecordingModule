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
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Framework.Monitoring;
using OpenSim.Region.Framework.Scenes;

namespace EventRecorder
{
    public class QueueingRecorder : IRecorder
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Name { get { return string.Format("Queueing -> {0}", m_decoratedRecorder.Name); } }

        public bool IsRunning { get; private set; }       

        /// <summary>
        /// Number of events waiting in queue.
        /// </summary>
        public int Count { get { return m_eventWriteQueue.Count; } }

        /// <summary>
        /// Maximum event capacity.
        /// </summary>
        public int Capacity { get { return m_eventWriteQueue.BoundedCapacity; } }

        /// <summary>
        /// The timeout in milliseconds to wait for at least one event to be written when the recorder is stopping.
        /// </summary>
        public int EventWriteTimeoutOnStop { get; set; }

        /// <summary>
        /// The decorated recorder (http://en.wikipedia.org/wiki/Decorator_pattern) which we'll use to write events
        /// that we pull off the queue.
        /// </summary>
        private IRecorder m_decoratedRecorder;

        /// <summary>
        /// Events are queued here before they are finally written.
        /// </summary>
        private BlockingCollection<IEvent> m_eventWriteQueue;

        /// <summary>
        /// Controls whether we need to warn in the log about exceeding the max queue size.
        /// </summary>
        /// <remarks>
        /// This is flipped to false once queue max has been exceeded and back to true when it falls below max, in 
        /// order to avoid spamming the log with lots of warnings.
        /// </remarks>
        private bool m_warnOverMaxQueue = true;              

        /// <summary>
        /// Used to cancel the event writing thread on shutdown if necessary.
        /// </summary>
        private CancellationTokenSource m_cancelSource = new CancellationTokenSource();

        /// <summary>
        /// Used to signal that we are ready to complete close.
        /// </summary>
        private ManualResetEvent m_finishedWritingAfterStop = new ManualResetEvent(false);

        public QueueingRecorder(IRecorder decoratedRecorder)
        {
            m_decoratedRecorder = decoratedRecorder; 
        }

        public void Initialise(IConfigSource configSource) 
        {
            IConfig config = configSource.Configs["EventRecorder"];

            m_decoratedRecorder.Initialise(configSource);
                        
            int maxQueueSize = config.GetInt("MaxEventQueueSize", 5000);

            if (maxQueueSize <= 0)
                throw new Exception(string.Format("MaxEventQueueSize must be > 0.  Value {0} is invalid", maxQueueSize));
            else
                m_log.DebugFormat("[EVENT RECORDER]: Using MaxEventQueueSize of {0}", maxQueueSize);

            EventWriteTimeoutOnStop = config.GetInt("EventWriteTimeoutOnStop", 20000);

            if (EventWriteTimeoutOnStop < 0)
                throw new Exception(
                    string.Format("EventWriteTimeoutOnStop must be >= 0.  Value {0} ms is invalid", EventWriteTimeoutOnStop));
            else
                m_log.DebugFormat("[EVENT RECORDER]: Using EventWriteTimeoutOnStop of {0} ms", EventWriteTimeoutOnStop);

            m_eventWriteQueue 
                = new BlockingCollection<IEvent>(new ConcurrentQueue<IEvent>(), maxQueueSize);
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
//                    Console.WriteLine("Sleeping");
//                    Thread.Sleep(15000);
                    IEvent ev = m_eventWriteQueue.Take(m_cancelSource.Token);
//                    Console.WriteLine("Finished Sleeping");

                    RecordEventFromQueue(ev);
                }
            }
            catch (OperationCanceledException)
            {
            }

            m_finishedWritingAfterStop.Set();
        }

        private void RecordEventFromQueue(IEvent ev)
        {
            ev.Record(m_decoratedRecorder);
        }

        public bool RecordEvent(UserChatEvent ev)
        {
            return RecordEventInternal(ev);
        }

        public bool RecordEvent(UserRegionEvent ev)
        {
            return RecordEventInternal(ev);
        }

        public bool RecordEvent(UserImEvent ev)
        {
            return RecordEventInternal(ev);
        }

        private bool RecordEventInternal(IEvent ev)
        {
            // We need to lock here to avoid a situation where two threads could simultaneous attempt to record an
            // event and both pass the size check before writing.
            lock (m_eventWriteQueue)
            {
                if (m_eventWriteQueue.Count < m_eventWriteQueue.BoundedCapacity)
                {
                    m_eventWriteQueue.Add(ev);
                    m_warnOverMaxQueue = true;
                    return true;
                }
                else
                {
                    if (m_warnOverMaxQueue)
                    {
                        m_log.WarnFormat(
                            "[EVENT RECORDER]: Event Queue at maximum capacity, not recording event {0}", ev);

                        m_warnOverMaxQueue = false;
                    }

                    return false;
                }
            }
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

                    m_log.InfoFormat("[EVENT RECORDER]: Waiting to write {0} events after stop.", eventsLeft);

                    bool isFinished = false;

                    while (!isFinished)
                    {
                        if (m_finishedWritingAfterStop.WaitOne(EventWriteTimeoutOnStop))
                        {
                            isFinished = true;
                        }
                        else
                        {
                            // After timeout no events have been written
                            if (eventsLeft == m_eventWriteQueue.Count)
                            {
                                m_log.WarnFormat(
                                    "[EVENT RECORDER]: No events written after {0} ms wait.  Discarding remaining {1} events", 
                                    EventWriteTimeoutOnStop, eventsLeft);

                                isFinished = true;
                            }
                            else
                            {
                                eventsLeft = m_eventWriteQueue.Count;
                            }
                        }
                    }
                }
                finally
                {
                    m_cancelSource.Dispose();
                }
            }
        }
    }
}
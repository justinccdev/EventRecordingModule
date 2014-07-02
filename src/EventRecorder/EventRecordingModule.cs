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
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Mono.Addins;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

[assembly: Addin("EventRecordingModule", "0.1")]
[assembly: AddinDependency("OpenSim", "0.5")]

namespace EventRecorder
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "EventRecordingModule")]
    public class EventRecordingModule : ISharedRegionModule
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);                
        
        public string Name { get { return "Event Recording Module"; } }        
        
        public Type ReplaceableInterface { get { return null; } }

        /// <summary>
        /// Is this module enabled?
        /// </summary>
        public bool Enabled { get; private set; }       

        /// <summary>
        /// Indicates whether the simulator has finished adding its initial scenes to the module.
        /// </summary>
        /// <remarks>
        /// This only exists to help handle a bug in OpenSimulator 0.8 where Close() is not called on simulator
        /// shutdown.
        /// </remarks>
        public bool FinishedAddingInitialScenes { get; private set; }

        /// <summary>
        /// Track the number of scenes being monitored.
        /// </summary>
        /// <remarks>
        /// This only exists to help handle a bug in OpenSimulator 0.8 where Close() is not called on simulator
        /// shutdown.
        public int NumberOfScenesMonitored { get; private set; }

        private IRecorder m_recorder;
        
        public void Initialise(IConfigSource configSource)
        {
//            m_log.DebugFormat("[EVENT RECORDER]: INITIALIZED MODULE");

            HashSet<string> recorders = new HashSet<string>{ "OpenSimLog", "MySQL" };

            IConfig config = configSource.Configs["EventRecorder"];
            if (config == null)
            {
                m_log.DebugFormat("[EVENT RECORDER]: No [EventRecorder] config section found");
                return;
            }

            Enabled = config.GetBoolean("Enabled", Enabled);

            if (!Enabled)
                return;

            string recorder = config.GetString("Recorder");

            if (recorder == null)
                throw new Exception(
                    string.Format("No Recorder parameter found in [EventRecorder] config.  Must be one of {0}", 
                        string.Join(", ", recorders)));

            if (!recorders.Contains(recorder))
                throw new Exception(
                    string.Format("Recorder '{0}' is invalid.  Must be one of {1}", string.Join(", ", recorders)));

            if (recorder == "OpenSimLog")
                m_recorder = new QueueingRecorder(new OpenSimLoggingRecorder());
            else if (recorder == "MySQL")
                m_recorder = new QueueingRecorder(new MySQLRecorder(configSource));

            SceneManager.Instance.OnRegionsReadyStatusChange += HandleRegionsReadyStatusChange;
            Enabled = true;

            m_recorder.Start();
        }
        
        public void PostInitialise()
        {
//            m_log.DebugFormat("[EVENT RECORDER]: POST INITIALIZED MODULE");
        }
        
        public void Close()
        {
//            m_log.DebugFormat("[EVENT RECORDER]: CLOSED MODULE");
            // DUE TO A BUG IN OPENSIMULATOR 0.8 AND BEFORE, THIS IS NOT BEING CALLED ON REGION SHUTDOWN
        }
        
        public void AddRegion(Scene scene)
        {
//            Console.WriteLine("[EVENT RECORDER]: ADD REGION {0}", scene.Name);

            if (!Enabled)
                return;

            if (FinishedAddingInitialScenes)
            {
                m_log.WarnFormat(
                    "[EVENT RECORDER]: Cannot currently dynamic add scenes to event recorder.  Please restart the simulator to add scene {0}", 
                    scene.Name);

                return;
            }

            NumberOfScenesMonitored++;

            scene.EventManager.OnMakeRootAgent += HandleOnMakeRootAgent;

//            scene.EventManager.OnMakeChildAgent 
//                += p 
//                    => m_log.DebugFormat(
//                        "[EVENT RECORDER]: Notified of avatar {0} moving away from scene {1}", p.UUID, p.Scene.Name);

//            scene.EventManager.OnClientClosed
//                += (agentID, s)
//                    => m_log.DebugFormat(
//                        "[EVENT RECORDER]: Notified of avatar {0} logging out of scene {1}", agentID, s.Name);

            scene.EventManager.OnClientClosed += HandleOnClientClosed;           
        }

        private void HandleRegionsReadyStatusChange(SceneManager sm)
        {
            // XXX: This is horrific, but the only way to tell whether we are shutting down at the moment is both to 
            // scene count and wait for OpenSimulator to signal that all scene are ready after initial startup.
            // But this stops this module working properly with scene-less simulators and those that add regions afterwards
            // This will be fixed in the next release of OpenSimulator (post 0.8) when Close() will be called on shutdown.
            if (sm.AllRegionsReady)
            {
                sm.OnRegionsReadyStatusChange -= HandleRegionsReadyStatusChange;
                FinishedAddingInitialScenes = true;
            }
        }

        private void HandleOnClientClosed(UUID agentID, Scene s)
        {           
            ScenePresence sp = s.GetScenePresence(agentID);

            if (sp == null)
            {
                m_log.WarnFormat(
                    "[EVENT RECORDER]: Received event that agent {0} had closed in {1} but no scene presence found", 
                    agentID, s.Name);
            }

            if (!sp.IsChildAgent)
                m_recorder.RecordUserRegionEvent(new UserRegionEvent(agentID, sp.Name, "logout", s.Name));
        }

        private void HandleOnMakeRootAgent(ScenePresence sp)
        {
            if ((sp.TeleportFlags & Constants.TeleportFlags.ViaLogin) != 0)
                m_recorder.RecordUserRegionEvent(new UserRegionEvent(sp.UUID, sp.Name, "login", sp.Scene.Name));
            else
                m_recorder.RecordUserRegionEvent(new UserRegionEvent(sp.UUID, sp.Name, "enter", sp.Scene.Name));
        }       
        
        public void RemoveRegion(Scene scene)
        {
//            m_log.DebugFormat("[EVENT RECORDER]: REGION {0} REMOVED", scene.RegionInfo.RegionName);

            scene.EventManager.OnMakeRootAgent -= HandleOnMakeRootAgent;
            scene.EventManager.OnClientClosed -= HandleOnClientClosed;

            if (--NumberOfScenesMonitored <= 0)
            {
                m_recorder.Stop();
                m_log.DebugFormat("[EVENT RECORDER]: Stopped.");
            }
        }        
        
        public void RegionLoaded(Scene scene)
        {
//            m_log.DebugFormat("[EVENT RECORDER]: REGION {0} LOADED", scene.RegionInfo.RegionName);
        }                
    }
}
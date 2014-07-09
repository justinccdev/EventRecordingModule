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
using OpenSim.Framework.Console;
using OpenSim.Framework.Monitoring;
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
        /// Sets whether we are recording user to user instant messages.
        /// </summary>
        /// <value><c>true</c> if record user im to user events; otherwise, <c>false</c>.</value>
        public bool RecordUserToUserImEvents { get; set; }

        /// <summary>
        /// Track the number of scenes being monitored.
        /// </summary>
        /// <remarks>
        /// This only exists to help handle a bug in OpenSimulator 0.8 where Close() is not called on simulator
        /// shutdown.
        public int RegionsMonitoredCount { get; private set; }

        /// <summary>
        /// This only exists so that we know when all regions have initially loaded.  Dumb, eh?
        /// </summary>
        public int RegionsLoadedCount { get; private set; }

        /// <summary>
        /// Used to record events.
        /// </summary>
        private QueueingRecorder m_recorder;

        /// <summary>
        /// Used to resolve UUID -> user name.
        /// </summary>
        private IUserManagement m_userManagementModule;

        /// <summary>
        /// Used to resolve UUID -> group name.
        /// </summary>
        private IGroupsModule m_groupsModule;

        private const int GridIdMaxSize = 36;
        private string m_gridId;
        
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

            bool enabledInConfig = config.GetBoolean("Enabled", false);

            if (!enabledInConfig)
                return;           

            // This is not necessary for all recorders but it's cleaner to enforce upfront than potentially end up
            // with a migration problem later.
            if (!config.Contains("GridID"))
                throw new Exception("A GridID must bs specified in the [EventRecorder] config section.");

            m_gridId = config.GetString("GridID");

            if (m_gridId.Length > GridIdMaxSize)
                throw new Exception(
                    string.Format(
                        "GridId [{0}] at {1} chars is longer than the maximum {2} chars", 
                        m_gridId, m_gridId.Length, GridIdMaxSize));

            m_log.DebugFormat("[EVENT RECORDER]: GridId set to {0}", m_gridId);

            string recorderName = config.GetString("Recorder");           

            IRecorder decoratedRecorder;

            if (recorderName == "OpenSimLog")
                decoratedRecorder = new OpenSimLoggingRecorder();
            else if (recorderName == "MySQL")
                decoratedRecorder = new MySQLRecorder();
            else if (recorderName == null)
                throw new Exception(
                    string.Format("No Recorder parameter found in [EventRecorder] config.  Must be one of {0}", 
                              string.Join(", ", recorders)));
            else
                throw new Exception(
                    string.Format(
                        "Recorder '{0}' is not a valid recorder name.  Must be one of {1}", 
                        recorderName, string.Join(", ", recorders)));

            m_recorder = new QueueingRecorder(decoratedRecorder);
            m_recorder.Initialise(configSource);

            RecordUserToUserImEvents = config.GetBoolean("RecordUserToUserImEvents", RecordUserToUserImEvents);
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

            RegionsMonitoredCount++;

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

            //scene.EventManager.OnChatToClients += HandleOnChatToClients;
        }

        private void HandleInfoCommand(string module, string[] args)
        {
            ConsoleDisplayList cdl = new ConsoleDisplayList();
            cdl.AddRow("Recorder", m_recorder.Name);
            cdl.AddRow("Grid ID", m_gridId);
            cdl.AddRow("RecordUserToUserImEvents", RecordUserToUserImEvents);
            cdl.AddRow("Events queue capacity", m_recorder.Capacity);
            cdl.AddRow("Events queued", m_recorder.Count);

            MainConsole.Instance.Output(cdl.ToString());
        }

        private void HandleOnClientClosed(UUID agentId, Scene s)
        {                  
            if (!Enabled)
                return;

            ScenePresence sp = s.GetScenePresence(agentId);

            if (sp == null)
            {
                m_log.WarnFormat(
                    "[EVENT RECORDER]: Received event that agent {0} had closed in {1} but no scene presence found", 
                    agentId, s.Name);
            }

            if (!sp.IsChildAgent)
                m_recorder.RecordEvent(new UserRegionEvent(agentId, sp.Name, "logout", m_gridId, s.Name));
        }

        private void HandleOnMakeRootAgent(ScenePresence sp)
        {            
            if (!Enabled)
                return;

            if ((sp.TeleportFlags & Constants.TeleportFlags.ViaLogin) != 0)
                m_recorder.RecordEvent(new UserRegionEvent(sp.UUID, sp.Name, "login", m_gridId, sp.Scene.Name));
            else
                m_recorder.RecordEvent(new UserRegionEvent(sp.UUID, sp.Name, "enter", m_gridId, sp.Scene.Name));

            sp.ControllingClient.OnChatFromClient += HandleOnChatFromClient;
            sp.ControllingClient.OnInstantMessage += HandleOnInstantMessage;
        }

        private void HandleOnChatFromClient(object sender, OSChatMessage e)
        {
            IClientAPI client = (IClientAPI)sender;

            if (ChatTypeEnum.Whisper != e.Type && ChatTypeEnum.Say != e.Type && ChatTypeEnum.Shout != e.Type)
                return;

            m_recorder.RecordEvent( 
                new UserChatEvent(
                    client.AgentId, client.Name, e.Position, e.Type, e.Message, e.Channel, m_gridId, client.Scene.Name));
        }

        private void HandleOnInstantMessage(IClientAPI client, GridInstantMessage msg)
        {
//            m_log.DebugFormat(
//                "[EVENT RECORDER]: Handling IM from {0} {1}, type {2}", 
//                client.Name, client.AgentId, (InstantMessageDialog)msg.dialog);

            if (msg.dialog != (byte)InstantMessageDialog.MessageFromAgent 
                && msg.dialog != (byte)InstantMessageDialog.SessionSend)
                return;           

            bool toGroup = msg.dialog == (byte)InstantMessageDialog.SessionSend;

            if (!toGroup && !RecordUserToUserImEvents)
                return;

            string receiverName = "UNKNOWN";
            UUID receiverId;

            if (toGroup)
            {
                receiverId = new UUID(msg.imSessionID);

                if (m_groupsModule != null)
                {
                    GroupRecord gr = m_groupsModule.GetGroupRecord(receiverId);

                    if (gr != null)
                        receiverName = gr.GroupName;
                }
            }
            else
            {
                receiverId = new UUID(msg.fromAgentID);
                receiverName = m_userManagementModule.GetUserName(new UUID(msg.toAgentID));
            }

            m_recorder.RecordEvent(
                new UserImEvent(
                    new UUID(msg.fromAgentID), msg.fromAgentName, receiverId, receiverName, 
                    toGroup, msg.message, m_gridId, client.Scene.Name));
        }
        
        public void RemoveRegion(Scene scene)
        {
//            m_log.DebugFormat("[EVENT RECORDER]: REGION {0} REMOVED", scene.RegionInfo.RegionName);

            scene.EventManager.OnMakeRootAgent -= HandleOnMakeRootAgent;
            scene.EventManager.OnClientClosed -= HandleOnClientClosed;

            if (--RegionsMonitoredCount <= 0)
            {
                m_recorder.Stop();
                m_log.DebugFormat("[EVENT RECORDER]: Stopped.");
            }
        }        
        
        public void RegionLoaded(Scene scene)
        {
            //            m_log.DebugFormat("[EVENT RECORDER]: REGION {0} LOADED", scene.RegionInfo.RegionName);

            RegionsLoadedCount++;

            if (RegionsLoadedCount == RegionsMonitoredCount && !Enabled)
            {
                m_userManagementModule = scene.RequestModuleInterface<IUserManagement>();

                if (m_userManagementModule == null)
                {
                    m_log.ErrorFormat("[EVENT RECORDER]: IUserManagement module required but not present.  Not enabling.");
                    return;
                }

                // If this scene doesn't have a groups module then we shouldn't see any group requests.
                m_groupsModule = scene.RequestModuleInterface<IGroupsModule>();
                            
                MainConsole.Instance.Commands.AddCommand(
                    "EventRecorder", true, "evr info", "evr info", "Show event recorder info", HandleInfoCommand);

                m_log.InfoFormat("[EVENT RECORDER]: Initialized with recorder {0}", m_recorder.Name);

                Enabled = true;

                m_recorder.Start();
            }
        }                
    }
}
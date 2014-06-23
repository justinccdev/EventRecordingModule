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
using System.Data;
using System.Reflection;
using FluentMigrator.Runner;
using log4net;
using MySql.Data.MySqlClient;
using OpenSim.Region.Framework.Scenes;

namespace EventRecorder
{
    public class MySQLRecorder : IRecorder
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string m_connectionString 
            = "Data Source=localhost;Database=eventrecorder;User ID=root;Password=passw0rd;";

        public MySQLRecorder()
        {
            Migrator migrator = new Migrator(m_connectionString);
            migrator.Migrate(runner => runner.MigrateUp());           
        }

        public void RecordUserLogin(ScenePresence sp)
        {
            try
            {
                using (MySqlConnection dbcon = new MySqlConnection(m_connectionString))
                {
                    dbcon.Open();

                    using (MySqlCommand cmd = new MySqlCommand(
                        "insert into Events (UserId, UserName, Type, Region, DateTime) values (?UserId, ?UserName, ?Type, ?Region, ?DateTime)",
                        dbcon))
                    {
                        cmd.Parameters.AddWithValue("?UserId", sp.UUID);
                        cmd.Parameters.AddWithValue("?UserName", sp.Name);
                        cmd.Parameters.AddWithValue("?Type", "login");
                        cmd.Parameters.AddWithValue("?Region", sp.Scene.Name);
                        cmd.Parameters.AddWithValue("?DateTime", DateTime.UtcNow);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                m_log.ErrorFormat(
                    "[MYSQL EVENT RECORDER]: Could not record login of avatar {0} {1} to {2}, error {3}", 
                    sp.Name, sp.UUID, sp.Scene.Name, e);
            }

//            m_log.DebugFormat(
//                "[EVENT RECORDER]: Notified of avatar {0} {1} logging into scene {2}", 
//                sp.Name, sp.UUID, sp.Scene.Name);
        }

        public void RecordUserLogout(ScenePresence sp)
        {
            try
            {
                using (MySqlConnection dbcon = new MySqlConnection(m_connectionString))
                {
                    dbcon.Open();

                    using (MySqlCommand cmd = new MySqlCommand(
                        "insert into Events (UserId, UserName, Type, Region, DateTime) values (?UserId, ?UserName, ?Type, ?Region, ?DateTime)",
                        dbcon))
                    {
                        cmd.Parameters.AddWithValue("?UserId", sp.UUID);
                        cmd.Parameters.AddWithValue("?UserName", sp.Name);
                        cmd.Parameters.AddWithValue("?Type", "logout");
                        cmd.Parameters.AddWithValue("?Region", sp.Scene.Name);
                        cmd.Parameters.AddWithValue("?DateTime", DateTime.UtcNow);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                m_log.ErrorFormat(
                    "[MYSQL EVENT RECORDER]: Could not record login of avatar {0} {1} to {2}, error {3}", 
                    sp.Name, sp.UUID, sp.Scene.Name, e);
            }

//            m_log.DebugFormat(
//                "[EVENT RECORDER]: Notified of avatar {0} {1} logging out of scene {2}", 
//                sp.Name, sp.UUID, sp.Scene.Name);
        }

        public void RecordUserEntrance(ScenePresence sp)
        {
            try
            {
                using (MySqlConnection dbcon = new MySqlConnection(m_connectionString))
                {
                    dbcon.Open();

                    using (MySqlCommand cmd = new MySqlCommand(
                        "insert into Events (UserId, UserName, Type, Region, DateTime) values (?UserId, ?UserName, ?Type, ?Region, ?DateTime)",
                        dbcon))
                    {
                        cmd.Parameters.AddWithValue("?UserId", sp.UUID);
                        cmd.Parameters.AddWithValue("?UserName", sp.Name);
                        cmd.Parameters.AddWithValue("?Type", "enter");
                        cmd.Parameters.AddWithValue("?Region", sp.Scene.Name);
                        cmd.Parameters.AddWithValue("?DateTime", DateTime.UtcNow);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                m_log.ErrorFormat(
                    "[MYSQL EVENT RECORDER]: Could not record login of avatar {0} {1} to {2}, error {3}", 
                    sp.Name, sp.UUID, sp.Scene.Name, e);
            }

//            m_log.DebugFormat(
//                "[EVENT RECORDER]: Notified of avatar {0} {1} moving into scene {2}", 
//                sp.Name, sp.UUID, sp.Scene.Name);  
        }
    }
}
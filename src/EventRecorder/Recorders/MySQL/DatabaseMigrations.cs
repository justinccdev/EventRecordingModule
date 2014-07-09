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

using FluentMigrator;

namespace EventRecorder
{
    [Migration(1)]
    public class DatabaseMigrations : Migration
    {
        public override void Up()
        {
            Create.Table("Events")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("UserId").AsString(36)
                .WithColumn("UserName").AsString(80)
                .WithColumn("Type").AsString(10)
                .WithColumn("Region").AsString(80)
                .WithColumn("DateTime").AsDateTime();
        }

        public override void Down()
        {
            Delete.Table("Events");
        }
    }

    [Migration(2)]
    public class AddGridId : Migration
    {
        public override void Up()
        {
            Alter.Table("Events").AddColumn("GridId").AsString(36).NotNullable();
        }

        public override void Down()
        {
            Delete.Column("GridId").FromTable("Events");
        }
    }

    [Migration(3)]
    public class AddChatEventsTable : Migration
    {
        public override void Up()
        {
            Create.Table("UserChatEvents")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("UserId").AsString(36)
                    .WithColumn("UserName").AsString(80)
                    .WithColumn("OriginX").AsFloat()
                    .WithColumn("OriginY").AsFloat()
                    .WithColumn("OriginZ").AsFloat()
                    .WithColumn("Type").AsString(10)
                    .WithColumn("Text").AsString(1024) // SL chat limit
                    .WithColumn("Channel").AsInt32()
                    .WithColumn("Region").AsString(80)
                    .WithColumn("DateTime").AsDateTime()
                    .WithColumn("GridId").AsString(36).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("UserChatEvents");
        }
    }

    [Migration(4)]
    public class AddImEventsTable : Migration
    {
        public override void Up()
        {
            Create.Table("UserImEvents")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                    .WithColumn("UserId").AsString(36)
                    .WithColumn("UserName").AsString(80)
                    .WithColumn("ReceiverId").AsString(36)
                    .WithColumn("ReceiverName").AsString(80)
                    .WithColumn("ReceiverType").AsString(10)
                    .WithColumn("Text").AsString(1024) // SL limit
                    .WithColumn("Region").AsString(80)
                    .WithColumn("DateTime").AsDateTime()
                    .WithColumn("GridId").AsString(36).NotNullable();
        }

        public override void Down()
        {
            Delete.Table("UserImEvents");
        }
    }

    [Migration(5)]
    public class RenameEventsTable : Migration
    {
        public override void Up()
        {
            Rename.Table("Events").To("UserRegionEvents");
        }

        public override void Down()
        {
            Rename.Table("UserRegionEvents").To("Events");
        }
    }
}
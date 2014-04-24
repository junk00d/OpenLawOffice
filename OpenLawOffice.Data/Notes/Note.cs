﻿// -----------------------------------------------------------------------
// <copyright file="Note.cs" company="Nodine Legal, LLC">
// Licensed to Nodine Legal, LLC under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  Nodine Legal, LLC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenLawOffice.Data.Notes
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using AutoMapper;
    using Dapper;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Note
    {
        public static Common.Models.Notes.Note Get(Guid id)
        {
            return DataHelper.Get<Common.Models.Notes.Note, DBOs.Notes.Note>(
                "SELECT * FROM \"note\" WHERE \"id\"=@id AND \"utc_disabled\" is null",
                new { id = id });
        }

        public static List<Common.Models.Notes.Note> ListForMatter(Guid matterId)
        {
            return DataHelper.List<Common.Models.Notes.Note, DBOs.Notes.Note>(
                "SELECT \"note\".* FROM \"note\" JOIN \"note_matter\" ON " +
                "\"note\".\"id\"=\"note_matter\".\"note_id\" " +
                "WHERE \"note_matter\".\"matter_id\"=@MatterId " +
                "AND \"note\".\"utc_disabled\" is null " +
                "AND \"note_matter\".\"utc_disabled\" is null",
                new { MatterId = matterId });
        }

        public static List<Common.Models.Notes.Note> ListForTask(long taskId)
        {
            return DataHelper.List<Common.Models.Notes.Note, DBOs.Notes.Note>(
                "SELECT \"note\".* FROM \"note\" JOIN \"note_task\" ON " +
                "\"note\".\"id\"=\"note_task\".\"note_id\" " +
                "WHERE \"note_task\".\"task_id\"=@TaskId " +
                "AND \"note\".\"utc_disabled\" is null " +
                "AND \"note_task\".\"utc_disabled\" is null",
                new { TaskId = taskId });
        }

        public static Common.Models.Notes.Note Create(Common.Models.Notes.Note model, Common.Models.Security.User creator)
        {
            if (!model.Id.HasValue) model.Id = Guid.NewGuid();
            model.CreatedBy = model.ModifiedBy = creator;
            model.UtcCreated = model.UtcModified = DateTime.UtcNow;
            DBOs.Notes.Note dbo = Mapper.Map<DBOs.Notes.Note>(model);

            using (IDbConnection conn = Database.Instance.GetConnection())
            {
                conn.Execute("INSERT INTO \"note\" (\"id\", \"title\", \"body\", \"utc_created\", \"utc_modified\", \"created_by_user_id\", \"modified_by_user_id\") " +
                    "VALUES (@Id, @Title, @Body, @UtcCreated, @UtcModified, @CreatedByUserId, @ModifiedByUserId)",
                    dbo);
            }

            return model;
        }

        public static Common.Models.Notes.NoteMatter RelateMatter(Common.Models.Notes.Note model,
            Guid matterId, Common.Models.Security.User creator)
        {
            return NoteMatter.Create(new Common.Models.Notes.NoteMatter()
            {
                Id = Guid.NewGuid(),
                Note = model,
                Matter = new Common.Models.Matters.Matter() { Id = matterId },
                CreatedBy = creator,
                ModifiedBy = creator,
                UtcCreated = DateTime.UtcNow,
                UtcModified = DateTime.UtcNow
            }, creator);
        }

        public static Common.Models.Notes.NoteTask RelateTask(Common.Models.Notes.Note model,
            long taskId, Common.Models.Security.User creator)
        {
            return NoteTask.Create(new Common.Models.Notes.NoteTask()
            {
                Id = Guid.NewGuid(),
                Note = model,
                Task = new Common.Models.Tasks.Task { Id = taskId },
                CreatedBy = creator,
                ModifiedBy = creator,
                UtcCreated = DateTime.UtcNow,
                UtcModified = DateTime.UtcNow
            }, creator);
        }

        public static Common.Models.Notes.Note Edit(Common.Models.Notes.Note model,
            Common.Models.Security.User modifier)
        {
            model.ModifiedBy = modifier;
            model.UtcModified = DateTime.UtcNow;
            DBOs.Notes.Note dbo = Mapper.Map<DBOs.Notes.Note>(model);

            using (IDbConnection conn = Database.Instance.GetConnection())
            {
                conn.Execute("UPDATE \"note\" SET " +
                    "\"title\"=@Title, \"body\"=@Body, \"utc_modified\"=@UtcModified, \"modified_by_user_id\"=@ModifiedByUserId " +
                    "WHERE \"id\"=@Id", dbo);
            }

            return model;
        }
    }
}
﻿using System;
using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class RosterLevelTableInfo
    {
        public string TableName { get; set; }
        public IEnumerable<RosterTableKey> RosterLevelInfo { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using WellNet.Utils;

namespace WellNet.ProcessRunner
{
    public static class StaticResources
    {
        public static int GetPollWaitMilliseconds()
        {
            return Convert.ToInt32(GetValueFromNameValuePair("System", "PollWaitSeconds")) * 1000;
        }

        public static string GetValueFromNameValuePair(string kind, string name)
        {
            return (new ProcessRunnerDcDataContext())
                .NameValuePairs
                .Single(nv => nv.Kind == kind && nv.Name == name)
                .Value;
        }

        public static void LogMessage(int? eventJobId, EventMessageSeverity severity, string context, string message, BackgroundWorker bgWorker)
        {
            var dc = new ProcessRunnerDcDataContext();
            var eventMessage = new Event_Message();
            if (eventJobId.HasValue)
                eventMessage.Event_JobId = eventJobId.Value;
            eventMessage.Severity = (int)severity;
            if (!string.IsNullOrEmpty(context))
                eventMessage.Context = context;
            if (!string.IsNullOrEmpty(message))
                eventMessage.Message = message;
            dc.Event_Messages.InsertOnSubmit(eventMessage);
            dc.SubmitChanges();
            if (bgWorker != null)
                bgWorker.ReportProgress(0, message);
        }

        public static string ArchiveFile(int vendorId, SetupJobDirection direction, string localFile)
        {
            var dc = new ProcessRunnerDcDataContext();
            var vendor = dc.Vendors.Single(v => v.Id == vendorId);
            var archivePath = dc.NameValuePairs.Single(nv => nv.Name.Equals("TransmissionArchive")).Value;
            var path = Path.Combine(archivePath, direction.ToString(), vendor.ArchiveLocation);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path = Path.Combine(path, DateTime.Now.Year.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var archiveFilename = Path.Combine(path, Path.GetFileName(localFile));
            if (File.Exists(archiveFilename))
                UiLibrary.AppendGuidToFilename(archiveFilename);
            File.Copy(localFile, archiveFilename);
            return archiveFilename;
        }

        public static void MarkFileAsFailed(string filename)
        {
            var failedFile = string.Format("{0}_FAILED{1}", Path.GetFileNameWithoutExtension(filename), Path.GetExtension(filename));
            if (File.Exists(failedFile))
                UiLibrary.AppendGuidToFilename(failedFile);
            File.Move(filename, failedFile);
        }

        public static bool ProcessStatusTable0AndReportIfFatal(int eventJobId, string funcContext, DataTable statusTable, BackgroundWorker bgWorker)
        {
            //SP's should have as their first table:
            //Severity int -- if 1, stops process, 2: info/documenting results, 3: transitory ui messages
            //Context varchar(1000) - a way to id a record if needed
            //Message varchar(1000)
            //create table #ProcessRunnerReporting (Severity int, Context varchar(500), Message varchar(500))
            var result = false;
            foreach (var dataRow in statusTable.Rows.Cast<DataRow>().OrderBy(dr => dr[0]))
            {
                var severity = (EventMessageSeverity)Convert.ToInt32(dataRow[0]);
                if (!result && severity == EventMessageSeverity.Fatal)
                    result = true;
                var context = string.Format("{0} {1}", funcContext, dataRow[1]).Trim();
                var message = dataRow[2].ToString();
                LogMessage(eventJobId, severity, context, message, bgWorker);
            }
            return result;
        }

        public static Dictionary<string, string> GetParametersForForSetupJobFunction(int setupJobId, int setupJobFunctionId)
        {
            //select
            //	 kp.Name ParmName
            //	,sp.Value ParmValue
            //from
            //	Setup_JobFunctionParameter sjfp
            //	inner join Setup_Parameter sp on sjfp.Setup_ParameterId = sp.Id
            //	inner join Kind_Parameter kp on sp.Kind_ParameterId = kp.Id
            //where
            //	sjfp.Setup_JobId = @setupJobId
            //	and sjfp.Setup_JobFunctionId = @setupJobFunctionId
            var dc = new ProcessRunnerDcDataContext();
            var q = from sjfp in dc.Setup_JobFunctionParameters
                    join sp in dc.Setup_Parameters on sjfp.Setup_ParameterId equals sp.Id
                    join kp in dc.Kind_Parameters on sp.Kind_ParameterId equals kp.Id
                    where (sjfp.Setup_JobId == setupJobId) && (sjfp.Setup_JobFunctionId == setupJobFunctionId)
                    select new { key = kp.Name, value = sp.Value };
            return q.ToDictionary(a => a.key, a => a.value);
        }

        public static string FilenameWithDate(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return filename;
            var parts = filename.Replace("}", "{").Split('{');
            if (parts.Length != 3)
                return filename;
            var datePart = parts[1];
            var dateMask = "yyyy-MM-dd";
            if (!datePart.ToUpper().Equals("DATE"))
                dateMask = datePart;
            datePart = DateTime.Today.ToString(dateMask);
            return string.Format("{0}{1}{2}", parts[0], datePart, parts[2]);
        }

        public static void UpdateEventJobStatusAndRunWhen(int eventJobId, EventJobStatus status)
        {
            var dc = new ProcessRunnerDcDataContext();
            var eventJob = dc.Event_Jobs.Single(ej => ej.Id == eventJobId);
            eventJob.Status = (int)status;
            dc.SubmitChanges();
            if (status == EventJobStatus.Working)
                dc.UpdateEventJobRunWhen(eventJobId);
        }
    }
}

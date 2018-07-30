using System;
using System.Data;
using System.IO;
using System.Linq;
using WellNet.Sql;
using WellNet.Utils;

namespace WellNet.ProcessRunner
{
    public class Function_StoredProcToFile : FunctionBase<FluentSchedulerJob>, IFunction
    {
        private enum FileType
        {
            PipeDelimitedNoHeader
        }

        public Function_StoredProcToFile(FluentSchedulerJob job, int jobFunctionId) : base(job, jobFunctionId)
        {
        }

        public void Execute()
        {
            var connMgr = ConnectionManager.Create();
            var sqlHelper = SqlHelper.Create(connMgr[_parameters["SqlConnection"]]);
            var cmd = sqlHelper.CreateStoredProcCommand(_parameters["StoredProcedure"], false);
            var dataSet = sqlHelper.PopulateDataSet(cmd);
            if (StaticResources.ProcessStatusTable0AndReportIfFatal(_fluentSchedulerJob.EventJobId, Context, dataSet.Tables[0], _fluentSchedulerJob.BgWorker))
                return;
            FileType fileType;
            if (!Enum.TryParse(_parameters["FileType"], out fileType))
                throw new ProcessRunnerException(Context, string.Format("Unknown file type: {0}", _parameters["FileType"]));
            var tempFileName = Path.GetTempFileName();
            using (TextWriter tw = new StreamWriter(tempFileName))
            {
                switch (fileType)
                {
                    case (FileType.PipeDelimitedNoHeader):
                        foreach (DataRow dataRow in dataSet.Tables[1].Rows)
                            tw.WriteLine( dataRow.ItemArray.Aggregate((a,b) => string.Format("{0}|{1}",a,b)) );
                        break;
                }
            }
            var fileName = _parameters["FileName"] == "TempFile" ? tempFileName : StaticResources.FilenameWithDate(_parameters["FileName"]);
            if (fileName != tempFileName)
            {
                if (string.IsNullOrEmpty(Path.GetDirectoryName(fileName)))
                    fileName = Path.Combine(Path.GetDirectoryName(tempFileName), fileName);
                File.Move(tempFileName, fileName);
            }
            _fluentSchedulerJob.PropertyBag[FluentSchedulerJob.PropertyBagKey.Filename] = fileName;
        }
    }
}

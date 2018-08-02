using WellNet.Sql;
using WellNet.Utils;

namespace WellNet.ProcessRunner
{
    public class Function_ExecuteStoredProc : FunctionBase<FluentSchedulerJob>, IFunction
    {
        public Function_ExecuteStoredProc(FluentSchedulerJob job, int jobFunctionId) : base(job, jobFunctionId)
        {
        }

        public void Execute()
        {
            var connMgr = ConnectionManager.Create();
            var sqlHelper = SqlHelper.Create(connMgr[_parameters["SqlConnection"]]);
            var cmd = sqlHelper.CreateStoredProcCommand(_parameters["StoredProcedure"], false);
            var dataSet = sqlHelper.PopulateDataSet(cmd);
            StaticResources.ProcessStatusTable0AndReportIfFatal(CallingJob.EventJobId, Context, dataSet.Tables[0], CallingJob.BgWorker);

            //todo: handle params for the sp
        }
    }
}

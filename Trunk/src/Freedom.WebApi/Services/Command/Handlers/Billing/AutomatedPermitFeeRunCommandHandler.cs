using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Hedgehog.CommandModel;
using Hedgehog.CommandModel.Billing;
using Hedgehog.Model;
using Hedgerow;
using Hedgerow.Extensions;

namespace Hedgehog.Services.Command.Handlers.Billing
{
    public class AutomatedPermitFeeRunCommandHandler : CommandHandlerBase<AutomatedPermitFeeRunCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.AddPermitFee; }
        }

        private static DataTable BuildIdTable(IEnumerable<Guid> guids)
        {
            DataTable dataTable = new DataTable("tvp_IdTable");

            dataTable.Columns.Add("Id", typeof(Guid));

            foreach (Guid value in guids)
                dataTable.Rows.Add(value);

            return dataTable;
        }

        public override async Task<CommandResult> Handle(CommandBase commandBase, CommandExecutionContext context)
        {
            AutomatedPermitFeeRunCommand command = (AutomatedPermitFeeRunCommand) commandBase;
            int numberOfTransactionsCreated = 0;

            using (IDbConnection connection = IoC.Get<IDbConnection>())
            {
                connection.Open();
                using (SqlTransaction transaction = (SqlTransaction) connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand dbCommand = (SqlCommand) connection.CreateCommand())
                        {
                            dbCommand.CommandType = CommandType.StoredProcedure;
                            dbCommand.CommandText = "GeneratePermitFees";
                            dbCommand.Transaction = transaction;

                            dbCommand.CreateParameter("batchId", command.BatchId);
                            dbCommand.CreateParameter("batchNumber",
                                $"BN{command.CommandCreatedDateTime:yyyyMMddhhmmss}");
                            dbCommand.CreateParameter("batchDateTime", command.CommandCreatedDateTime);
                            dbCommand.CreateParameter("billingYear", command.BillingCycleStartDate.Year);
                            dbCommand.CreateParameter("billingMonth", command.BillingCycleStartDate.Month);
                            dbCommand.CreateParameter("canFilterJustMyWorkAreas", command.CanFilterByJustMyWorkAreas);
                            dbCommand.CreateParameter("workAreaIds", BuildIdTable(command.WorkAreaIds));
                            dbCommand.CreateParameter("permitFeeCategoryIds", BuildIdTable(command.PermitFeeCategoryIds));
                            dbCommand.CreateParameter("modifiedDateTime", context.CurrentDateTime);
                            dbCommand.CreateParameter("modifiedById", context.CurrentUserId);
                            dbCommand.CreateParameter(ParameterDirection.Output, "numberOfTransactionsCreated", numberOfTransactionsCreated);

                            await dbCommand.ExecuteNonQueryAsync();
                            numberOfTransactionsCreated = (int) dbCommand.Parameters["numberOfTransactionsCreated"].Value;
                        }

                        if (numberOfTransactionsCreated > 0)
                            transaction.Commit();
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }

                connection.Close();
            }

            return numberOfTransactionsCreated > 0 ? new CommandResult(true) : new CommandResult(false);
        }
    }
}

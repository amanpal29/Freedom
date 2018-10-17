using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Hedgehog.CommandModel.Contacts;
using Hedgehog.Model;
using Hedgerow;
using Hedgerow.Extensions;

namespace Hedgehog.Services.Command.Handlers.Contacts
{
    public class MergerContactsCommandHandler : LocalContextCommandHandler<MergeContactsCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield return SystemPermission.RemoveDuplicateContacts; }
        }
        protected override async Task<bool> Handle(HedgehogLocalContext db, MergeContactsCommand commandBase, CommandExecutionContext context)
        {
            PopulateContactImage(db, commandBase);
            MergeContactsCommand command = commandBase;
            command.NewContact.CreatedById = command.NewContact.ModifiedById = context.CurrentUserId;
            command.NewContact.CreatedDateTime = command.NewContact.ModifiedDateTime = context.CurrentDateTime;
            db.Contact.Add(command.NewContact); // after here, the variable HedgehogLocalContext db, has the correct state to add the work item to re-index the new contact in the contact digest

            await db.OnCommittingAsync(command, context);

            await db.SaveChangesAsync();

            return await MergeContacts(command, context);
        }

        private void PopulateContactImage(HedgehogLocalContext db, MergeContactsCommand command)
        {
            List<Contact> contactWithImageList = db.Contact.Where(c => command.DeleteContactIds.Contains(c.Id) && c.ContactCardImage != null).OrderByDescending(c => c.ModifiedDateTime).ToList();
            if (contactWithImageList.Any())
            {
                command.NewContact.ContactCardImage = ( byte[] ) contactWithImageList.First().ContactCardImage.Clone();
            }
        }

        private async Task<bool> MergeContacts(MergeContactsCommand command, CommandExecutionContext context)
        {
            int sucess = 0;
            using (IDbConnection connection = IoC.Get<IDbConnection>())
            {
                connection.Open();

                using (SqlTransaction transaction = (SqlTransaction)connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand dbCommand = (SqlCommand)connection.CreateCommand())
                        {

                            dbCommand.CommandType = CommandType.StoredProcedure;
                            dbCommand.CommandText = "MergeContacts";
                            dbCommand.Transaction = transaction;

                            dbCommand.CreateParameter("deleteContactIds", BuildIdTable(command.DeleteContactIds));
                            dbCommand.CreateParameter("newContactId", command.NewContact.Id);
                            dbCommand.CreateParameter("modifiedDateTime", context.CurrentDateTime);
                            dbCommand.CreateParameter("modifiedById", context.CurrentUserId);
                            dbCommand.CreateParameter(ParameterDirection.Output, "sucess", sucess);

                            await dbCommand.ExecuteNonQueryAsync();
                            sucess = (int)dbCommand.Parameters["sucess"].Value;
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return sucess == 1;
        }

        private static DataTable BuildIdTable(IEnumerable<Guid> guids)
        {
            DataTable dataTable = new DataTable("tvp_IdTable");

            dataTable.Columns.Add("Id", typeof(Guid));

            foreach (Guid value in guids)
                dataTable.Rows.Add(value);

            return dataTable;
        }
    }
}

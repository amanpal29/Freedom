using System;
using Freedom.Domain.CommandModel;

namespace Freedom.Domain.Services.Repository.PendingChanges
{
    public class PendingChange
    {
        internal PendingChange(int id, DateTimeOffset transactionDateTime, CommandBase command)
        {
            Id = id;
            TransactionDateTime = transactionDateTime;
            Command = command;
        }

        internal PendingChange(int id, Exception ex)
        {
            Id = id;
            Error = ex;
        }

        public int Id { get; }

        public DateTimeOffset TransactionDateTime { get; }

        public CommandBase Command { get; }

        public Exception Error { get; }

        public bool IsError => Error != null;
    }
}
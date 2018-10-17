using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Freedom.Domain.CommandModel;
using Freedom.Extensions;

namespace Freedom.Domain.Services.Repository.PendingChanges
{
    internal class PendingChangesEnumerable : IEnumerable<PendingChange>
    {
        private readonly DbConnection _connection;
        private readonly Guid _userId;
        private readonly byte[] _symmetricKey;

        public const string CountSql = "select count(*) from [_PendingChanges] where [UserId] = @userId";

        internal PendingChangesEnumerable(DbConnection connection, Guid userId, byte[] symmetricKey)
        {
            _connection = connection;
            _userId = userId;
            _symmetricKey = symmetricKey;
        }

        #region Implementation of IEnumerable

        public IEnumerator<PendingChange> GetEnumerator()
        {
            DbCommand command = _connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText =
                "select [Id], [transactionDateTime], datalength([payload]), [payload] " +
                "from [_PendingChanges] " +
                "where [UserId] = @userId";

            command.CreateParameter("userId", _userId);

            return new PendingChangeEnumerator(command.ExecuteReader(CommandBehavior.SequentialAccess), _symmetricKey);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region PendingChangeEnumerator

        private sealed class PendingChangeEnumerator : IEnumerator<PendingChange>
        {
            private const int InitializationVectorSizeInBytes = 16;

            private readonly SqlDataReader _dataReader;
            private readonly byte[] _symmetricKey;

            internal PendingChangeEnumerator(DbDataReader dataReader, byte[] symmetricKey)
            {
                _symmetricKey = symmetricKey;
                _dataReader = (SqlDataReader)dataReader;
            }

            private ICryptoTransform GetDecryptor(byte[] iv)
            {
                if (iv == null)
                    throw new ArgumentNullException(nameof(iv));

                SymmetricAlgorithm algorithm = new AesCryptoServiceProvider();

                algorithm.KeySize = _symmetricKey.Length * 8;
                algorithm.Key = _symmetricKey;

                algorithm.IV = iv;

                return algorithm.CreateDecryptor();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
            private CommandBase DecryptCommand(byte[] encryptedPayload)
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(CommandBase));

                using (MemoryStream memoryStream = new MemoryStream(encryptedPayload))
                {
                    byte[] iv = new byte[InitializationVectorSizeInBytes];

                    memoryStream.Read(iv, 0, iv.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, GetDecryptor(iv), CryptoStreamMode.Read))
                    using (GZipStream compressionStream = new GZipStream(cryptoStream, CompressionMode.Decompress, true))
                        return (CommandBase)serializer.ReadObject(compressionStream);
                }
            }

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                if (!_dataReader.Read())
                {
                    Current = null;
                    return false;
                }

                int id = _dataReader.GetInt32(0);

                try
                {
                    DateTimeOffset dateTimeOffset = _dataReader.GetDateTimeOffset(1);

                    long payloadLength =  _dataReader.GetInt64(2);

                    byte[] payloadBytes = new byte[payloadLength];

                    _dataReader.GetBytes(3, 0, payloadBytes, 0, payloadBytes.Length);

                    Current = new PendingChange(id, dateTimeOffset, DecryptCommand(payloadBytes));

                }
                catch (Exception ex)
                {
                    Current = new PendingChange(id, ex);
                }
   
                return true;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public PendingChange Current { get; private set; }

            object IEnumerator.Current => Current;

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                    _dataReader?.Dispose();
            }

            #endregion
        }

        #endregion
    }
}

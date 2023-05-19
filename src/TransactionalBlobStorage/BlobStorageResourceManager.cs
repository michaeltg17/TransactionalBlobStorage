using System.Transactions;
using TransactionalBlobStorage.Operations.Base;

namespace TransactionalBlobStorage
{
    public class BlobStorageResourceManager : IEnlistmentNotification, IBlobStorageResourceManager
    {
        List<TransactionalBlobOperation> executedOperations;

        public Task ExecuteOperation(BlobOperation operation)
        {
            AddOperation(operation);
            return operation.ExecuteInTransaction();
        }

        public async Task<T> ExecuteOperation<T>(BlobOperation<T> operation)
        {
            AddOperation(operation);
            return (T)(await operation.ExecuteInTransaction())!;
        }

        void AddOperation(TransactionalBlobOperation operation)
        {
            if (executedOperations == null)
            {
                var currentTransaction = Transaction.Current;
                currentTransaction?.EnlistVolatile(this, EnlistmentOptions.None);
                executedOperations = new List<TransactionalBlobOperation>();
            }

            executedOperations.Add(operation);
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            executedOperations.Reverse();
            foreach (var operation in executedOperations)
            {
                operation.Rollback().GetAwaiter().GetResult();
            }

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            Rollback(enlistment);
        }
    }
}

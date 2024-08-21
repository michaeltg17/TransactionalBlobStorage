using System.Transactions;

namespace TransactionalBlobStorage.Net.Helpers
{
    public static class TransactionHelper
    {
        public static bool IsInTransaction()
        {
            return Transaction.Current != null;
        }
    }
}

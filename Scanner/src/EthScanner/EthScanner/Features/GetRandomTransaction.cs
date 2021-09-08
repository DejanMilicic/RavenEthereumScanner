using System.Threading;
using System.Threading.Tasks;
using EthScanner.Models;
using MediatR;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace EthScanner.Features
{
    public class GetRandomTransaction
    {
        public class Query : IRequest<TransactionInfo>
        {

        }

        internal class Handler : IRequestHandler<Query, TransactionInfo>
        {
            private readonly IDocumentStore _store;

            public Handler(IDocumentStore store)
            {
                _store = store;
            }

            public async Task<TransactionInfo> Handle(Query query, CancellationToken cancellationToken)
            {
                using var session = _store.OpenAsyncSession(new SessionOptions { NoTracking = true });
                TransactionInfo tr = await session.Query<TransactionInfo>().FirstAsync();
                return tr;
            }
        }
    }
}

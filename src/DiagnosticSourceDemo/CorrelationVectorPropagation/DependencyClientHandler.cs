using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CorrelationVectorPropagation
{
    /// <summary>
    /// This Delegating Handler is responsible for tracking the Dependency Name and
    /// Type attributes of a given Http Client in such a way that it can be used
    /// for logging at the conclusion of an Http Request.
    /// </summary>
    public class DependencyClientHandler : DelegatingHandler
    {
        public const string RequestPropertyKey = "DependencyInfo";
        private string _dependencyName, _dependencyType;

        public DependencyClientHandler(string dependencyName, string dependencyType)
            : base(new HttpClientHandler())
        {
            _dependencyName = dependencyName;
            _dependencyType = dependencyType;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Properties.ContainsKey(RequestPropertyKey))
            {
                request.Properties.Add(
                    RequestPropertyKey,
                    new DependencyOperationInfo()
                    {
                        DependencyName = _dependencyName,
                        DependencyType = _dependencyType
                    });
            }
            else if (request.Properties["DependencyInfo"] is DependencyOperationInfo dependencyOperationInfo)
            {
                dependencyOperationInfo.DependencyName = _dependencyName;
                dependencyOperationInfo.DependencyType = _dependencyType;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}

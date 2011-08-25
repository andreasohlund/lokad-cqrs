namespace Lokad.Cqrs.Feature.Http
{
    public class HttpEnvironment : IHttpEnvironment
    {
        public string OptionalHostName { get; set; }
        public int Port { get; set; }
        public string VirtualDirectory { get; set; }
    }
}
using System.Threading.Tasks;

namespace Language_Dictionary.Infrastructure
{
    internal delegate Task ActionAsync();

    internal delegate Task ActionAsync<in T>(T parameter);
}
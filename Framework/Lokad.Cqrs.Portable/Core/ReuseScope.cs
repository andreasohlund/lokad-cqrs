
namespace Lokad.Cqrs.Core
{
	/// <summary>
	/// Determines visibility and reuse of instances provided by the container.
	/// </summary>
	public enum ReuseScope
	{
		/// <summary>
		/// Instances are reused only at the given container. Descendent 
		/// containers do not reuse parent container instances and get  
		/// a new instance at their level.
		/// </summary>
		Container, 
		/// <summary>
		/// Each request to resolve the dependency will result in a new 
		/// instance being returned.
		/// </summary>
		None,
	}
}

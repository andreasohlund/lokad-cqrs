using System;

namespace Lokad.Cqrs.Core
{
    public sealed class ServiceKey
	{
	    readonly int _hash;

		public ServiceKey(Type factoryType, string serviceName)
		{
			FactoryType = factoryType;
			Name = serviceName;

			_hash = factoryType.GetHashCode();
			if (serviceName != null)
				_hash ^= serviceName.GetHashCode();
		}

		public readonly Type FactoryType;
		public readonly string Name;

		#region Equality

		public bool Equals(ServiceKey other)
		{
			return ServiceKey.Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			return ServiceKey.Equals(this, obj as ServiceKey);
		}

		public static bool Equals(ServiceKey obj1, ServiceKey obj2)
		{
			if (Object.Equals(null, obj1) ||
				Object.Equals(null, obj2))
				return false;

			return obj1.FactoryType == obj2.FactoryType && 
				obj1.Name == obj2.Name;
		}

		public override int GetHashCode()
		{
			return _hash;
		}

		#endregion
	}
}
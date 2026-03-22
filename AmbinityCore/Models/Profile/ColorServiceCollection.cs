using System.Collections.Generic;
using AmbinityCore.LightingEngines;
using AmbinityCore.Models.Lighting.Zone;

namespace AmbinityCore.Models.Profile
{
    public class ColorServiceManager
    {

        private readonly object _lock = new();
        private readonly Dictionary<LightingZone, IColorService> _currentActiveColorServices = new();

        public void Add(IColorService service)
        {
            if (service == null || service.Zone == null) return;
            lock (_lock)
            {
                _currentActiveColorServices[service.Zone] = service;
            }
        }

        public void Remove(IColorService service)
        {
            if (service == null || service.Zone == null) return;
            lock (_lock)
            {
                _currentActiveColorServices.Remove(service.Zone);
            }
        }

        public IColorService? GetByZone(LightingZone zone)
        {
            if (zone == null) return null;
            lock (_lock)
            {
                _currentActiveColorServices.TryGetValue(zone, out var service);
                return service;
            }
        }
        public List<IColorService> GetAll()
        {
            lock (_lock)
            {
                return new List<IColorService>(_currentActiveColorServices.Values);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _currentActiveColorServices.Clear();
            }
        }
    }
}

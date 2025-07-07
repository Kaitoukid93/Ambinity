using System;
using System.Collections.Generic;
using System.Linq;
using Ambinity.Views.Draw2DCanvas;

namespace Ambinity.Views.LayoutEditor.Canvas
{
    public class CanvasViewModelFactory
    {
        private readonly Dictionary<Type, CanvasViewModelBase> _instances;
        private CanvasViewModelBase _current;
        public CanvasViewModelBase Current
        {
            get => _current;
            private set
            {

                _current = value;
                CurrentChanged?.Invoke(_current);

            }
        }
        public event Action<CanvasViewModelBase> CurrentChanged;
        public CanvasViewModelFactory(IEnumerable<CanvasViewModelBase> canvasViewModels)
        {
            // Store all injected CanvasViewModelBase implementations by their type
            _instances = canvasViewModels.ToDictionary(vm => vm.GetType(), vm => vm);
            Current = _instances.Values.FirstOrDefault();
        }

        /// <summary>
        /// Get an instance of the requested CanvasViewModelBase-derived type.
        /// Sets the returned instance as the current active instance.
        /// </summary>
        public T Get<T>() where T : CanvasViewModelBase
        {
            var type = typeof(T);
            if (!_instances.TryGetValue(type, out var instance))
                throw new InvalidOperationException($"No CanvasViewModelBase of type {type.Name} registered.");
            Current = instance;
            return (T)instance;
        }

        /// <summary>
        /// Set the current active CanvasViewModelBase instance directly.
        /// </summary>
        public void SetCurrent(CanvasViewModelBase viewModel)
        {
            if (!_instances.ContainsValue(viewModel))
                throw new InvalidOperationException("The provided viewmodel is not managed by this factory.");
            Current = viewModel;
        }
    }
}

using System.Collections.Generic;

namespace Ambinity.Services
{
    public class CanvasHistoryService<T>
    {
        private readonly Stack<T> _undoStack = new Stack<T>();
        private readonly Stack<T> _redoStack = new Stack<T>();

        public void SaveState(T state)
        {
            _undoStack.Push(state);
            _redoStack.Clear(); // Clear redo stack when a new state is saved
        }

        public T Undo(T currentState)
        {
            if (_undoStack.Count == 0) return currentState;

            _redoStack.Push(currentState);
            return _undoStack.Pop();
        }

        public T Redo(T currentState)
        {
            if (_redoStack.Count == 0) return currentState;

            _undoStack.Push(currentState);
            return _redoStack.Pop();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }
}

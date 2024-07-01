using Avalonia.Input;

namespace Draw2D.Core.Policies.CanvasPolicy
{
    public abstract class KeyboardPolicy : PolicyBase
    {
        public abstract void OnKeyDown(Canvas canvas, Key key);

        public abstract void OnKeyUp(Canvas canvas, Key key);

    }

    public class ConnectionModeKeyboardPolicy : KeyboardPolicy
    {
        
        public override void OnKeyDown(Canvas canvas, Key key)
        {
            if (key == Key.C)
            {
                canvas.ActivateConnectionRouter();
            }
        }

        public override void OnKeyUp(Canvas canvas, Key key)
        {
            if (key == Key.C)
            {

            }
        }
    }
}

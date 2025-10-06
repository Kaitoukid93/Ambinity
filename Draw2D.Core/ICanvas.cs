using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Draw2D.Core.Factories.Handles;
using Draw2D.Core.Graphic;
using Draw2D.Core.Layout.Connection;
using Draw2D.Core.Policies;
using Draw2D.Core.Policies.RouterPolicy;

namespace Draw2D.Core
{
    public interface ICanvas
    {
        IEnumerable<Figure> Figures { get; }

        event EventHandler<EventArgs> SceneChanged;
        event EventHandler<FigureClickEventArgs> FigureRightClicked;
        event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;
        event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        event EventHandler<CanvasClickEventArgs> CanvasRightClicked;
        event EventHandler<HoverChangedEventArgs> FigureHoverChanged;
        void AddFigure(Figure figure);
        Selection Selection { get; }

        /// <summary>
        /// Width in 1/100 mm.
        /// </summary>
        float Width { get; set; }

        /// <summary>
        /// Height in 1/100 mm.
        /// </summary>
        float Height { get; set; }

        float ZoomLevel { get; set; }

        ICoordinateSystem CoordinateSystem { get; set; }
        HandleShapeFactory HandleShapeFactory { get; }
        ToolBase ActiveTool { get; }
        IGrid Grid { get; set; }
        double ViewportWidth { get; set; }
        double ViewportHeight { get; set; }
        double ContentOffsetX { get; set; }
        double ContentOffsetY { get; set; }
        void OnMouseLeftDown(double x, double y, bool isShiftKey, bool isCtrlKey);
        void OnMouseMove(double x, double y, bool isShiftKey, bool isCtrlKey);
        void OnMouseLeftUp(double x, double y, bool isShiftKey, bool isCtrlKey);

        void OnMouseRightDown(double mouseX, double mouseY, bool isShiftKey, bool isCtrlKey);
        void OnMouseRightUp(double mouseX, double mouseY, bool isShiftKey, bool isCtrlKey);

        ICanvas InstallEditPolicy(PolicyBase policy);
        ICanvas UninstallEditPolicy(PolicyBase policy);
        void OnKeyDown(Key key);
        void OnKeyUp(Key key);
        void CreateConnection();
        void OnMouseLeftDoubleClick(double mouseX, double mouseY, bool isShiftKey, bool isCtrlKey);

        List<VectorFigure> GetRenderableFigures();
        void InstallTool(ToolBase tool, Action<ToolBase> onDone);

        IEnumerable<ISnapPolicy> GetInstalledSnapPolicies();
        void RemoveSelected();
        void Clear();
        void StartBulkEdit();
        void EndBulkEdit();
        Color StrokeColor { get; set; }
        FrameBuffer BackgroundImageBuffer { get; set; }

        bool ShouldDrawBackgroundImage { get; set; }
        bool ShouldDrawEntityColors { get; set; }
        bool ShouldDrawBorder { get; set; }
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        public SelectionChangedEventArgs()
        {
        }
    }

    public class HoverChangedEventArgs : EventArgs
    {
        public Figure Figure { get; private set; }
        public bool IsHover { get; private set; }
        public HoverChangedEventArgs(Figure figure,bool isHover)
        {
            Figure = figure;
            IsHover = isHover;
        }
    }

    public class ConnectionCreatedEventArgs : EventArgs
    {
        public Connection Connection { get; private set; }

        public ConnectionCreatedEventArgs(Connection connection)
        {
            Connection = connection;
        }
    }

    public class CanvasClickEventArgs : EventArgs
    {
        public ICanvas Sender { get; private set; }
        public float MousePosX { get; private set; }
        public float MousePosY { get; private set; }

        public CanvasClickEventArgs(ICanvas sender,float mousePosX, float mousePosY)
        {
            Sender = sender;
            MousePosX = mousePosX;
            MousePosY = mousePosY;
        }
    }

    public class FigureClickEventArgs : EventArgs
    {
        public Figure Sender { get; private set; }
        public float MousePosX { get; private set; }
        public float MousePosY { get; private set; }

        public FigureClickEventArgs(Figure sender, float mousePosX, float mousePosY)
        {
            Sender = sender;
            MousePosX = mousePosX;
            MousePosY = mousePosY;
        }
    }
}

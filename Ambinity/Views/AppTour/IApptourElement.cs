namespace Ambinity.Views.AppTour;

public interface IApptourElement
{
    string Name { get;}
    void Deactivate();
    void Activate();
    int CurrentStep { get; set; }
    int StepCount { get;}
}
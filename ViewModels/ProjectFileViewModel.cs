namespace ShieldVSExtension.ViewModels;

public class ProjectFileViewModel
{
    public string FileName { get; set; }

    public ProjectFileViewModel()
    {
    }

    public ProjectFileViewModel(string fileName)
    {
        FileName = fileName;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;

namespace ShieldVSExtension.Common.Helpers
{
    public static class Dte2Helper
    {
        private const string VsProjectKindSolutionFolder = ProjectKinds.vsProjectKindSolutionFolder;

        private const string VsProjectKindMiscFiles = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";


        public static IEnumerable<Project> GetProjects(this Solution solution)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var result = new List<Project>();

            foreach (var project in solution.Projects.OfType<Project>())
            {
                if (project.Kind == VsProjectKindMiscFiles)
                    continue;

                if (project.Kind == VsProjectKindSolutionFolder)
                {
                    result.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    result.Add(project);
                }
            }

            return result;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var result = new List<Project>();
            foreach (var projectItem in solutionFolder.ProjectItems.OfType<ProjectItem>())
            {
                var subProject = projectItem.SubProject;
                if (subProject == null)
                    continue;

                if (subProject.Kind == VsProjectKindMiscFiles)
                    continue;

                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    result.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    result.Add(subProject);
                }
            }

            return result;
        }

        public static string GetFullOutputPath(this Project project)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (Path.GetExtension(project.FullName).Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    return Path.Combine(Path.GetDirectoryName(project.FullName) ?? throw new InvalidOperationException(), (string)project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value);
                }
                else
                {
                    var outputUrlStr = ((object[])project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item("Built").FileURLs).OfType<string>().First();
                    var outputUrl = new Uri(outputUrlStr, UriKind.Absolute);
                    return Path.GetDirectoryName(outputUrl.LocalPath);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}

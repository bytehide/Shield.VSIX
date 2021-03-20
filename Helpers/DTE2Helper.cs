using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShieldVSExtension.Helpers
{
    public static class DTE2Helper
    {
        private const string vsProjectKindSolutionFolder = ProjectKinds.vsProjectKindSolutionFolder;

        private const string vsProjectKindMiscFiles = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";


        public static IEnumerable<Project> GetProjects(this Solution solution)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var result = new List<Project>();

            foreach (var project in solution.Projects.OfType<Project>())
            {
                if (project.Kind == vsProjectKindMiscFiles)
                    continue;

                if (project.Kind == vsProjectKindSolutionFolder)
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

                if (subProject.Kind == vsProjectKindMiscFiles)
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

            if (Path.GetExtension(project.FullName).Equals(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(Path.GetDirectoryName(project.FullName), (string)project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value);
            }
            else
            {
                var outputUrlStr = ((object[])project.ConfigurationManager.ActiveConfiguration.OutputGroups.Item("Built").FileURLs).OfType<string>().First();
                var outputUrl = new Uri(outputUrlStr, UriKind.Absolute);
                return Path.GetDirectoryName(outputUrl.LocalPath);
            }
        }
    }
}

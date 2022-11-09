using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace ShieldVSExtension.Helpers
{
    public static class ProjectItemExtensions
    {
        public static bool ProjectItemIsDirty(this ProjectItem projectItem)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (projectItem.IsDirty)
            {
                return true;
            }

            return projectItem.ProjectItems != null && projectItem.ProjectItems.Cast<ProjectItem>().Any(ProjectItemIsDirty);
        }

        /// <summary>
        /// Find file in projects collection.
        /// </summary>
        /// <param name="items">Projects collection.</param>
        /// <param name="filePath">File path, relative to the <paramref name="items"/> root.</param>
        /// <returns>The found file or <c>null</c>.</returns>
        public static ProjectItem FindProjectItem(this ProjectItems items, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Argument `filePath` is null or empty.", "filePath");
            }

            var backslashIndex = filePath.IndexOf("\\", StringComparison.Ordinal);
            var findFolder = backslashIndex != -1;
            if (findFolder)
            {
                var folderName = filePath.Substring(0, backslashIndex);
                return (from ProjectItem item in items where item.Kind == Constants.vsProjectItemKindVirtualFolder || item.Kind == Constants.vsProjectItemKindPhysicalFolder where folderName == item.Name let nextpath = filePath.Substring(backslashIndex + 1) select FindProjectItem(item.ProjectItems, nextpath)).FirstOrDefault();
            }
            else
            {
                var fileName = filePath;
                foreach (var item in items.Cast<ProjectItem>().Where(item => item.Kind == Constants.vsProjectItemKindPhysicalFile))
                {
                    if (item.Name == fileName)
                    {
                        return item;
                    }

                    // Nested item, e.g. Default.aspx or MainWindow.xaml.
                    if (item.ProjectItems.Count <= 0) continue;
                    var childItem = FindProjectItem(item.ProjectItems, fileName);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }

            return null;
        }
    }
}

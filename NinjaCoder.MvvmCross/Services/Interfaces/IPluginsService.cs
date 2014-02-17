﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the IPluginsService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.Services.Interfaces
{
    using System.Collections.Generic;

    using NinjaCoder.MvvmCross.Entities;

    using Scorchio.VisualStudio.Services.Interfaces;

    /// <summary>
    /// Defines the IPluginsService type.
    /// </summary>
    public interface IPluginsService
    {
        /// <summary>
        /// Adds the plugins.
        /// </summary>
        /// <param name="visualStudioService">The visual studio service.</param>
        /// <param name="plugins">The plugins.</param>
        /// <param name="viewModelName">Name of the view model.</param>
        /// <param name="createUnitTests">if set to <c>true</c> [create unit tests].</param>
        /// <param name="useNuget">if set to <c>true</c> [use nuget].</param>
        /// <returns>The messages.</returns>
        IEnumerable<string> AddPlugins(
            IVisualStudioService visualStudioService, 
            IEnumerable<Plugin> plugins, 
            string viewModelName,
            bool createUnitTests,
            bool useNuget);

        /// <summary>
        /// Adds the project plugins.
        /// </summary>
        /// <param name="projectService">The project service.</param>
        /// <param name="plugins">The plugins.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <param name="extensionName">Name of the extension.</param>
        /// <param name="addTemplates">if set to <c>true</c> [add templates].</param>
        /// <returns>The added plugins.</returns>
        IEnumerable<Plugin> AddProjectPlugins(
            IProjectService projectService,
            IEnumerable<Plugin> plugins, 
            string folderName, 
            string extensionName,
            bool addTemplates);
    }
}

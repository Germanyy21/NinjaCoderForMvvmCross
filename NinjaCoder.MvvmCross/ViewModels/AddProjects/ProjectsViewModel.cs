﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the ProjectsViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.ViewModels.AddProjects
{
    using Factories.Interfaces;
    using Scorchio.Infrastructure.Extensions;
    using Scorchio.Infrastructure.Services;
    using Scorchio.Infrastructure.Wpf;
    using Scorchio.Infrastructure.Wpf.ViewModels;
    using Scorchio.Infrastructure.Wpf.ViewModels.Wizard;
    using Scorchio.VisualStudio.Entities;
    using Scorchio.VisualStudio.Services;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Windows.Input;

    /// <summary>
    /// Defines the ProjectsViewModel type.
    /// </summary>
    public class ProjectsViewModel : BaseWizardStepViewModel
    {
        /// <summary>
        /// Gets or sets the visual studio service.
        /// </summary>
        private readonly IVisualStudioService visualStudioService;

        /// <summary>
        /// The settings service.
        /// </summary>
        private readonly ISettingsService settingsService;

        /// <summary>
        /// The file system.
        /// </summary>
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// The project factory.
        /// </summary>
        private readonly IProjectFactory projectFactory;

        /// <summary>
        /// The message box service.
        /// </summary>
        private readonly IMessageBoxService messageBoxService;

        /// <summary>
        /// The folder browser dialog service.
        /// </summary>
        private readonly IFolderBrowserDialogService folderBrowserDialogService;

        /// <summary>
        /// The caching service.
        /// </summary>
        private readonly ICachingService cachingService;

        /// <summary>
        /// The solution already created.
        /// </summary>
        private readonly bool solutionAlreadyCreated;

        /// <summary>
        /// The projects.
        /// </summary>
        private ObservableCollection<SelectableItemViewModel<ProjectTemplateInfo>> projects;

        /// <summary>
        /// The path.
        /// </summary>
        private string path;

        /// <summary>
        /// The project.
        /// </summary>
        private string project;

        /// <summary>
        /// The project is focused.
        /// </summary>
        private bool projectIsFocused;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectsViewModel" /> class.
        /// </summary>
        /// <param name="visualStudioService">The visual studio service.</param>
        /// <param name="settingsService">The settings service.</param>
        /// <param name="projectFactory">The project factory.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="messageBoxService">The message box service.</param>
        /// <param name="folderBrowserDialogService">The folder browser dialog service.</param>
        /// <param name="cachingService">The caching service.</param>
        public ProjectsViewModel(
            IVisualStudioService visualStudioService,
            ISettingsService settingsService,
            IProjectFactory projectFactory,
            IFileSystem fileSystem,
            IMessageBoxService messageBoxService,
            IFolderBrowserDialogService folderBrowserDialogService,
            ICachingService cachingService)
        {
            TraceService.WriteLine("ProjectsViewModel::Constructor Start");

            this.visualStudioService = visualStudioService;
            this.settingsService = settingsService;
            this.fileSystem = fileSystem;
            this.projectFactory = projectFactory;
            this.messageBoxService = messageBoxService;
            this.folderBrowserDialogService = folderBrowserDialogService;
            this.cachingService = cachingService;

            this.projects = new ObservableCollection<SelectableItemViewModel<ProjectTemplateInfo>>();

            //// set the defaults!
            this.Project = visualStudioService.GetDefaultProjectName();

            if (string.IsNullOrEmpty(this.Project) 
                && this.settingsService.UseTempProjectName)
            {
                this.Project = "P" + DateTime.Now.ToString("yyMMddHHmm");
            }

            string defaultPath = this.settingsService.DefaultProjectsPath;

            //// if we are already in the solution disable project name and path.
            this.solutionAlreadyCreated = this.visualStudioService.SolutionAlreadyCreated;

            if (this.solutionAlreadyCreated)
            {
                TraceService.WriteLine("Solution already created");

                this.Path = visualStudioService.SolutionService.GetParentDirectoryName();
            }
            else
            {
                this.Path = string.IsNullOrEmpty(defaultPath) == false
                                ? defaultPath
                                : visualStudioService.DTEService.GetDefaultProjectsLocation();
            }

            TraceService.WriteLine("ProjectsViewModel::Constructor End");
        }

        /// <summary>
        /// Gets a value indicating whether [allow updates].
        /// </summary>
        public bool AllowUpdates
        {
            get { return !this.visualStudioService.SolutionAlreadyCreated; }
        }

        /// <summary>
        /// Gets the framework.
        /// </summary>
        public string Framework
        {
            get { return this.settingsService.FrameworkType.GetDescription(); }
        }

        /// <summary>
        /// Gets or sets the Path.
        /// </summary>
        public string Path
        {
            get
            {
                if (string.IsNullOrEmpty(this.path) == false &&
                    this.path.EndsWith(@"\") == false)
                {
                    this.path = $@"{this.path}\";
                }

                return this.path;
            }

            set
            {
                this.SetProperty(ref this.path, value);
            }
        }

        /// <summary>
        /// Gets or sets the Project.
        /// </summary>
        public string Project
        {
            get { return this.project.CaptialiseFirstCharacter(); }
            set { this.SetProperty(ref this.project, value); }
        }

        /// <summary>
        /// Gets the projects.
        /// </summary>
        public ObservableCollection<SelectableItemViewModel<ProjectTemplateInfo>> Projects
        {
            get { return this.projects; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [project is focused].
        /// </summary>
        public bool ProjectIsFocused
        {
            get { return this.projectIsFocused; }
            set { this.SetProperty(ref this.projectIsFocused, value); }
        }

        /// <summary>
        /// Gets the browser folders command.
        /// </summary>
        public ICommand BrowserFoldersCommand
        {
            get { return new RelayCommand(this.BrowserFolders); }
        }

        /// <summary>
        /// Called when [initialize].
        /// </summary>
        public override void OnInitialize()
        {
            this.projects = new ObservableCollection<SelectableItemViewModel<ProjectTemplateInfo>>();

            IEnumerable<ProjectTemplateInfo> projectTemplateInfos = this.projectFactory.GetAllowedProjects(this.settingsService.FrameworkType);

            foreach (SelectableItemViewModel<ProjectTemplateInfo> template in projectTemplateInfos
                .Select(projectTemplateInfo => new SelectableItemViewModel<ProjectTemplateInfo>(
                                        projectTemplateInfo, 
                                        projectTemplateInfo.PreSelected,
                                        projectTemplateInfo.IsEnabled)))
            {
                this.projects.Add(template);
            }
        }

        /// <summary>
        /// Browsers the folders.
        /// </summary>
        public void BrowserFolders()
        {
            bool? returnValue = this.folderBrowserDialogService.ShowDialog();

            if (returnValue.HasValue &&
                returnValue == true)
            {
                this.Path = this.folderBrowserDialogService.SelectedPath;
                this.OnNotify("Path");
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string DisplayName
        {
            get { return "Project Details"; }
        }

        /// <summary>
        /// Determines whether this instance [can move to next page].
        /// </summary>
        /// <returns>True or false.</returns>
        public override bool CanMoveToNextPage()
        {
            if (this.DoesDirectoryAlreadyExist() &&
                this.solutionAlreadyCreated == false)
            {
                this.messageBoxService.Show(
                    Constants.Settings.DirectoryExists,
                    Constants.Settings.ApplicationName);

                this.ProjectIsFocused = true;
                return false;
            }
          
            return true;
        }

        /// <summary>
        /// Called when [save].
        /// </summary>
        public override void OnSave()
        {
            base.OnSave();

            this.settingsService.DefaultProjectsPath = this.Path;

            this.cachingService.Projects = this.GetFormattedRequiredTemplates();

            foreach (SelectableItemViewModel<ProjectTemplateInfo> templateInfo in this.projects)
            {
                if (templateInfo.Item.ProjectSuffix == this.settingsService.CoreTestsProjectSuffix)
                {
                    this.settingsService.AddCoreTestsProject = templateInfo.IsSelected;
                }
                else if (templateInfo.Item.ProjectSuffix == this.settingsService.DroidProjectSuffix)
                {
                    this.settingsService.AddAndroidProject = templateInfo.IsSelected;
                }
                else if (templateInfo.Item.ProjectSuffix == this.settingsService.iOSProjectSuffix)
                {
                    this.settingsService.AddiOSProject = templateInfo.IsSelected;
                }
                else if (templateInfo.Item.ProjectSuffix == this.settingsService.WindowsPhoneProjectSuffix)
                {
                    this.settingsService.AddWindowsPhoneProject = templateInfo.IsSelected;
                }
                else if (templateInfo.Item.ProjectSuffix == this.settingsService.WpfProjectSuffix)
                {
                    this.settingsService.AddWpfProject = templateInfo.IsSelected;
                }
                else if (templateInfo.Item.ProjectSuffix == this.settingsService.XamarinFormsTestsProjectSuffix)
                {
                    this.settingsService.AddXamarinFormsTestsProject = templateInfo.IsSelected;
                }
                else if (templateInfo.Item.ProjectSuffix == this.settingsService.WindowsUniversalProjectSuffix)
                {
                    this.settingsService.AddWindowsUniversalProject = templateInfo.IsSelected;
                }
            }
        }

        /// <summary>
        /// Gets the solution path.
        /// </summary>
        /// <returns>The solution path.</returns>
        public string GetSolutionPath()
        {
            return $@"{this.Path}{this.Project}";
        }

        /// <summary>
        /// Gets the formatted required templates.
        /// </summary>
        /// <returns>Formatted required templates</returns>
        public IEnumerable<ProjectTemplateInfo> GetFormattedRequiredTemplates()
        {
            List<ProjectTemplateInfo> templateInfos = new List<ProjectTemplateInfo>();

            foreach (SelectableItemViewModel<ProjectTemplateInfo> templateInfo in this.projects
                .Where(x => x.IsSelected))
            {
                this.GetRequiredTemplate(templateInfo.Item);
                templateInfos.Add(templateInfo.Item);
            }

            return templateInfos;
        }

        /// <summary>
        /// Does the directory already exist.
        /// </summary>
        /// <returns>True or false.</returns>
        internal bool DoesDirectoryAlreadyExist()
        {
            string solutionsPath = this.GetSolutionPath();

            return this.fileSystem.Directory.Exists(solutionsPath);
        }

        /// <summary>
        /// Gets the required template.
        /// </summary>
        /// <param name="projectInfo">The project info.</param>
        internal void GetRequiredTemplate(ProjectTemplateInfo projectInfo)
        {
            projectInfo.Name = this.Project + projectInfo.ProjectSuffix;
        }
    }
}

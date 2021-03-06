﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//  Defines the ITextTemplatingService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.Services.Interfaces
{
    using Scorchio.VisualStudio.Entities;
    using System.Collections.Generic;

    /// <summary>
    ///  Defines the ITextTemplatingService type.
    /// </summary>
    public interface ITextTemplatingService
    {
        /// <summary>
        /// Adds the text templates.
        /// </summary>
        /// <param name="statusBarMessage">The status bar message.</param>
        /// <param name="textTemplates">The text templates.</param>
        /// <returns>A list of strings.</returns>
        IEnumerable<string> AddTextTemplates(
            string statusBarMessage,
            IEnumerable<TextTemplateInfo> textTemplates);
    }
}

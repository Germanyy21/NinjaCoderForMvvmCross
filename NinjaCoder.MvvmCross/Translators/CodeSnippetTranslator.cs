﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the SnippetTranslator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace NinjaCoder.MvvmCross.Translators
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using Constants;

    using Scorchio.Infrastructure.Translators;
    using Scorchio.VisualStudio.Entities;

    /// <summary>
    /// Defines the SnippetTranslator type.
    /// </summary>
    public class CodeSnippetTranslator : ITranslator<string, CodeSnippet>
    {
        /// <summary>
        /// Translates the specified from.
        /// </summary>
        /// <param name="from">From object.</param>
        /// <returns>A CodeSnippet.</returns>
        public CodeSnippet Translate(string @from)
        {
            XDocument doc = XDocument.Load(from);

            if (doc.Root != null)
            {
                XmlReader xmlReader = doc.Root.CreateReader();

                XmlSerializer serializer = new XmlSerializer(typeof(CodeSnippet));
                CodeSnippet codeSnippet = (CodeSnippet)serializer.Deserialize(xmlReader);
                return this.CleanedUp(codeSnippet);
            }

            return null;
        }
        
        /// <summary>
        /// Cleaned up.
        /// </summary>
        /// <param name="codeSnippet">The code snippet.</param>
        /// <returns>A cleaned up version of the CodeSnippet</returns>
        internal CodeSnippet CleanedUp(CodeSnippet codeSnippet)
        {
            //// we have problems with CR and LF in the code snippets
            //// here we try and fix them!!!
            
            //// replace all the single \n with a null!
            Regex regEx = new Regex(Settings.ReplaceBlankLinesRegEx);

            string code = regEx.Replace(codeSnippet.Code, Environment.NewLine).TrimEnd();

            //// get rid of tabs
            code = code.Replace("\t", "    ");
            
            codeSnippet.Code = code;

            if (codeSnippet.UsingStatements != null)
            {
                //// sometimes people might put the ; in the statement - cater for it!
                for (int i = 0; i < codeSnippet.UsingStatements.Count; i++)
                {
                    codeSnippet.UsingStatements[i] = this.CleanUsingStatement(codeSnippet.UsingStatements[i]);
                }
            }

            return codeSnippet;
        }

        /// <summary>
        /// Cleans the using statement.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <returns>The cleaned using statement.</returns>
        internal string CleanUsingStatement(string statement)
        {
            return statement.Replace(";", string.Empty);
        }

        /// <summary>
        /// Cleans the code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>The cleaned up code.</returns>
        internal string CleanedCode(string code)
        {
            if (code != null)
            {
                code = code.Replace("\n ", string.Empty);
                code = code.Trim();
            }

            return code;
        }
    }
}

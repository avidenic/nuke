﻿// Copyright 2018 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace Nuke.Common.ProjectModel
{
    [PublicAPI]
    public static class ProjectExtensions
    {
        public static Microsoft.Build.Evaluation.Project GetMSBuildProject(
            this Project project,
            string configuration = null,
            string targetFramework = null)
        {
            return ProjectModelTasks.ParseProject(project.Path, configuration, targetFramework);
        }

        [CanBeNull]
        public static string GetProperty(this Project project, string propertyName)
        {
            var property = project.GetMSBuildProject().GetProperty(propertyName);
            return property?.EvaluatedValue;
        }
        
        [CanBeNull]
        public static T GetProperty<T>(this Project project, string propertyName)
        {
            return Convert<T>(project.GetProperty(propertyName));
        }

        public static IEnumerable<string> GetItems(this Project project, string itemGroupName)
        {
            var items = project.GetMSBuildProject().GetItems(itemGroupName);
            return items.Select(x => x.EvaluatedInclude);
        }

        public static IEnumerable<T> GetItems<T>(this Project project, string itemGroupName)
        {
            return project.GetItems(itemGroupName).Select(Convert<T>);
        }

        public static IEnumerable<string> GetItemMetadata(this Project project, string itemGroupName, string metadataName)
        {
            var items = project.GetMSBuildProject().GetItems(itemGroupName);
            return items.Select(x => x.GetMetadataValue(metadataName));
        }

        public static IEnumerable<T> GetItemMetadata<T>(this Project project, string itemGroupName, string metadataName)
        {
            return project.GetItemMetadata(itemGroupName, metadataName).Select(Convert<T>);
        }
        
        public static IReadOnlyCollection<string> GetTargetFrameworks(this Project project)
        {
            var msbuildProject = project.GetMSBuildProject();
            var targetFrameworkProperty = msbuildProject.GetProperty("TargetFramework");
            if (targetFrameworkProperty != null)
                return new[]{ targetFrameworkProperty.EvaluatedValue };
            
            var targetFrameworksProperty = msbuildProject.GetProperty("TargetFrameworks");
            if (targetFrameworksProperty != null)
                return targetFrameworksProperty.EvaluatedValue.Split(';');

            return new string[0];
        }
        
        [CanBeNull]
        private static T Convert<T>(string value)
        {
            try
            {
                var typeConverter = TypeDescriptor.GetConverter(typeof(T));
                return (T)typeConverter.ConvertFromInvariantString(value);
            }
            catch
            {
                ControlFlow.Fail($"Value '{value}' could not be converted to '{typeof(T).Name}'.");
                // ReSharper disable once HeuristicUnreachableCode
                return default;
            }
        }
    }
}

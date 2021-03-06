﻿using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleTemplateRenderer.Console
{
    internal static class Program
    {
        internal static Task<int> Main(string[] args)
        {
            var directoryCommand = new Command("dir", "Recursively render template content starting in a specified directory.")
            {
                new Argument<DirectoryInfo>("template", "The directory of the template content."),
                new Argument<DirectoryInfo>("output", "The directory where the rendered content should be placed."),
                new Argument<FileInfo>("variables", "The path to a JSON file with a flat object of template variables and their values."),
                new Option<FileInfo>("--gitignore", "The path to an optional gitignore file to filter out content when performing the operation."),
                new Option<bool>("--transformFilePath", "Apply transform variables to file paths.")
            };
            directoryCommand.Handler = CommandHandler.Create<DirectoryInfo, DirectoryInfo, FileInfo, FileInfo, bool>(RenderTemplateDirectory);

            var fileCommand = new Command("file", "Render a template file.")
            {
                new Argument<FileInfo>("template", "The file path to the template file."),
                new Argument<FileInfo>("output", "The file path where the rendered content should be placed."),
                new Argument<FileInfo>("variables", "The path to a JSON file with a flat object of template variables and their values."),
                new Option<bool>("--transformFilePath", "Apply transform variables to file paths.")
            };
            fileCommand.Handler = CommandHandler.Create<FileInfo, FileInfo, FileInfo, bool>(RenderTemplateFile);

            var rootCommand = new RootCommand
            {
                directoryCommand,
                fileCommand
            };

            rootCommand.Description = @"A simple template rendering tool that takes a JSON file 
with a list of key/value pairs, a target file or directory, 
and an output file or directory. It goes through the indicated file(s), 
performs a find/replace on the file contents, 
and writes the new file(s) to the specified output location. 
Optionally, it can also transform the file paths.";

            return rootCommand.InvokeAsync(args);
        }

        private static void RenderTemplateFile(
            FileInfo template, 
            FileInfo output, 
            FileInfo variables, 
            bool transformFilePath)
        {
            // TODO: Figure out how the CommandLine models can handle this parsing step...
            var templateVariables = JsonSerializer
                   .Deserialize<Dictionary<string, string>>(
                       File.ReadAllText(variables.FullName));

            var args = new FileRenderArgs
            {
                TemplateFile = template,
                OutputFile = output,
                TemplateVariables = templateVariables,
                TransformFilePath = transformFilePath
            };

            FileTemplateRenderer.RenderFileFromTemplate(args);
        }

        private static void RenderTemplateDirectory(
            DirectoryInfo template, 
            DirectoryInfo output, 
            FileInfo variables, 
            FileInfo gitignore, 
            bool transformFilePath)
        {
            // TODO: Figure out how the CommandLine models can handle this parsing step...
            var templateVariables = JsonSerializer
                   .Deserialize<Dictionary<string, string>>(
                       File.ReadAllText(variables.FullName));

            var args = new DirectoryRenderArgs
            {
                TemplateDirectory = template,
                OutputDirectory = output,
                TemplateVariables = templateVariables,
                Gitignore = gitignore,
                TransformFilePath = transformFilePath
            };

            DirectoryTemplateRenderer.Render(args);
        }
    }
}

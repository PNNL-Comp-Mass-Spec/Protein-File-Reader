using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace ProteinReader_UnitTests
{
    internal static class FileRefs
    {
        public const string SHARE_PATH = @"\\proto-2\unitTest_Files\Protein_File_Reader\";

        public static FileInfo GetTestFile(string relativeFilePath)
        {
            var dataFile = new FileInfo(relativeFilePath);

            if (dataFile.Exists)
            {
#if DEBUG
                Console.WriteLine("Found file " + dataFile.FullName);
                Console.WriteLine();
#endif
                return dataFile;
            }

#if DEBUG
            Console.WriteLine("Could not find " + relativeFilePath);
            Console.WriteLine("Checking alternative locations");
#endif

            var relativePathsToCheck = new List<string>
            {
                dataFile.Name
            };

            if (!Path.IsPathRooted(relativeFilePath) &&
                (relativeFilePath.Contains(Path.DirectorySeparatorChar) || relativeFilePath.Contains(Path.AltDirectorySeparatorChar)))
            {
                relativePathsToCheck.Add(relativeFilePath);
            }

            if (dataFile.Directory != null)
            {
                var parentToCheck = dataFile.Directory.Parent;

                while (parentToCheck != null)
                {
                    foreach (var relativePath in relativePathsToCheck)
                    {
                        var alternateFile = new FileInfo(Path.Combine(parentToCheck.FullName, relativePath));

                        if (alternateFile.Exists)
                        {
#if DEBUG
                            Console.WriteLine("... found at " + alternateFile.FullName);
                            Console.WriteLine();
#endif
                            return alternateFile;
                        }
                    }

                    parentToCheck = parentToCheck.Parent;
                }
            }

            foreach (var relativePath in relativePathsToCheck)
            {
                var serverPathFile = new FileInfo(Path.Combine(SHARE_PATH, relativePath));

                if (serverPathFile.Exists)
                {
#if DEBUG
                    Console.WriteLine("... found at " + serverPathFile);
                    Console.WriteLine();
#endif
                    return serverPathFile;
                }
            }

            var currentDirectory = new DirectoryInfo(".");

            Assert.Fail("Could not find " + relativeFilePath + "; current working directory: " + currentDirectory.FullName);
            return null;
        }
    }
}

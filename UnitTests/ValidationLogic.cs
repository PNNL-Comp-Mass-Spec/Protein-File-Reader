using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProteinFileReader;

namespace ProteinReader_UnitTests
{
    internal static class ValidationLogic
    {
        public static void CheckProteinStats(ProteinFileReaderBaseClass reader, int proteinCountExpected, int totalResidueCountExpected)
        {
            var proteins = new List<string>();
            var totalResidueCount = 0;

            while (reader.ReadNextProteinEntry())
            {
                proteins.Add(reader.ProteinName);
                totalResidueCount += reader.ProteinSequence.Length;
            }

            Console.WriteLine("Proteins loaded: " + proteins.Count);
            Console.WriteLine("Total residue count: " + totalResidueCount);

            Assert.AreEqual(proteinCountExpected, proteins.Count);
            Assert.AreEqual(totalResidueCountExpected, totalResidueCount);
        }

        /// <summary>
        /// Verify that the file starts with and ends with the expected proteins
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expectedFirstProteinNamesOrSequences">Comma separated list of protein names or sequences that should be at the start of the file</param>
        /// <param name="expectedLastProteinNamesOrSequences">Comma separated list of protein names or sequences that should be at the end of the file</param>
        /// <param name="checkName">When true, checking protein names; when false, checking protein sequences</param>
        public static void CheckProteinNamesOrSequences(
            ProteinFileReaderBaseClass reader,
            string expectedFirstProteinNamesOrSequences,
            string expectedLastProteinNamesOrSequences,
            bool checkName = true)
        {
            var expectedFirstNamesOrSequences = expectedFirstProteinNamesOrSequences.Split(',').ToList();
            var expectedLastNamesOrSequences = expectedLastProteinNamesOrSequences.Split(',').ToList();

            var firstItems = new List<string>();
            var lastItems = new Queue<string>();

            while (reader.ReadNextProteinEntry())
            {
                string nameOrSequence;
                if (checkName)
                {
                    nameOrSequence= reader.ProteinName;
                }
                else
                {
                    nameOrSequence= reader.ProteinSequence;
                }

                lastItems.Enqueue(nameOrSequence);

                if (lastItems.Count > expectedLastNamesOrSequences.Count)
                    lastItems.Dequeue();

                if (firstItems.Count >= expectedFirstNamesOrSequences.Count)
                    continue;

                firstItems.Add(nameOrSequence);
            }

            string itemDescription;
            if (checkName)
                itemDescription = expectedFirstNamesOrSequences.Count > 1 ? "proteins" : "protein";
            else
                itemDescription = expectedFirstNamesOrSequences.Count > 1 ? "sequences" : "sequence";

            Console.WriteLine("First {0} {1}", expectedFirstNamesOrSequences.Count, itemDescription);
            for (var i = 0; i < expectedFirstNamesOrSequences.Count; i++)
            {
                var nameOrSequence = firstItems[i];
                Console.WriteLine(nameOrSequence);
                Assert.AreEqual(expectedFirstNamesOrSequences[i], nameOrSequence);
            }

            Console.WriteLine();
            Console.WriteLine("Last {0} {1}", expectedLastNamesOrSequences.Count, itemDescription);
            foreach (var expectedProtein in expectedLastNamesOrSequences)
            {
                if (string.IsNullOrWhiteSpace(expectedProtein))
                    continue;

                var nameOrSequence = lastItems.Dequeue();
                Console.WriteLine(nameOrSequence);
                Assert.AreEqual(expectedProtein, nameOrSequence);
            }
        }

        /// <summary>
        /// Verify that the file contains the given protein names and descriptions (anywhere in the file)
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="proteinNames">Delimited list of protein names</param>
        /// <param name="proteinDescriptions">Delimited list of protein descriptions</param>
        /// <param name="delimiter">Delimiter to use</param>
        public static void CheckProteinDescription(ProteinFileReaderBaseClass reader, string proteinNames, string proteinDescriptions, char delimiter)
        {
            var proteinsToFind = new Dictionary<string, string>();

            var proteinNameList = proteinNames.Split(delimiter);
            var proteinDescriptionList = proteinDescriptions.Split(delimiter);

            Assert.AreEqual(proteinNameList.Length, proteinDescriptionList.Length);

            for (var i = 0; i < proteinNameList.Length; i++)
            {
                proteinsToFind.Add(proteinNameList[i], proteinDescriptionList[i]);
            }

            var proteinsInFile = new Dictionary<string, string>();

            while (reader.ReadNextProteinEntry())
            {
                if (!proteinsToFind.ContainsKey(reader.ProteinName))
                    continue;

                proteinsInFile.Add(reader.ProteinName, reader.ProteinDescription);
            }

            foreach (var expectedProtein in proteinsToFind)
            {
                var proteinName = expectedProtein.Key;
                var proteinDescription = expectedProtein.Value;

                if (!proteinsInFile.TryGetValue(proteinName, out var description))
                    Assert.Fail("Expected protein not found: " + expectedProtein);

                Console.WriteLine(proteinName + ": " + description);
                Assert.AreEqual(proteinDescription, description);
            }
        }
    }
}

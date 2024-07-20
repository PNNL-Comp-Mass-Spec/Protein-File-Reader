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

            if (proteinCountExpected == 0 && totalResidueCountExpected == 0)
                return;

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
        /// <param name="delimiter">Delimiter for expected protein names or sequences</param>
        public static void CheckProteinNamesOrSequences(
            ProteinFileReaderBaseClass reader,
            string expectedFirstProteinNamesOrSequences,
            string expectedLastProteinNamesOrSequences,
            bool checkName,
            char delimiter = ',')
        {
            var expectedFirstNamesOrSequences = expectedFirstProteinNamesOrSequences.Split(delimiter).ToList();
            var expectedLastNamesOrSequences = expectedLastProteinNamesOrSequences.Split(delimiter).ToList();

            if (string.IsNullOrWhiteSpace(expectedFirstProteinNamesOrSequences) && string.IsNullOrWhiteSpace(expectedLastProteinNamesOrSequences))
            {
                expectedFirstNamesOrSequences.Clear();
                expectedLastNamesOrSequences.Clear();
            }

            var firstItems = new List<string>();
            var lastItems = new Queue<string>();

            var firstItemsToStore = expectedFirstNamesOrSequences.Count == 0 ? 5 : expectedFirstNamesOrSequences.Count;
            var lastItemsToStore = expectedLastNamesOrSequences.Count == 0 ? 5 : expectedLastNamesOrSequences.Count;

            while (reader.ReadNextProteinEntry())
            {
                string nameOrSequence;

                if (checkName)
                {
                    nameOrSequence = reader.ProteinName;
                }
                else
                {
                    nameOrSequence = reader.ProteinSequence;
                }

                lastItems.Enqueue(nameOrSequence);

                if (lastItems.Count > lastItemsToStore)
                    lastItems.Dequeue();

                if (firstItems.Count >= firstItemsToStore)
                    continue;

                firstItems.Add(nameOrSequence);
            }

            if (expectedFirstNamesOrSequences.Count == 0 && expectedLastNamesOrSequences.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("First {0} proteins", 5);
                foreach (var nameOrSequence in firstItems)
                {
                    Console.WriteLine(nameOrSequence);
                }

                Console.WriteLine();
                Console.WriteLine("Last {0} proteins", 5);
                foreach (var nameOrSequence in lastItems)
                {
                    Console.WriteLine(nameOrSequence);
                }

                return;
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

            var proteinNameList = proteinNames.Split(delimiter).ToList();
            var proteinDescriptionList = proteinDescriptions.Split(delimiter).ToList();

            if (string.IsNullOrWhiteSpace(proteinNames) && string.IsNullOrWhiteSpace(proteinDescriptions))
            {
                proteinNameList.Clear();
                proteinDescriptionList.Clear();
            }

            Assert.AreEqual(proteinNameList.Count, proteinDescriptionList.Count);

            for (var i = 0; i < proteinNameList.Count; i++)
            {
                proteinsToFind.Add(proteinNameList[i], proteinDescriptionList[i]);
            }

            var matchedProteinsInFile = new Dictionary<string, string>();
            var proteinsRead = 0;

            while (reader.ReadNextProteinEntry())
            {
                proteinsRead++;

                if (!proteinsToFind.ContainsKey(reader.ProteinName))
                    continue;

                matchedProteinsInFile.Add(reader.ProteinName, reader.ProteinDescription);
            }

            Console.WriteLine("Read {0} proteins", proteinsRead);
            Console.WriteLine();

            if (proteinsToFind.Count == 0)
            {
                Console.WriteLine("Name verification has been skipped since proteinsToFind is empty");
                return;
            }

            foreach (var expectedProtein in proteinsToFind)
            {
                var proteinName = expectedProtein.Key;
                var proteinDescription = expectedProtein.Value;

                if (!matchedProteinsInFile.TryGetValue(proteinName, out var description))
                    Assert.Fail("Expected protein not found: " + expectedProtein);

                Console.WriteLine(proteinName + ": " + description);
                Assert.AreEqual(proteinDescription, description);
            }
        }
    }
}

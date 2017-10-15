using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProteinFileReader;

namespace ProteinReader_UnitTests
{
    internal class ValidationLogic
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

        public static void CheckProteinNames(ProteinFileReaderBaseClass reader, string expectedFirstProteinNames, string expectedLastProteinNames)
        {

            var proteins = new List<string>();

            while (reader.ReadNextProteinEntry())
            {
                proteins.Add(reader.ProteinName);
            }

            var expectedFirstProteins = expectedFirstProteinNames.Split(',').ToList();
            var expectedLastProteins = expectedLastProteinNames.Split(',').ToList();

            Console.WriteLine("First {0} proteins", expectedFirstProteins.Count);
            for (var i = 0; i < expectedFirstProteins.Count; i++)
            {
                var proteinName = proteins[i];
                Console.WriteLine(proteinName);
                Assert.AreEqual(expectedFirstProteins[i], proteinName);
            }

            Console.WriteLine();
            Console.WriteLine("Last {0} proteins", expectedLastProteins.Count);
            for (var i = 0; i < expectedLastProteins.Count; i++)
            {
                var proteinName = proteins[proteins.Count - i - 1];
                Console.WriteLine(proteinName);
                Assert.AreEqual(expectedLastProteins[i], proteinName);
            }
        }

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

            var proteins = new Dictionary<string, string>();

            while (reader.ReadNextProteinEntry())
            {
                if (!proteinsToFind.ContainsKey(reader.ProteinName))
                    continue;

                proteins.Add(reader.ProteinName, reader.ProteinDescription);
            }

            foreach (var expectedProtein in proteinsToFind)
            {
                if (!proteins.TryGetValue(expectedProtein.Key, out var description))
                    Assert.Fail("Expected protein not found: " + expectedProtein);

                Console.WriteLine(expectedProtein.Key + ": " + description);
                Assert.AreEqual(expectedProtein.Value, description);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace ProteinReader_UnitTests
{
    [TestFixture]
    public class FastaFileTests
    {
        // ReSharper disable CommentTypo

        // Ignore Spelling: Promega, neuregulin, isoform, coli, UniProt, sp
        // Ignore Spelling: iminobutanoate, iminopropanoate, deaminase, acetyltransferase

        // ReSharper restore CommentTypo

        [Test]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta", 4437, 1357830)]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta.gz", 4437, 1357830)]
        [TestCase(@"Test_Data\JunkTest.fasta", 26, 7429)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta", 16, 4766)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta", 15, 41451)]
        public void CheckProteinStats(string fastaFile, int proteinCountExpected, int totalResidueCountExpected)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta.gz",
            "sp|P0AF93|RIDA_ECOLI,sp|P0A944|RIMI_ECOLI,sp|P0A7S9|RS13_ECOLI,sp|P77489|PAOC_ECOLI,sp|P0ABF1|PCNB_ECOLI",
            "sp|P0ACC1|PRMC_ECOLI,sp|P77272|PTYBC_ECOLI,sp|P0ACQ0|RBSR_ECOLI,sp|P0A8A8|RIMP_ECOLI,sp|P76104|RLHA_ECOLI")]
        [TestCase(@"Test_Data\JunkTest.fasta",
            "TOM5,TOM5,,,TOM5,TOM7,TOM22",
            "IPI:IPI00177321.1|REFSEQ_XP:XP_168060,IPI:IPI00043226.5,NCU10043.1")]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta",
            "Contaminant_TRYP_PIG,Contaminant_Trypa1,Contaminant_Trypa2",
            "Contaminant_K22E_HUMAN,Contaminant_K1C9_HUMAN,Contaminant_K1C10_HUMAN")]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta",
            "1433B_HUMAN,1433E_HUMAN,68MP_HUMAN",
            "TKNK_HUMAN,ZEP2_HUMAN,ZSCA4_HUMAN")]
        // ReSharper restore StringLiteralTypo
        public void CheckProteinNames(string fastaFile, string expectedFirstProteinNames, string expectedLastProteinNames)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            ValidationLogic.CheckProteinNamesOrSequences(reader, expectedFirstProteinNames, expectedLastProteinNames);
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta",
            "sp|P0AF93|RIDA_ECOLI,sp|P0A944|RIMI_ECOLI,sp|P0A7S9|RS13_ECOLI",
            "sp|P0ACQ0|RBSR_ECOLI,sp|P0A8A8|RIMP_ECOLI,sp|P76104|RLHA_ECOLI")]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta.gz",
            "sp|P0AF93|RIDA_ECOLI,sp|P0A944|RIMI_ECOLI,sp|P0A7S9|RS13_ECOLI",
            "sp|P0ACQ0|RBSR_ECOLI,sp|P0A8A8|RIMP_ECOLI,sp|P76104|RLHA_ECOLI")]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta",
            "Contaminant_TRYP_PIG,Contaminant_Trypa1,Contaminant_Trypa2",
            "Contaminant_K22E_HUMAN,Contaminant_K1C9_HUMAN,Contaminant_K1C10_HUMAN")]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta",
            "1433B_HUMAN,1433E_HUMAN,68MP_HUMAN",
            "TKNK_HUMAN,ZEP2_HUMAN,ZSCA4_HUMAN")]
        // ReSharper restore StringLiteralTypo
        public void CheckProteinNamesParameterlessConstructor(string fastaFile, string expectedFirstProteinNames, string expectedLastProteinNames)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader();
            reader.OpenFile(dataFile.FullName);

            ValidationLogic.CheckProteinNamesOrSequences(reader, expectedFirstProteinNames, expectedLastProteinNames);
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta.gz",
            "sp|P0AF93|RIDA_ECOLI,sp|P0A944|RIMI_ECOLI",
            "2-iminobutanoate/2-iminopropanoate deaminase OS=Escherichia coli (strain K12) OX=83333 GN=ridA PE=1 SV=2,[Ribosomal protein S18]-alanine N-acetyltransferase OS=Escherichia coli (strain K12) OX=83333 GN=rimI PE=1 SV=1",
            ',')]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta",
            "Contaminant_Trypa1|Contaminant_CTRB_BOVIN",
            "VATVSLPR-like Promega trypsin artifact 1 (871.1) xATVSLPR (PromTArt1)|CHYMOTRYPSINOGEN B (EC 3.4.21.1). - BOS TAURUS (BOVINE).",
            '|')]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta",
            "1433E_HUMAN|NRG4_HUMAN",
            "14-3-3 protein epsilon; Short=14-3-3E; Accession=Q7M4R4|Pro-neuregulin-4, membrane-bound isoform; Short=Pro-NRG4; Contains: Neuregulin-4; Short=NRG-4; Accession=Q8WWG1; A6NIE8",
            '|')]
        // ReSharper restore StringLiteralTypo
        public void CheckProteinDescription(string fastaFile, string proteinNames, string proteinDescriptions, char delimiter)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            ValidationLogic.CheckProteinDescription(reader, proteinNames, proteinDescriptions, delimiter);
        }

        [Test]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta", 250, 5.75)]
        [TestCase(@"Test_Data\E_coli_K12_UniProt_2020-10-19.fasta.gz", 250, 5.58)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta", 5, 30.23)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta", 2, 14.15)]
        public void TestPercentFileProcessed(string fastaFile, int progressIntervalProteins, double expectedAverageProgressPercentInterval)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            var proteinsRead = 0;
            var lastProgress = 0f;
            var progressPercentIntervals = new List<double>();

            if (progressIntervalProteins < 1)
                progressIntervalProteins = 1;

            while (reader.ReadNextProteinEntry())
            {
                proteinsRead++;
                if (proteinsRead % progressIntervalProteins > 0)
                    continue;

                var percentComplete = reader.PercentFileProcessed();
                Console.WriteLine("{0:F1}% read", percentComplete);

                var progressInterval = percentComplete - lastProgress;
                progressPercentIntervals.Add(progressInterval);

                lastProgress = percentComplete;
            }

            Console.WriteLine("{0:F1}% read at finish", reader.PercentFileProcessed());
            Console.WriteLine();

            var averageInterval = progressPercentIntervals.Average();
            Console.WriteLine("Average progress interval: {0:F2}%", averageInterval);
            if (expectedAverageProgressPercentInterval > 0)
                Assert.AreEqual(expectedAverageProgressPercentInterval, averageInterval, 0.01);
        }
    }
}

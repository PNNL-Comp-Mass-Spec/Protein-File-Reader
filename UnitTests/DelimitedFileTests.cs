using NUnit.Framework;
using ProteinFileReader;

namespace ProteinReader_UnitTests
{
    public class DelimitedFileTests
    {
        [Test]
        [TestCase(@"Test_Data\JunkTest.txt", 26, 7427)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt", 16, 4766)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt", 15, 41451)]
        public void CheckProteinStats(string fastaFile, int proteinCountExpected, int totalResidueCountExpected)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new DelimitedFileReader(dataFile.FullName) {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);
        }

        [Test]
        [TestCase(@"Test_Data\JunkTest.txt",
            "TOM5,TOM5,",
            "NCU10043.1,IPI:IPI00043226.5,IPI:IPI00177321.1|REFSEQ_XP:XP_168060")]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt",
            "Contaminant_TRYP_PIG,Contaminant_Trypa1,Contaminant_Trypa2",
            "Contaminant_K1C10_HUMAN,Contaminant_K1C9_HUMAN,Contaminant_K22E_HUMAN")]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt",
            "1433B_HUMAN,1433E_HUMAN,68MP_HUMAN",
            "ZSCA4_HUMAN,ZEP2_HUMAN,TKNK_HUMAN")]
        public void CheckProteinNames(string fastaFile, string expectedFirstProteinNames, string expectedLastProteinNames)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new DelimitedFileReader(dataFile.FullName)
            {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinNames(reader, expectedFirstProteinNames, expectedLastProteinNames);
        }

        [Test]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt",
            "Contaminant_Trypa1|Contaminant_CTRB_BOVIN",
            "VATVSLPR-like Promega trypsin artifact 1 (871.1) xATVSLPR (PromTArt1)|CHYMOTRYPSINOGEN B (EC 3.4.21.1). - BOS TAURUS (BOVINE).",
            '|')]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt",
            "1433E_HUMAN|NRG4_HUMAN",
            "14-3-3 protein epsilon; Short=14-3-3E; Accession=Q7M4R4|Pro-neuregulin-4, membrane-bound isoform; Short=Pro-NRG4; Contains: Neuregulin-4; Short=NRG-4; Accession=Q8WWG1; A6NIE8",
            '|')]
        public void CheckProteinDescription(string fastaFile, string proteinNames, string proteinDescriptions, char delimiter)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new DelimitedFileReader(dataFile.FullName)
            {
                SkipFirstLine = false
            };

            ValidationLogic.CheckProteinDescription(reader, proteinNames, proteinDescriptions, delimiter);

        }

        [Test]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_SequenceOnly.txt", 10, 3988, DelimitedFileReader.eDelimitedFileFormatCode.SequenceOnly)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequence.txt", 9, 3859, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt", 15, 41451, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_UniqueIDSequence.txt", 9, 3859, DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceID.txt", 14, 7101, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceIDMassNET.txt", 11, 4101, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceIDMassNETPlusStDevAndDiscriminant.txt", 10, 3988, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_IDSequenceMassNET.txt", 10, 3988, DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameDescriptionHashSequence.txt", 14, 7101, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Hash_Sequence)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov_with_extra.txt", 16, 4766, DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Hash_Sequence)]
        public void CheckDelimitedFileFormats(
            string fastaFile,
            int proteinCountExpected, int totalResidueCountExpected,
            DelimitedFileReader.eDelimitedFileFormatCode formatCode)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new DelimitedFileReader(dataFile.FullName, formatCode)
            {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);

        }
    }
}

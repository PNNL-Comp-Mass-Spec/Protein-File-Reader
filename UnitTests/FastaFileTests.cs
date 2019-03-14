using NUnit.Framework;

namespace ProteinReader_UnitTests
{
    [TestFixture]
    public class FastaFileTests
    {
        [Test]
        [TestCase(@"Test_Data\JunkTest.fasta", 26, 7429)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta", 16, 4766)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta", 15, 41451)]
        public void CheckProteinStats(string fastaFile, int proteinCountExpected, int totalResidueCountExpected)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);
        }

        [Test]
        [TestCase(@"Test_Data\JunkTest.fasta",
            "TOM5,TOM5,",
            "NCU10043.1,IPI:IPI00043226.5,IPI:IPI00177321.1|REFSEQ_XP:XP_168060")]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta",
            "Contaminant_TRYP_PIG,Contaminant_Trypa1,Contaminant_Trypa2",
            "Contaminant_K1C10_HUMAN,Contaminant_K1C9_HUMAN,Contaminant_K22E_HUMAN")]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta",
            "1433B_HUMAN,1433E_HUMAN,68MP_HUMAN",
            "ZSCA4_HUMAN,ZEP2_HUMAN,TKNK_HUMAN")]
        public void CheckProteinNames(string fastaFile, string expectedFirstProteinNames, string expectedLastProteinNames)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            ValidationLogic.CheckProteinNamesOrSequences(reader, expectedFirstProteinNames, expectedLastProteinNames);
        }

        [Test]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.fasta",
            "Contaminant_Trypa1|Contaminant_CTRB_BOVIN",
            "VATVSLPR-like Promega trypsin artifact 1 (871.1) xATVSLPR (PromTArt1)|CHYMOTRYPSINOGEN B (EC 3.4.21.1). - BOS TAURUS (BOVINE).",
            '|')]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.fasta",
            "1433E_HUMAN|NRG4_HUMAN",
            "14-3-3 protein epsilon; Short=14-3-3E; Accession=Q7M4R4|Pro-neuregulin-4, membrane-bound isoform; Short=Pro-NRG4; Contains: Neuregulin-4; Short=NRG-4; Accession=Q8WWG1; A6NIE8",
            '|')]
        public void CheckProteinDescription(string fastaFile, string proteinNames, string proteinDescriptions, char delimiter)
        {
            var dataFile = FileRefs.GetTestFile(fastaFile);

            var reader = new ProteinFileReader.FastaFileReader(dataFile.FullName);

            ValidationLogic.CheckProteinDescription(reader, proteinNames, proteinDescriptions, delimiter);

        }
    }
}

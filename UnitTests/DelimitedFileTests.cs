﻿using System;
using System.IO;
using NUnit.Framework;
using ProteinFileReader;

namespace ProteinReader_UnitTests
{
    public class DelimitedProteinFileTests
    {
        // Ignore Spelling: Bradykinin, des, isoform, neuregulin, Promega

        [Test]
        [TestCase(@"Test_Data\JunkTest.txt", 26, 7427)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt", 16, 4766)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt", 15, 41451)]
        public void CheckProteinStats(string proteinsFile, int proteinCountExpected, int totalResidueCountExpected)
        {
            var dataFile = FileRefs.GetTestFile(proteinsFile);

            var reader = new DelimitedProteinFileReader(dataFile.FullName)
            {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase(@"Test_Data\JunkTest.txt",
            "TOM5,TOM5,,,TOM5,TOM7,TOM22",
            "IPI:IPI00177321.1|REFSEQ_XP:XP_168060,IPI:IPI00043226.5,NCU10043.1")]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt",
            "Contaminant_TRYP_PIG,Contaminant_Trypa1,Contaminant_Trypa2",
            "Contaminant_K22E_HUMAN,Contaminant_K1C9_HUMAN,Contaminant_K1C10_HUMAN")]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt",
            "1433B_HUMAN,1433E_HUMAN,68MP_HUMAN",
            "TKNK_HUMAN,ZEP2_HUMAN,ZSCA4_HUMAN")]
        [TestCase(@"Test_Data\QC_Standards_2004-01-21.txt",
            "K2C1_HUMAN,K22E_HUMAN,K1CI_HUMAN",
            "P010|CYC_BOVIN,P011|MANA_YEAST,P012|PHS2_RABIT")]
        [TestCase(@"Test_Data\QC_Standards_2004-01-21.csv",
            "K2C1_HUMAN;K22E_HUMAN;K1CI_HUMAN;KRHU0;TRYP_PIG;001|Brady2-9;002|des-Pro3,Ala2,6-Bradykinin",
            "P009|OVAL_CHICK;P010|CYC_BOVIN;P011|MANA_YEAST;P012|PHS2_RABIT", ';')]
        // ReSharper restore StringLiteralTypo
        public void CheckProteinNames(string proteinsFile, string expectedFirstProteinNames, string expectedLastProteinNames, char nameDelimiter = ',')
        {
            var dataFile = FileRefs.GetTestFile(proteinsFile);

            var reader = new DelimitedProteinFileReader(dataFile.FullName)
            {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinNamesOrSequences(reader, expectedFirstProteinNames, expectedLastProteinNames, true, nameDelimiter);
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase(@"Test_Data\JunkTest.txt",
            "MFGGFQPPALSREELQAAEAEATFTIQRAVFTAVALYLSPFVIDAVSKVL,MFGGFQPPALSREELQAAEAEAT FTIQRAVFTAVALYLSPFVIDAVSKVL",
            "MHPEPLLNSTQSAPHHFPDSFQATPFCFNQSLIPGSPSNSSILSGSLDYSYSPVQLPSYAPENYNSPASLDTRTCGYPPEDHSYQHLSSHAQYSCFSSATTSICYCASCEAEDLDALQAAEYFYPSTDCVDFAPSAAATSDFYKRETNCDICYS,MTPQDDSVNFDIIESHKENIQALPSGRSARKLAELFSPSHGASSSRQALAPQLTPTPNPTEVKSVNDAIRAEYEAELAAFNPEEQDDPLDIYDRYVRWTLDAYPSASATPQSQLHLLLERATRAFVGSAQYRNDARYLRMWLHYIRMFSDAPREAFVFLSRHQIGEQLALYYEEFAAYLEGEGRWAQAEEVYKMGIEKEARPVSRLVRKFGEFEQRRAALPEGAEDQSNSVPVLPVVRKALGVKSDPFLAAGVERLPRDPQAPRPNMGVGGAAAGPSKPKSKLAIFSDADAAAQQPALGSLSAGSKGWDSIGSLADRKKENTVEPKPWAGEVLKAGGKKPVQKMQVFRDTIKKTTNPMMMNKQQLSQSHIPIHHSQSQVTVNPVSGKRERVFVDLRVIYPTPDEPGTELSFEEIWAARRGWLDVVWEDERKKRPLDAMLTPKRDENMEDLTLEMASTKIVAYHDVLKLDENGKPIYPEHKAGRSAKKNKVIEVNETQIIKAKLDSPSGPKMKKRGSSSEPTMTLHTKAATDDIYDIFNAPLKPTANPSLSDDENRYDSDDYTCGVDDTSKNLATSEAGDETTVVEAIEETDVADEDDDVKSEWSDFTARKHIPDYQGGGDEDHEMTDVLDEELNGQPETSHSQFNHNKVPDENPTQGERPLTHNPPPAPEEVPPTTRNIFVPVPPADYVPTRRPYRDPVEVANNRLPFMTPITERTEFSSLDVTYHHPNMHHPVLTKTPSKAPGLHHDMRDDMSVDGEEEDGQFSENEDEDMLEPESSPLREIPEEDEEEEEQQEKFSPIPVLYGQLGQNQPAARSPLAVKSVTTSPLAHRAVSPLAPRLPGPIIKELQCNPVDELVRKKILANVHPPLTSYEGFYDHRHEKIRKGAEIRKFAQAQSAASKRGRKSTSADKRDSLNEPHPIMLQLPGNPSTKYTIKKELGAGAYAPVYLVENSKPTKTASRTGYGYGQQENENNNLPAHLIRHPREALKMEQPPTAWEFYMMRLAHTRLLAANTSLHHRALASLSPALELHLYQDEGFLFLPFFQHGTLLDVINLFRSESSGVMDEQLAMFFTVELLRAIEALHSVGIMHGDVKVDNCLLRLPTTTDILPTNQYHADGSGGWAARGLTLIDFGRGIDFRQFRQEVQFVADWKPTAQDCAEMREGRPWTWQIDYHGLAGVVHALLFGKYIDTVPCGTSHSRSFGGPGGNDQKRYKIRENLKRYWQTDIWGHCFDLLLNPGSYVDEEEGGNMPVLKGLRTVRERMEGWLEANCERGVGLRGWMGKVEAWSRGRR",
            DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt",
            "FPTDDDDKIVGGYTCAANSIPYQVSLNSGSHFCGGSLINSQWVVSAAHCYKSRIQVRLGEHNIDVLEGNEQFINAAKIITHPNFNGNTLDNDIMLIKLSSPATLNSRVATVSLPRSCAAAGTECLISGWGNTKSSGSSYPSLLQCLKAPVLSDSSCKSSYPGQITGNMICVGFLEGGKDSCQGDSGGPVVCNGQLQGIVSWGYGCAQKNKPGVYTKVCNYVNWIQQTIAAN,QATVSLPR",
            "MSCRQFSSSYLSRSGGGGGGGLGSGGSIRSSYSRFSSSGGRGGGGRFSSSSGYGGGSSRVCGRGGGGSFGYSYGGGSGGGFSASSLGGGFGGGSRGFGGASGGGYSSSGGFGGGFGGGSGGGFGGGYGSGFGGLGGFGGGAGGGDGGILTANEKSTMQELNSRLASYLDKVQALEEANNDLENKIQDWYDKKGPAAIQKNYSPYYNTIDDLKDQIVDLTVGNNKTLLDIDNTRMTLDDFRIKFEMEQNLRQGVDADINGLRQVLDNLTMEKSDLEMQYETLQEELMALKKNHKEEMSQLTGQNSGDVNVEINVAPGKDLTKTLNDMRQEYEQLIAKNRKDIENQYETQITQIEHEVSSSGQEVQSSAKEVTQLRHGVQELEIELQSQLSKKAALEKSLEDTKNRYCGQLQMIQEQISNLEAQITDVRQEIECQNQEYSLLLSIKMRLEKEIETYHNLLEGGQEDFESSGAGKIGLGGRGGSGGSYGRGSRGGSGGSYGGGGSGGGYGGGSGSRGGSGGSYGGGSGSGGGSGGGYGGGSGGGHSGGSGGGHSGGSGGNYGGGSGSGGGSGGGYGGGSGSRGGSGGSHGGGSGFGGESGGSYGGGEEASGSGGGYGGGSGKSSHS,MSVRYSSSKHYSSSRSGGGGGGGGCGGGGGVSSLRISSSKGSLGGGFSSGGFSGGSFSRGSSGGGCFGGSSGGYGGLGGFGGGSFHGSYGSSSFGGSYGGSFGGGNFGGGSFGGGSFGGGGFGGGGFGGGFGGGFGGDGGLLSGNEKVTMQNLNDRLASYLDKVRALEESNYELEGKIKEWYEKHGNSHQGEPRDYSKYYKTIDDLKNQILNLTTDNANILLQIDNARLAADDFRLKYENEVALRQSVEADINGLRRVLDELTLTKADLEMQIESLTEELAYLKKNHEEEMKDLRNVSTGDVNVEMNAAPGVDLTQLLNNMRSQYEQLAEQNRKDAEAWFNEKSKELTTEIDNNIEQISSYKSEITELRRNVQALEIELQSQLALKQSLEASLAETEGRYCVQLSQIQAQISALEEQLQQIRAETECQNTEYQQLLDIKIRLENEIQTYRSLLEGEGSSGGGGRGGGSFGGGYGGGSSGGGSSGGGYGGGHGGSSGGGYGGGSSGGGSSGGGYGGGSSSGGHGGGSSSGGHGGSSSGGYGGGSSGGGGGGYGGGSSGGGSSSGGGYGGGSSSGGHKSSSSGSVGESSSKGPRY",
            DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt",
            "MTMDKSELVQKAKLAEQAERYDDMAAAMKAVTEQGHELSNEERNLLSVAYKNVVGARRSSWRVISSIEQKTERNEKKQQMGKEYREKIEAELQDICNDVLELLDKYLIPNATQPESKVFYLKMKGDYFRYLSEVASGDNKQTTVSNSQQAYQEAFEISKKEMQPTHPIRLGLALNFSVFYYEILNSPEKACSLAKTAFDEAIAELDTLNEESYKDSTLIMQLLRDNLTLWTSENQGDEGDAGEGEN",
            "MALDLRTIFQCEPSENNLGSENSAFQQSQGPAVQREEGISEFSRMVLNSFQDSNNSYARQELQRLYRIFHSWLQPEKHSKDEIISLLVLEQFMIGGHCNDKASVKEKWKSSGKNLERFIEDLTDDSINPPALVHVHMQGQEALFSEDMPLRDVIVHLTKQVNAQTTREANMGTPSQTSQDTSLETGQGYEDEQDGWNSSSKTTRVNENITNQGNQIVSLIIIQEENGPRPEEGGVSSDNPYNSKRAELVTARSQEGSINGITFQGVPMVMGAGCISQPEQSSPESALTHQSNEGNSTCEVHQKGSHGVQKSYKCEECPKVFKYLCHLLAHQRRHRNERPFVCPECQKGFFQISDLRVHQIIHTGKKPFTCSMCKKSFSHKTNLRSHERIHTGEKPYTCPFCKTSYRQSSTYHRHMRTHEKITLPSVPSTPEAS",
            DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence)]
        [TestCase(@"Test_Data\QC_Standards_2004-01-21.txt",
            "SRQFSSRSGYRSGGGFSSGSAGIINYQRRTTSSSTRRSGGGGGRFSSCGGGGGSFGAGGGFGSRSLVNLGGSKSISISVARGGGRGSGFGGGYGGGGFGGGGFGGGGFGGGGIGGGGFGGFGSGGGGFGGGGFGGGGYGGGYGPVCPPGGIQEVTINQSLLQPLNVEIDPEIQKVKSREREQIKSLNNQFASFIDKVRFLEQQNQVLQTKWELLQQVDTSTRTHNLEPYFESFINNLRRRVDQLKSDQSRLDSELKNMQDMVEDYRNKYEDEINKRTNAENEFVTIKKDVDGAYMTKVDLQAKLDNLQQEIDFLTALYQAELSQMQTQISETNVILSMDNNRSLDLDSIIAEVKAQNEDIAQKSKAEAESLYQSKYEELQITAGRHGDSVRNSKIEISELNRVIQRLRSEIDNVKKQISNLQQSISDAEQRGENALKDAKNKLNDLEDALQQAKEDLARLLRDYQELMNTKLALDLEIATYRTLLEGEESRMSGECAPNVSVSVSTSHTTISGGGSRGGGGGGYGSGGSSYGSGGGSYGSGGGGGGGRGSYGSGGSSYGSGGGSYGSGGGGGGHGSYGSGSSSGGYRGGSGGGGGGSSGGRGSGGGSSGGSIGGRGSSSGGVKSSGGSSSVRFVSTTYSGVTR,MSCQISCKSRGRGGGGGGFRGFSSGSAVVSGGSRRSTSSFSCLSRHGGGGGGFGGGGFGSRSLVGLGGTKSISISVAGGGGGFGAAGGFGGRGGGFGGGSGFGGGSGFGGGSGFSGGGFGGGGFGGGRFGGFGGPGGVGGLGGPGGFGPGGYPGGIHEVSVNQSLLQPLNVKVDPEIQNVKAQEREQIKTLNNKFASFIDKVRFLEQQNQVLQTKWELLQQMNVGTRPINLEPIFQGYIDSLKRYLDGLTAERTSQNSELNNMQDLVEDYKKKYEDEINKRTAAENDFVTLKKDVDNAYMIKVELQSKVDLLNQEIEFLKVLYDAEISQIHQSVTDTNVILSMDNSRNLDLDSIIAEVKAQYEEIAQRSKEEAEALYHSKYEELQVTVGRHGDSLKEIKIEISELNRVIQRLQGEIAHVKKQCKNVQDAIADAEQRGEHALKDARNKLNDLEEALQQAKEDLARLLRDYQELMNVKLALDVEIATYRKLLEGEECRMSGDLSSNVTVSVTSSTISSNVASKAAFGGSGGRGSSSGGGYSSGSSSYGSGGRQSGSRGGSGGGGSISGGGYGSGGGSGGRYGSGGGSKGGSISGGGYGSGGGKHSSGGGSRGGSSSGGGYGSGGGGSSSVKGSSGEAFGSSVTFSFR",
            "SNKLFRLDAGYQQYDWGKIGSSSAVAQFAAHSDPSVQIEQDKPYAELWMGTHSKMPSYNHESKESLRDIISKNPSAMLGKDIIDKFHATNELPFLFKVLSIEKVLSIQAHPDKALGKILHAQDPKNYPDDNHKPEMAIAVTDFEGFCGFKPLQEIADELKRIPELRNIVGEETSRNFIENIQPSAQKGSPEDEQNKKLLQAVFSRVMNASDDKIKIQARSLVERSKNSPSDFNKPDLPELIQRLNKQFPDDVGLFCGCLLLNHCRLNAGEAIFLRAKDPHAYISGDIMECMAASDNVVRAGFTPKFKDVKNLVSMLTYTYDPVEKQKMQPLKFDRSSGNGKSVLYNPPIEEFAVLETTFDEKLGQRHFEGVDGPSILITTKGNGYIKADGQKLKAEPGFVFFIAPHLPVDLEAEDEAFTTYRAFVEPN,SRPLSDQEKRKQISVRGLAGVENVTELKKNFNRHLHFTLVKDRNVATPRDYYFALAHTVRDHLVGRWIRTQQHYYEKDPKRIYYLSLEFYMGRTLQNTMVNLALENACDEATYQLGLDMEELEEIEEDAGLGNGGLGRLAACFLDSMATLGLAAYGYGIRYEFGIFNQKICGGWQMEEADDWLRYGNPWEKARPEFTLPVHFYGRVEHTSQGAKWVDTQVVLAMPYDTPVPGYRNNVVNTMRLWSAKAPNDFNLKDFNVGGYIQAVLDRNLAENISRVLYPNDNFFEGKELRLKQEYFVVAATLQDIIRRFKSSKFGCRDPVRTNFDAFPDKVAIQLNDTHPSLAIPELMRVLVDLERLDWDKAWEVTVKTCAYTNHTVLPEALERWPVHLLETLLPRHLQIIYEINQRFLNRVAAAFPGDVDRLRRMSLVEEGAVKRINMAHLCIAGSHAVNGVARIHSEILKKTIFKDFYELEPHKFQNKTNGITPRRWLVLCNPGLAEIIAERIGEEYISDLDQLRKLLSYVDDEAFIRDVAKVKQENKLKFAAYLEREYKVHINPNSLFDVQVKRIHEYKRQLLNCLHVITLYNRIKKEPNKFVVPRTVMIGGKAAPGYHMAKMIIKLITAIGDVVNHDPVVGDRLRVIFLENYRVSLAEKVIPAADLSEQISTAGTEASGTGNMKFMLNGALTIGTMDGANVEMAEEAGEENFFIFGMRVEDVDRLDQRGYNAQEYYDRIPELRQIIEQLSSGFFSPKQPDLFKDIVNMLMHHDRFKVFADYEEYVKCQERVSALYKNPREWTRMVIRNIATSGKFSSDRTIAQYAREIWGVEPSRQRLPAPDEKIP",
            DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Hash_Sequence)]
        // ReSharper restore StringLiteralTypo
        public void CheckProteinSequences(
            string proteinsFile,
            string expectedFirstProteinSequences,
            string expectedLastProteinSequences,
            DelimitedProteinFileReader.ProteinFileFormatCode fileFormatCode)
        {
            var dataFile = FileRefs.GetTestFile(proteinsFile);

            var reader = new DelimitedProteinFileReader(dataFile.FullName, fileFormatCode)
            {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinNamesOrSequences(reader, expectedFirstProteinSequences, expectedLastProteinSequences, false);
        }

        // ReSharper disable StringLiteralTypo
        [Test]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt",
            "Contaminant_Trypa1|Contaminant_CTRB_BOVIN",
            "VATVSLPR-like Promega trypsin artifact 1 (871.1) xATVSLPR (PromTArt1)|CHYMOTRYPSINOGEN B (EC 3.4.21.1). - BOS TAURUS (BOVINE).",
            '|')]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt",
            "1433E_HUMAN|NRG4_HUMAN",
            "14-3-3 protein epsilon; Short=14-3-3E; Accession=Q7M4R4|Pro-neuregulin-4, membrane-bound isoform; Short=Pro-NRG4; Contains: Neuregulin-4; Short=NRG-4; Accession=Q8WWG1; A6NIE8",
            '|')]
        // ReSharper enable StringLiteralTypo
        public void CheckProteinDescription(string proteinsFile, string proteinNames, string proteinDescriptions, char delimiter)
        {
            var dataFile = FileRefs.GetTestFile(proteinsFile);

            var reader = new DelimitedProteinFileReader(dataFile.FullName)
            {
                SkipFirstLine = false
            };

            ValidationLogic.CheckProteinDescription(reader, proteinNames, proteinDescriptions, delimiter);
        }

        [Test]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_SequenceOnly.txt", 10, 3988, DelimitedProteinFileReader.ProteinFileFormatCode.SequenceOnly)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequence.txt", 9, 3859, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt", 15, 41451, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_UniqueIDSequence.txt", 9, 3859, DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceID.txt", 14, 7101, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceID.csv", 14, 7101, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceIDMassNET.txt", 11, 4101, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameSequenceIDMassNETPlusStDevAndDiscriminant.txt", 10, 3988, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_IDSequenceMassNET.txt", 10, 3988, DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_NameDescriptionHashSequence.txt", 14, 7101, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Hash_Sequence)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov_with_extra.txt", 16, 4766, DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Hash_Sequence)]
        public void CheckDelimitedFileFormats(
            string proteinsFile,
            int proteinCountExpected, int totalResidueCountExpected,
            DelimitedProteinFileReader.ProteinFileFormatCode formatCode)
        {
            var dataFile = FileRefs.GetTestFile(proteinsFile);

            var reader = new DelimitedProteinFileReader(dataFile.FullName, formatCode)
            {
                SkipFirstLine = true
            };

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);
        }

        [Test]
        [TestCase(@"Test_Data\JunkTest.txt", 26, 7427, true)]
        [TestCase(@"Test_Data\NonExistentFile.txt", 0, 0, false)]
        [TestCase(@"Test_Data\Tryp_Pig_Bov.txt", 16, 4766, true)]
        [TestCase(@"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt", 15, 41451, true)]
        public void TestOpenFileParameterlessConstructor(string proteinsFile, int proteinCountExpected, int totalResidueCountExpected, bool fileShouldExist)
        {
            var dataFile = fileShouldExist
                ? FileRefs.GetTestFile(proteinsFile)
                : new FileInfo(proteinsFile);

            var reader = new DelimitedProteinFileReader
            {
                SkipFirstLine = true
            };

            var fileOpened = reader.OpenFile(dataFile.FullName);

            if (!fileOpened)
            {
                if (!fileShouldExist)
                {
                    Console.WriteLine("reader.OpenFile returned false, which is to be expected for a non existent file");
                    return;
                }

                Assert.Fail("Input file not found: " + dataFile.FullName);
            }

            ValidationLogic.CheckProteinStats(reader, proteinCountExpected, totalResidueCountExpected);
        }

        [Test]
        [TestCase(@"Test_Data\JunkTest.txt," +
                  @"Test_Data\Tryp_Pig_Bov.txt," +
                  @"Test_Data\H_sapiens_Uniprot_SPROT_2017-04-12_excerpt.txt," +
                  @"Test_Data\QC_Standards_2004-01-21.csv")]
        public void TestOpenMultipleFiles(string proteinFiles)
        {
            var inputFiles = proteinFiles.Split(',');

            var reader = new DelimitedProteinFileReader
            {
                SkipFirstLine = true
            };

            var fileNumber = 0;
            foreach (var inputFile in inputFiles)
            {
                fileNumber++;

                var dataFile = FileRefs.GetTestFile(inputFile);

                Console.WriteLine("Opening file {0}: {1}", fileNumber, dataFile.FullName);

                // Note that OpenFile will call CloseFile before opening the file
                var fileOpened = reader.OpenFile(dataFile.FullName);

                if (!fileOpened)
                {
                    Assert.Fail("Input file not found: " + dataFile.FullName);
                }

                ValidationLogic.CheckProteinStats(reader, 0, 0);

                if (fileNumber == 2)
                {
                    // Explicitly call CloseFile
                    reader.CloseFile();
                }

                Console.WriteLine();
            }
        }
    }
}

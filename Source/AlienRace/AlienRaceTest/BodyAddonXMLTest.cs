namespace AlienRaceTest
{
    using System;
    using System.Xml;
    using System.Xml.Linq;
    using AlienRace;
    using NUnit.Framework;
    using RimWorld;
    using TestSupport;
    using Verse;

    public class BodyAddonXMLTest : BaseUnityTest
    {
        private const string BODY_ADDONS_X_PATH =
            "/Defs/AlienRace.ThingDef_AlienRace/alienRace/generalSettings/alienPartGenerator/bodyAddons";

        private readonly XmlDocument testXmlRaceDef = new XmlDocument();

        [OneTimeSetUp]
        public void SetupXMLTests()
        {
            this.testXmlRaceDef.Load("testData/TestRace.xml");
            PrefsData prefsData = new PrefsData
                                  {
                                      logVerbose = false
                                  };
            InitPrefs(prefsData);
        }

        private XmlNode BodyAddonNodeMatching(string xPathFragment)
        {
            XmlNode testXmlNode = this.testXmlRaceDef.SelectSingleNode($"{BODY_ADDONS_X_PATH}/{xPathFragment}");
            Assert.IsNotNull(testXmlNode, nameof(testXmlNode) + " != null");
            Console.WriteLine($"Parsing:\n{XElement.Parse(testXmlNode.OuterXml)}");
            return testXmlNode;
        }

        [Test]
        public void TestCanParseCustomBackstoryGraphicXML()
        {
            AlienPartGenerator.BodyAddonBackstoryGraphic testBackstoryGraphic =
                new AlienPartGenerator.BodyAddonBackstoryGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar");

            // Attempt to parse XML
            testBackstoryGraphic.LoadDataFromXmlCustom(testXmlNode);

            Assert.AreEqual("Test_Templar", testBackstoryGraphic.backstory);
            Assert.AreEqual("test/B",       testBackstoryGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomAgeGraphicXML()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");

            AlienPartGenerator.BodyAddonAgeGraphic testAgeGraphic =
                new AlienPartGenerator.BodyAddonAgeGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar/ageGraphics/HumanlikeAdult");

            // Attempt to parse XML
            testAgeGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreSame(humanlikeAdultLifeStageDef, testAgeGraphic.age);
            Assert.AreEqual("test/BA", testAgeGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomDamageGraphicXML()
        {
            AlienPartGenerator.BodyAddonDamageGraphic testDamageGraphic =
                new AlienPartGenerator.BodyAddonDamageGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar/ageGraphics/HumanlikeAdult/damageGraphics/a5");

            // Attempt to parse XML
            testDamageGraphic.LoadDataFromXmlCustom(testXmlNode);

            Assert.AreEqual(5f,          testDamageGraphic.damage);
            Assert.AreEqual("test/BAd5", testDamageGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomHediffGraphicXML()
        {
            // Setup XRefs
            HediffDef crackHediffDef = AddHediffWithName("Crack");

            AlienPartGenerator.BodyAddonHediffGraphic bodyAddonHediffGraphic =
                new AlienPartGenerator.BodyAddonHediffGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/hediffGraphics/Crack");

            // Attempt to parse XML
            bodyAddonHediffGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreSame(crackHediffDef, bodyAddonHediffGraphic.hediff);
            Assert.AreEqual("test/C", bodyAddonHediffGraphic.GetPath());
        }

        [Test]
        public void TestCanParseCustomHediffSeverityGraphicXML()
        {
            AlienPartGenerator.BodyAddonHediffSeverityGraphic bodyAddonHediffSeverityGraphic =
                new AlienPartGenerator.BodyAddonHediffSeverityGraphic();

            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/hediffGraphics/Crack/hediffGraphics/Plague/severity/a0.5");

            // Attempt to parse XML
            bodyAddonHediffSeverityGraphic.LoadDataFromXmlCustom(testXmlNode);
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);

            Assert.AreEqual(0.5f,        bodyAddonHediffSeverityGraphic.severity);
            Assert.AreEqual("test/CPs5", bodyAddonHediffSeverityGraphic.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomBackstorySubtree()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");
            
            // Select test node
            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]/backstoryGraphics/Test_Templar");

            // Attempt to parse XML
            AlienPartGenerator.BodyAddonBackstoryGraphic parsedGraphic = DirectXmlToObject.ObjectFromXml<AlienPartGenerator.BodyAddonBackstoryGraphic>(testXmlNode, false);
            
            // Reflectively populate all the XRefs
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
            
            Assert.AreEqual("Test_Templar", parsedGraphic.backstory);
            Assert.AreEqual("test/B",       parsedGraphic.GetPath());
            Assert.IsNotNull(parsedGraphic.ageGraphics);
            Assert.AreEqual(1, parsedGraphic.ageGraphics.Count);

            AlienPartGenerator.BodyAddonAgeGraphic parsedAgeGraphic = parsedGraphic.ageGraphics[0];
            Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
            Assert.AreEqual("test/BA", parsedAgeGraphic.GetPath());
            Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
            Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);

            AlienPartGenerator.BodyAddonDamageGraphic parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
            Assert.AreEqual("test/BAd1", parsedDamageGraphic1.GetPath());
            
            AlienPartGenerator.BodyAddonDamageGraphic parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
            Assert.AreEqual(5f,          parsedDamageGraphic5.damage);
            Assert.AreEqual("test/BAd5", parsedDamageGraphic5.GetPath());
        }
        
        [Test]
        public void TestCanParseCustomWholeAddon()
        {
            // Setup XRefs
            LifeStageDef humanlikeAdultLifeStageDef = AddLifestageWithName("HumanlikeAdult");
            HediffDef crackHediffDef = AddHediffWithName("Crack");
            HediffDef plagueHediffDef = AddHediffWithName("Plague");
            
            // Select test node
            XmlNode testXmlNode =
                this.BodyAddonNodeMatching("li[bodyPart[contains(text(), 'Nose')]]");

            // Attempt to parse XML
            AlienPartGenerator.BodyAddon parsedGraphic = DirectXmlToObject.ObjectFromXml<AlienPartGenerator.BodyAddon>(testXmlNode, false);
            
            // Reflectively populate all the XRefs
            DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
            
            Assert.AreEqual("Nose", parsedGraphic.bodyPart);
            Assert.IsTrue(parsedGraphic.inFrontOfBody);
            Assert.IsTrue(parsedGraphic.alignWithHead);
            Assert.AreEqual("base", parsedGraphic.ColorChannel);
            Assert.AreEqual("test/default", parsedGraphic.path);
            
            // Hediff Graphics
            Assert.IsNotNull(parsedGraphic.hediffGraphics);
            Assert.AreEqual(1, parsedGraphic.hediffGraphics.Count);
            
                // Crack
                AlienPartGenerator.BodyAddonHediffGraphic parsedCrackGraphic = parsedGraphic.hediffGraphics[0];
                Assert.AreEqual("test/C", parsedCrackGraphic.GetPath());
                Assert.AreSame(crackHediffDef, parsedCrackGraphic.hediff);
                
                // Crack Hediff Graphics
                Assert.IsNotNull(parsedCrackGraphic.hediffGraphics);
                Assert.AreEqual(1, parsedCrackGraphic.hediffGraphics.Count);
                
                    // Crack->Plague
                    AlienPartGenerator.BodyAddonHediffGraphic parsedPlagueGraphic = parsedCrackGraphic.hediffGraphics[0];
                    Assert.AreEqual("test/CP", parsedPlagueGraphic.GetPath());
                    Assert.AreSame(plagueHediffDef, parsedPlagueGraphic.hediff);
                    
                    // Crack->Plague->Severity Graphics
                    Assert.IsNotNull(parsedPlagueGraphic.severity);
                    Assert.AreEqual(1, parsedPlagueGraphic.severity.Count);
                    
                        AlienPartGenerator.BodyAddonHediffSeverityGraphic parsedSeverityGraphic = parsedPlagueGraphic.severity[0];
                        Assert.AreEqual("test/CPs5", parsedSeverityGraphic.GetPath());
                        Assert.AreEqual(0.5f, parsedSeverityGraphic.severity);
                        
                        // Crack->Plague->Severity->Backstory Graphics
                        Assert.IsNotNull(parsedSeverityGraphic.backstoryGraphics);
                        Assert.AreEqual(1, parsedSeverityGraphic.backstoryGraphics.Count);
                        
                            AlienPartGenerator.BodyAddonBackstoryGraphic parsedBackstoryGraphic = parsedSeverityGraphic.backstoryGraphics[0];
                            Assert.AreEqual("Test_Templar", parsedBackstoryGraphic.backstory);
                            Assert.AreEqual("test/CPs5B", parsedBackstoryGraphic.GetPath());
                            
                            // Crack->Plague->Severity->Backstory->Age Graphics
                            Assert.IsNotNull(parsedBackstoryGraphic.ageGraphics);
                            Assert.AreEqual(1, parsedBackstoryGraphic.ageGraphics.Count);
                            
                                AlienPartGenerator.BodyAddonAgeGraphic parsedAgeGraphic = parsedBackstoryGraphic.ageGraphics[0];
                                Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                                Assert.AreEqual("test/CPs5BA", parsedAgeGraphic.GetPath());
                                
                                // Crack->Plague->Severity->Backstory->Age->Damage Graphics
                                Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                                Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                                
                                    AlienPartGenerator.BodyAddonDamageGraphic parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                                    Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                                    Assert.AreEqual("test/CPs5BAd1", parsedDamageGraphic1.GetPath());
                                    
                                    AlienPartGenerator.BodyAddonDamageGraphic parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                                    Assert.AreEqual(5f,          parsedDamageGraphic5.damage);
                                    Assert.AreEqual("test/CPs5BAd5", parsedDamageGraphic5.GetPath());

                // Crack Backstory Graphics
                Assert.IsNotNull(parsedCrackGraphic.backstoryGraphics);
                Assert.AreEqual(1, parsedCrackGraphic.backstoryGraphics.Count);
                
                    parsedBackstoryGraphic = parsedCrackGraphic.backstoryGraphics[0];
                    Assert.AreEqual("Test_Templar", parsedBackstoryGraphic.backstory);
                    Assert.AreEqual("test/CB", parsedBackstoryGraphic.GetPath());
                            
                    // Crack->Backstory->Age Graphics
                    Assert.IsNotNull(parsedBackstoryGraphic.ageGraphics);
                    Assert.AreEqual(1, parsedBackstoryGraphic.ageGraphics.Count);
                    
                        parsedAgeGraphic = parsedBackstoryGraphic.ageGraphics[0];
                        Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                        Assert.AreEqual("test/CBA", parsedAgeGraphic.GetPath());
                        
                        // Crack->Backstory->Age->Damage Graphics
                        Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                        Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                        
                            parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                            Assert.AreEqual("test/CBAd1", parsedDamageGraphic1.GetPath());
                            
                            parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                            Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                            Assert.AreEqual("test/CBAd5", parsedDamageGraphic5.GetPath());
                            
                // Crack Age Graphics
                Assert.IsNotNull(parsedCrackGraphic.ageGraphics);
                Assert.AreEqual(1, parsedCrackGraphic.ageGraphics.Count);
        
                    parsedAgeGraphic = parsedCrackGraphic.ageGraphics[0];
                    Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                    Assert.AreEqual("test/CA", parsedAgeGraphic.GetPath());
            
                    // Crack->Age->Damage Graphics
                    Assert.IsNull(parsedAgeGraphic.damageGraphics);
                        
                // Backstory Graphics
                Assert.IsNotNull(parsedGraphic.backstoryGraphics);
                Assert.AreEqual(1, parsedGraphic.backstoryGraphics.Count);
                
                    parsedBackstoryGraphic = parsedGraphic.backstoryGraphics[0];
                    Assert.AreEqual("Test_Templar", parsedBackstoryGraphic.backstory);
                    Assert.AreEqual("test/B", parsedBackstoryGraphic.GetPath());
                            
                    // Backstory->Age Graphics
                    Assert.IsNotNull(parsedBackstoryGraphic.ageGraphics);
                    Assert.AreEqual(1, parsedBackstoryGraphic.ageGraphics.Count);
                    
                        parsedAgeGraphic = parsedBackstoryGraphic.ageGraphics[0];
                        Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                        Assert.AreEqual("test/BA", parsedAgeGraphic.GetPath());
                        
                        // Backstory->Age->Damage Graphics
                        Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                        Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                        
                            parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                            Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                            Assert.AreEqual("test/BAd1", parsedDamageGraphic1.GetPath());
                            
                            parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                            Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                            Assert.AreEqual("test/BAd5", parsedDamageGraphic5.GetPath());
                            
                            
                // Age Graphics
                Assert.IsNotNull(parsedGraphic.ageGraphics);
                Assert.AreEqual(1, parsedGraphic.ageGraphics.Count);
                
                    parsedAgeGraphic = parsedGraphic.ageGraphics[0];
                    Assert.AreSame(humanlikeAdultLifeStageDef, parsedAgeGraphic.age);
                    Assert.AreEqual("test/A", parsedAgeGraphic.GetPath());
                    
                    // Backstory->Age->Damage Graphics
                    Assert.IsNotNull(parsedAgeGraphic.damageGraphics);
                    Assert.AreEqual(2, parsedAgeGraphic.damageGraphics.Count);
                    
                        parsedDamageGraphic1 = parsedAgeGraphic.damageGraphics[0];
                        Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                        Assert.AreEqual("test/Ad1", parsedDamageGraphic1.GetPath());
                        
                        parsedDamageGraphic5 = parsedAgeGraphic.damageGraphics[1];
                        Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                        Assert.AreEqual("test/Ad5", parsedDamageGraphic5.GetPath());
                        
                        
                // Backstory->Age->Damage Graphics
                Assert.IsNotNull(parsedGraphic.damageGraphics);
                Assert.AreEqual(2, parsedGraphic.damageGraphics.Count);
                
                    parsedDamageGraphic1 = parsedGraphic.damageGraphics[0];
                    Assert.AreEqual(1f,          parsedDamageGraphic1.damage);
                    Assert.AreEqual("test/d1", parsedDamageGraphic1.GetPath());
                    
                    parsedDamageGraphic5 = parsedGraphic.damageGraphics[1];
                    Assert.AreEqual(5f,           parsedDamageGraphic5.damage);
                    Assert.AreEqual("test/d5", parsedDamageGraphic5.GetPath());
        }
    }
}
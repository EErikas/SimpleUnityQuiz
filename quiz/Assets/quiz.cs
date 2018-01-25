using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Net;
using System.Text;
/*
To do:
-> Tasku skaiciavimas zaidimo metu
-> Reiksmiu uzpildymas
-> Meniu
*/
public class quiz : MonoBehaviour
{

    public GameObject button1;
    public GameObject button2;
    public GameObject button3;
    public GameObject button4;

    public Text text;
    public Text health;
    public Text score;
    public Text gameScore;
    public Text gameOverMessage;
    public GameObject game;
    public GameObject over;

    public string correctAnswer;
    public string answerListPath;
    public string questionListPath;

    int healths = 3;
    int scores = 0;

    public bool firstStart = true;
    int questionNo = 0;
    List<Question> mainGameQuestionList;

    // Use this for initialization
    void Start()
    {
        SetButtonColor();
        if (firstStart)
        {
            mainGameQuestionList = AnswersAndQuestions();
            ShuffleList(mainGameQuestionList);
            firstStart = false;
            Debug.Log("Question count: " + mainGameQuestionList.Count());
        }
        if (scores == mainGameQuestionList.Count)
        {
            GameOver();
            Wait();
            gameOverMessage.text = "Sveikiname";
        }
        else
        {
            over.SetActive(false);
            var currentQuestion = mainGameQuestionList[questionNo];
            SetQuestion(currentQuestion);
        }
    }
    // Update is called once per frame
    void Update()
    {
        gameScore.text = scores.ToString();
        health.text = healths.ToString();
        score.text = scores.ToString();
        if (healths <= 0)
            GameOver();
        if (scores == mainGameQuestionList.Count)
        {
            GameOver();
            Wait();
            gameOverMessage.text = "Sveikiname";
        }
    }

    public void ToMenu()
    {
        Application.LoadLevel("menu");
    }

    void ShuffleList(IList list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    void SetQuestion(Question q)
    {
        ShuffleList(q.allAnswers);
        text.color = Color.black;
        text.text = q.question;
        button1.GetComponentInChildren<Text>().text = q.allAnswers[0].ToUpper();
        button2.GetComponentInChildren<Text>().text = q.allAnswers[1].ToUpper();
        button3.GetComponentInChildren<Text>().text = q.allAnswers[2].ToUpper();
        button4.GetComponentInChildren<Text>().text = q.allAnswers[3].ToUpper();
        correctAnswer = q.answer.ToUpper();
    }
    public void ButtonClick()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        if (button.GetComponentInChildren<Text>().text == correctAnswer)
        {
            button.GetComponentInChildren<Image>().color = Color.green;
            scores++;
            questionNo++;
            StartCoroutine("Wait");
        }
        else
        {
            button.GetComponentInChildren<Image>().color = Color.red;
            healths--;
        }
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds((float)0.5);
        Start();
    }
    void SetButtonColor()
    {
        Color btnColor = new Color(180, 176, 176, 255);
        button1.GetComponentInChildren<Image>().color = btnColor;
        button2.GetComponentInChildren<Image>().color = btnColor;
        button3.GetComponentInChildren<Image>().color = btnColor;
        button4.GetComponentInChildren<Image>().color = btnColor;
    }
    void GameOver()
    {
        game.SetActive(false);
        over.SetActive(true);
    }

    public void Restart()
    {
        game.SetActive(true);
        healths = 3;
        scores = 0;
        questionNo = 0;
        firstStart = true;
        Start();
    }

    List<Question> AnswersAndQuestions()
    {
        List<Question> questionList = new List<Question>();
        List<string> definitionList = HardcodedLists.testDefinitionList;
        foreach (string line in definitionList)
        {
            var lineElements = line.Split('>');
            //If answer is not empty:
            if (lineElements[1].Length > 0)
            {
                Question q = new Question();
                q.answer = lineElements[0];
                q.question = lineElements[1];
                q.allAnswers = RandomAnswers(q.answer);
                questionList.Add(q);
                ConsoleLogOutput(q);
            }
        }
        return questionList;
    }
    public List<string> RandomAnswers(string correctAnswer)
    {
        List<string> answerList = HardcodedLists.testAnswerList;

        List<string> output = new List<string>();
        int i = 0;
        while (i < 3)
        {
            int randomIndex = UnityEngine.Random.Range(0, answerList.Count);
            if ((!output.Contains(answerList[randomIndex])) && (answerList[randomIndex] != correctAnswer))
            {
                output.Add(answerList[randomIndex]);
                i++;
            }
        }
        output.Add(correctAnswer);
        return output;
    }
    void ConsoleLogOutput(Question q)
    {
        var newLine = System.Environment.NewLine;
        string logAllAnswers = "Options:" + newLine;
        int n = 1;
        foreach (string line in q.allAnswers)
        {
            logAllAnswers += n.ToString() + ". " + line + newLine;
            n++;
        }
        Debug.LogFormat("Question: {0}{1}Correct answer: {2}{1}{3}", q.question, newLine, q.answer, logAllAnswers);
    }
    //----------------------------------------------------<Unused methods>------------------------------------------------------------------
    void List2HardcodeFormat(List<string> list, string fileName)
    {
        List<string> newList = new List<string>();
        int i = 1;
        foreach (string line in list)
        {
            string newLine = "/*" + (i + 1000).ToString().Remove(0, 1) + "*/" + "\"" + line + "\",";
            newList.Add(newLine);
            i++;
        }
        File.WriteAllLines("Assets/Hardcoded" + fileName + ".txt", newList.ToArray());
    }
    void FormatDataFile(List<string> s, string name)
    {
        string path = "Assets/" + name + ".csv";
        File.WriteAllLines(path, s.ToArray());
    }
    void GetFiles()
    {
        var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "rIdejos_Data");
        answerListPath = Path.Combine(dataDir, "answers.csv");
        questionListPath = Path.Combine(dataDir, "questions.csv");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
            Debug.Log("Directory created at: " + dataDir);
        }
        WebClient client = new WebClient();
        client.DownloadFile("https://drive.google.com/uc?export=download&id=1Xae-Phqc6x4eyMhx-k57KHZWDyGVYufa", answerListPath);
        Debug.Log("File saved at: " + answerListPath);
        client.DownloadFile("https://drive.google.com/uc?export=download&id=1wmF4to9eaNoteYX_X6yTAX7Cl69aFiHS", questionListPath);
        Debug.Log("File saved at: " + questionListPath);
    }
    //----------------------------------------------------</Unsed methods>------------------------------------------------------------------
    public class Question
    {
        public string question { get; set; }
        public string answer { get; set; }
        public List<string> allAnswers { get; set; }
    }
    public static class HardcodedLists
    {
        public static List<string> testDefinitionList = new List<string>()
        {
            /*001*/"Absoliutizmas>Aukščiausios valdžios priklausymas vienam asmeniui",
            /*002*/"Aktyvizmas>Sąmoningas veiksmas, kurio tikslas yra socialinis ar politinis pokytis",
            /*003*/"Anarchizmas>Politinė filosofija, teigianti, kad hierarchinės valdymo formos yra nereikalingos",
            /*004*/"Aristokratija>Labiausiai privilegijuota bet kurio socialinio sluoksnio ar luomo viršūnė",
            /*005*/"Bausmės>Valdžios taikomos sankcijos už įstatymų nevykdymą",
            /*006*/"Demokratija>Politinė filosofija, kurios pagrindas yra žmonių valdžia",
            /*007*/"Ekonomika>Socialinis mokslas, kurio pavadinimas išvertus iš graikų kalbos reiškia „namų teisė“",
            /*008*/"Fašizmas>Autoritarizmu ir nacionalizmu paremta politinė srovė",
            /*009*/"Feminizmas>Politinių, socialinių ir kultūrinių judėjimų derinys, kurių tikslas - ginti moterų interesus visuomenėje",
            /*010*/"Globalizacija>Procesas, kuriam vykstant atskiros pasaulio valstybės tampa vis labiau priklausomos viena nuo kitos",
            /*011*/"Juodoji galia>Politinis judėjimas, kuris XX a. 7 deš. kilo JAV ir kurio tikslas buvo užtikrinti ir ginti juodaodžių amerikiečių teises",
            /*012*/"Juodoji sąmonė>PAR apartheido metu veikęs judėjimas, įkvėptas „Juodosios galios“ judėjimo",
            /*013*/"Kapitalizmas>Ekonominė sistema, kurioje gamybos priemonės yra privati nuosavybė, pastangas motyvuoja pelnas, o darbuotojams mokamas užmokestis",
            /*014*/"Karas>Ginkluotas konfliktas tarp organizuotų subjektų",
            /*015*/"Karo nusikaltimai>Niurnbergo tribunolo chartija apibrėžė dvi iki šiol svarbias sąvokas: nusikaltimai prieš žmoniją ir ...",
            /*016*/"Klasės>Žmonių grupės, turinčios tokį patį socialinį statusą visuomenėje",
            /*017*/"Komunizmas>Galutinis pagal marksizmo dialektiką sumodeliuotos istorijos etapas",
            /*018*/"Laisvė>Radikaliausia X gynimo forma yra liberalizmas. Kas yra X?",
            /*019*/"Liberalizmas>Politinė teorija, kurios viena svarbiausių savybių yra pilietinių laisvių įteisinimas ir apsauga bei nuosaikių konstitucinių priemonių taikymas",
            /*020*/"Lygybė>Siektinas idealas, kurio pagrindas yra teiginys, jog nėra tokio bruožo, kuris suteiktų asmeniui ar grupei viršenybę kitų asmenų ar grupių atžvilgiu",
            /*021*/"Marksizmas>K. Markso ir F. Engelso sukurta politinė ideologija, kurios pagrindą sudaro ekonominių santykių teorija",
            /*022*/"Nacionalizmas>Politinė ideologija, kuri teigia, kad natūralus politinis vienetas yra „nacija“",
            /*023*/"Naujasis konservatizmas>Politinė srovė, kuri atsirado JAV XX a. 7 deš., „kairiųjų“ pažiūrų intelektualams „perbėgus“ į „dešinę“",
            /*024*/"Politika>Bendro žmonių gyvenimo visuomenėje organizavimo ir nuolatinio reguliavimo veikla",
            /*025*/"Privatumas>",
            /*026*/"Socializmas>Politinių teorijų, besilaikančių nuostatos, kad  gamybos priemonės turi būti valstybės nuosavybė, visuma",
            /*027*/"Suvakarietinimas (vesternizacija)>Vakarų įtakos augimas nevakarietiškose šalyse",
            /*028*/"Dirbtinis intelektas>Protingų mašinų kūrimo mokslas ir inžinerija",
            /*029*/"Teisė>Priemonių visuma, teisingumui visuomenėje nustatyti ir užtikrinti",
            /*030*/"Teisingumas>Teisingų dalykų įgyvendinimas",
            /*031*/"Totalitarizmas>Rėžimas, kuris remiasi visų visuomenės gyvenimo sričių kontrole",
            /*032*/"Evoliucija>Procesas, kurio metu gyvosios gamtos objektai keičiasi paveldėjimo būdu",
            /*033*/"žodžio laisvė>Laisvė skleisti savo mintis be cenzūros",
            /*034*/"Antisemitizmas>Priešiškumas ar išankstinis nusistatymas nukreiptas prieš žydus kaip tautinę, religinę ar rasinę grupę.",
            /*035*/"Antropocentrizmas>Pasaulėžiūra, teigianti, jog žmogus yra reikšmingiausia būtybė visatoje",
            /*036*/"Ateitis>",
            /*037*/"Reliatyvumas>",
            /*038*/"Civilizacija>Tam tikra tautos, kuri yra pasiekusi gana aukštą kultūros, technologijų ir visuomenės organizacinės struktūros išsivystymo lygį, būsena",
            /*039*/"Daugiakultūriškumas>Idėja, kuria siekiama skatinti socialinę sanglaudą ir harmoniją tarp skirtingos etninės, religinės ir kultūrinės tapatybės žmonių, jiems išlaikant savo skirtingumą",
            /*040*/"Etnocentrizmas>",
            /*041*/"Neuromokslai>",
            /*042*/"Gamtos mokslai>",
            /*043*/"Internetas>Pasaulinis kompiuterių tinklas",
            /*044*/"Istorija>Praeities įvykiai arba praeities įvykių tyrinėjimas",
            /*045*/"Kognityvinė terapija>",
            /*046*/"Laikmečio dvasia (Zeitgeist)>Populiariausios idėjos, kurios gali apibūdinti tam tikro laikotarpio visuomenę",
            /*047*/"Kvantinė mechanika>",
            /*048*/"Postmodernizmas>",
            /*049*/"Prisitaikymo teorija>",
            /*050*/"Kognityviniai mokslai>",
            /*051*/"Psichoanalizė>",
            /*052*/"Psichologija>Mokslas, tiriantis psichinius reiškinius, jų kilmę, raidą, reiškimosi formas ir mechanizmus",
            /*053*/"Rasizmas>Diskriminacija, neapykanta, nepasitikėjimas ir fanatiškos nuostatos kitų žmonių atžvilgiu dėl jų odos spalvos ar kitokių skiriamųjų išvaizdos bruožų.",
            /*054*/"Reklama>",
            /*055*/"Reliatyvizmas>",
            /*056*/"Religija>Su antgamtiniu subjektu ar subjektais susijusių įsitikinimų bei iš šių įsitikinimų kylančių praktinių veiksmų visuma",
            /*057*/"Romantizmas>",
            /*058*/"Sociobiologija>",
            /*059*/"švietimas>",
            /*060*/"Tapatybė>",
            /*061*/"Vartotojiškumas>",
            /*062*/"Vergovė>",
            /*063*/"Altruizmas>",
            /*064*/"Autonomija>Savivalda kitaip",
            /*065*/"Bioetika>",
            /*066*/"Deontologija>Etikos teorija, teigianti, kad tam tikri poelgiai yra savaime blogi, nors jų pasekmės ir geros, ir net jeigu tokių veiksmų pasekmės yra moraliai privalomos",
            /*067*/"Egoizmas>",
            /*068*/"Egzistencializmas>",
            /*069*/"Eksperimentinė  filosofija>",
            /*070*/"Epistemologija>",
            /*071*/"Estetika>",
            /*072*/"Etika>",
            /*073*/"Etikos istorija>",
            /*074*/"Eutanazija>",
            /*075*/"Falsifikacija>",
            /*076*/"Gyvūnų teisės>",
            /*077*/"Humanizmas>",
            /*078*/"Intuityvizmas>",
            /*079*/"Iracionalizmas>",
            /*080*/"Klaidingas neformaliosios logikos argumentavimas>",
            /*081*/"Klonavimas>",
            /*082*/"Juodosios skylės>",
            /*083*/"Logika>Samprotavimų ir argumentų mokslas",
            /*084*/"Meilė>",
            /*085*/"Metafizika>",
            /*086*/"Pasekmizmas (konsekvencializmas)>",
            /*087*/"Prasmės teorija>",
            /*088*/"Sąmonės filosofija>",
            /*089*/"Šviečiamasis amžius>XVIII a. Europoje ir Šiaurės Amerikoje kilęs minties judėjimas, kuris pabrėžė pasitikėjimą protu, skatino mokslą bei priešinosi absoliutinei monarchijai, organizuotai religijai ir prietarams",
            /*090*/"Verslo etika>",
            /*091*/"žaidimų teorija>",
            /*092*/"žmogaus teisės>",
            /*093*/"Agnosticizmas>Požiūris, kad asmuo privalo susilaikyti ir nuo tikėjimo, ir nuo netikėjimo tam tikrais dalykais, remdamasis tuo, kad nei vieno, nei antro neįmanoma patvirtinti esamais įrodymais",
            /*094*/"Ateizmas> Griežtas tvirtinimas, kuriuo neigiamas antgamtinių subjektų arba institucijų buvimas",
            /*095*/"Budizmas>Viena iš didžiausių pasaulio religijų, kurios pradininku yra laikomas Sidharta Gautama",
            /*096*/"Daoizmas (taosizmas)>Kinijos filosofinės minties tradicija, kurios įkūrėjas yra Lao Dzė",
            /*097*/"Fundamentalizmas>Kokios nors tiesos supratimas griežtai paremtas jos fundamentinėmis idėjomis, ir bekompromisinis jų laikymasis bei taikymas",
            /*098*/"Hinduizmas ir brahmanizmas>Senovinė Indijos religija",
            /*099*/"Islamas>Monoteistinė religija, kuri turi dvi didžiausias atšakas: šiitus ir sunitus",
            /*100*/"Judaizmas>Seniausia pasaulyje monoteistinė religija, kurios išpažinėjų šventasis raštas vadinamas Tora",
            /*101*/"Katalikybė>Po didžiosios schizmos atsiradusi krikščionybės šaka, kurios išpažinėjai pripažįsta Romos Popiežių",
            /*102*/"Kreacionizmas>Teorija, kad pasaulį sukūrė dievybė",
            /*103*/"Krikščionybė>Monoteistinė religija, grindžiama Jėzaus Kristaus gyvenimu ir mokymu, perteiktais Naujajame Testamente.",
            /*104*/"Pomirtinis gyvenimas>",
            /*105*/"Protestantizmas>Krikščioniškosios denominacijos, kurios susikūrė po Reformacijos XVI a.",
            /*106*/"Sekuliarumas>Religijos ir valstybės reikalų atskyrimas",
            /*107*/"Stačiatikybė>Po didžiosios schizmos atsiradusi krikščionybės šaka, dar vadinama ortodoksine krikščionybe",
            /*108*/"Abiogenezė>Procesas, kurio metu Žemės planetoje iš negyvos medžiagos atsirado gyvybė",
            /*109*/"Antropinis principas>Įsitikinimas, kad gamtos konstantos yra pritaikytos gyvybės atsiradimui ir lėmė tai, kad mes tam tikru metu gyvename tam tikroje Visatos vietoje.",
            /*110*/"Biologija>Gyvybės mokslas",
            /*111*/"Biologinė įvairovė>Milžiniškos Žemėje gyvuojančių skirtingų būtybių ir sistemų gausos apibūdinimas",
            /*112*/"Didžiojo sprogimo kosmologija>Plačiausiai žinoma kosmologijos teorija apie Visatos kilmę"
        };
        public static List<string> testAnswerList = new List<string>()
        {
            "Absoliutizmas",
            "Aktyvizmas",
            "Anarchizmas",
            "Aristokratija",
            "Bausmės",
            "Demokratija",
            "Ekonomika",
            "Fašizmas",
            "Feminizmas",
            "Globalizacija",
            "Juodoji galia",
            "Juodoji sąmonė",
            "Kapitalizmas",
            "Karas",
            "Karo nusikaltimai",
            "Klasės",
            "Komunizmas",
            "Laisvė",
            "Liberalizmas",
            "Lygybė",
            "Marksizmas",
            "Nacionalizmas",
            "Naujasis konservatizmas",
            "Politika",
            "Privatumas",
            "Socializmas",
            "Suvakarietinimas (vesternizacija)",
            "Tapatybė",
            "Teisė",
            "Teisingumas",
            "Totalitarizmas",
            "žmogaus teisės",
            "žodžio laisvė",
            "Antisemitizmas",
            "Antropocentrizmas",
            "Ateitis",
            "Civilizacija",
            "Daugiakultūriškumas",
            "Etnocentrizmas",
            "Gamtos mokslai",
            "Internetas",
            "Istorija",
            "Kognityvinė terapija",
            "Laikmečio dvasia (Zeitgeist)",
            "Meilė",
            "Postmodernizmas",
            "Prisitaikymo teorija",
            "Psichoanalizė",
            "Psichologija",
            "Rasizmas",
            "Reklama",
            "Reliatyvizmas",
            "Religija",
            "Romantizmas",
            "Sociobiologija",
            "švietimas",
            "Vartotojiškumas",
            "Vergovė",
            "Altruizmas",
            "Autonomija",
            "Bioetika",
            "Deontologija",
            "Egoizmas",
            "Egzistencializmas",
            "Eksperimentinė  filosofija",
            "Epistemologija",
            "Estetika",
            "Etika",
            "Etikos istorija",
            "Eutanazija",
            "Falsifikacija",
            "Gyvūnų teisės",
            "Humanizmas",
            "Intuityvizmas",
            "Iracionalizmas",
            "Klaidingas neformaliosios logikos argumentavimas",
            "Klonavimas",
            "Logika",
            "Metafizika",
            "Pasekmizmas (konsekvencializmas)",
            "Prasmės teorija",
            "Sąmonės filosofija",
            "Šviečiamasis amžius",
            "Verslo etika",
            "žaidimų teorija",
            "Agnosticizmas",
            "Ateizmas",
            "Budizmas",
            "Daoizmas (taosizmas)",
            "Fundamentalizmas",
            "Hinduizmas ir brahmanizmas",
            "Islamas",
            "Judaizmas",
            "Katalikybė",
            "Kreacionizmas",
            "Krikščionybė",
            "Pomirtinis gyvenimas",
            "Protestantizmas",
            "Sekuliarumas",
            "Stačiatikybė",
            "Abiogenezė",
            "Antropinis principas",
            "Biologija",
            "Biologinė įvairovė",
            "Didžiojo sprogimo kosmologija",
            "Dirbtinis intelektas",
            "Evoliucija",
            "Juodosios skylės",
            "Kognityviniai mokslai",
            "Kvantinė mechanika",
            "Neuromokslai",
            "Reliatyvumas"
        };
    }
}
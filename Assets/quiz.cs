using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

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

    private List<string> textData;

    private List<string> answerList;
    int healths = 3;
    int scores = 0;

    public bool firstStart = true;
    int questionNo = 0;
    List<Question> mainGameQuestionList;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");
        ResetButtons();
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
        // Assigning questions to the buttons:
        var optionButtons = GameObject.FindGameObjectsWithTag("optionButtons");
        for (int i = 0; i < q.allAnswers.Count(); i++)
        {
            optionButtons[i].GetComponentInChildren<Text>().text = q.allAnswers[i].ToUpper();
        }
        correctAnswer = q.answer.ToUpper();
    }

    public void ButtonClick()
    {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        if (button.GetComponentInChildren<Text>().text == correctAnswer)
        {
            //Sets all buttons to be inactive so they won't be clicked again:
            foreach (var b in GameObject.FindGameObjectsWithTag("optionButtons"))
            {
                b.GetComponent<Button>().interactable = false;
            }
            button.GetComponentInChildren<Image>().color = Color.green;
            scores++;
            questionNo++;
            StartCoroutine("Wait");
        }
        else
        {
            //Sets button to be inactive so it won't be clicked again:
            button.GetComponent<Button>().interactable = false;
            button.GetComponentInChildren<Image>().color = Color.red;
            healths--;
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds((float)0.5);
        Start();
    }

    void ResetButtons()
    {
        Color btnColor = new Color(180, 176, 176, 255);
        // Finding all buttons with the same tag:
        var optionButtons = GameObject.FindGameObjectsWithTag("optionButtons");
        foreach (var button in optionButtons)
        {
            button.GetComponentInChildren<Image>().color = btnColor;
            button.GetComponent<Button>().interactable = true;
        }
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

    void readTextData(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (IsWebGL())
            ReadWebGLCSV(filePath);
        else
            textData = File.ReadAllLines(filePath).ToList();

    }
    IEnumerator ReadWebGLCSV(string filePath)
    {
        Debug.LogFormat("Opening: " + filePath);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(filePath))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                textData = webRequest.downloadHandler.text.Split('\n').ToList();
                Debug.LogFormat("Lines read: {0}", textData.Count);
            }
            else
                Debug.LogError("HTTP Error: " + webRequest.error);
        }
        Wait();
    }
    void ProcessWebGLCSV(string csvText)
    {
        textData = csvText.Split('\n').ToList();
        Debug.LogFormat("Lines read: {0}", textData.Count);
    }
    bool IsWebGL()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer;
    }

    bool IsNotEmpty(string answerField)
    {
        if (IsWebGL())
            return answerField.Length > 1;
        return answerField.Length > 0;
    }
    List<Question> AnswersAndQuestions()
    {
        List<Question> questionList = new List<Question>();
        // StartCoroutine(readTextData("definitions.csv"));
        readTextData("definitions.csv");
        answerList = new List<string>();
        foreach (string line in textData)
        {
            var lineElements = line.Split(';');

            // Add name of definition to answer list
            answerList.Add(lineElements[0]);



            Debug.LogFormat(
            "---\n{0}:{1};{2}:{3}",
            lineElements[0],
            lineElements[0].Length,
            lineElements[1],
            lineElements[1].Length
            );


            //If answer is not empty:
            if (IsNotEmpty(lineElements[1]))
            {
                Question q = new Question();
                q.answer = lineElements[0];
                q.question = lineElements[1];
                questionList.Add(q);
            }
        }

        foreach (Question q in questionList)
        {
            q.allAnswers = RandomAnswers(q.answer);
            ConsoleLogOutput(q);
        }
        Debug.Log(IsWebGL());
        return questionList;
    }

    List<string> RandomAnswers(string correctAnswer)
    {
        List<string> output = new List<string>();
        int i = 0;
        while (i < 3)
        {
            int randomIndex = UnityEngine.Random.Range(0, answerList.Count);
            if (
                (!output.Contains(answerList[randomIndex]))
                && (answerList[randomIndex] != correctAnswer)
            )
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
        Debug.LogFormat(
            "Question: {0}{1}Correct answer: {2}{1}{3}",
            q.question,
            newLine,
            q.answer,
            logAllAnswers
        );
    }

    public class Question
    {
        public string question { get; set; }
        public string answer { get; set; }
        public List<string> allAnswers { get; set; }
    }
}

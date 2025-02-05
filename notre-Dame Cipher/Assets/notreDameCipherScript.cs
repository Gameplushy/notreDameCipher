using KeepCoding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using RNG = UnityEngine.Random;

public class notreDameCipherScript : ModuleScript
{

    public GameObject[] Modes;
    public KMSelectable[] Arrows;
    public KMSelectable[] Buttons;
    public KMSelectable[] AnswerButtons;
    public MeshRenderer[] AnswerButtonsMat;
    public Material[] TilesMat;
    public TextMesh[] Displays;

    private string[,] displayTexts;
    private int page = 0;
    private char[,] matrix = new char[5, 5];
    private string startingWord, endingWord, lettersToPress;
    private int answerStep = 0;

    private void Start()
    {
        Modes[0].SetActive(true);
        Modes[1].SetActive(false);
        displayTexts = new string[,]
        {
            {"START WITH :","","END WITH :","" },
            {"ROSACE","","","" },
            {"VITRAIL","","","" },
            {"CROSS","","","" }
        };
    }

    public override void OnActivate()
    {
        Arrows.Assign(onInteract: SwapPage);
        Buttons.Assign(onInteract: ChangePhase);
        startingWord = displayTexts[0, 1] = WordList.wl[RNG.Range(0, WordList.wl.Length)];
        endingWord = displayTexts[0, 3] = WordList.wl[RNG.Range(0, WordList.wl.Length)];
        lettersToPress = new string(endingWord.Distinct().ToArray());
        ShowPage();
        InitiateMatrix();
        RosaceSetup();
        VitrailSetup();
        CrossSetup();
        ShowMatrix("Final");
        Log("Your final word is : {0}".Form(endingWord));
        AnswerButtons.Assign(onInteract: SubmitAttempt);
    }

    private void ShowMatrix(string prefix = "Current")
    {
        Log(prefix + " matrix :");
        for (int i = 0; i < 5; i++)
        {
            Log("{0}{1}{2}{3}{4}".Form(matrix[i, 0], matrix[i, 1], matrix[i, 2], matrix[i, 3], matrix[i, 4]));
        }
    }


    private void SwapPage(int i)
    {
        ButtonEffect(Arrows[i], 1, KMSoundOverride.SoundEffect.ButtonPress);
        if (i == 0) i = -1;
        page = (page + i % 4 + 4) % 4;
        ShowPage();
    }

    private void ShowPage()
    {
        for (int j = 0; j < 4; j++)
            Displays[j].text = displayTexts[page, j];
    }

    private void ChangePhase(int i)
    {
        ButtonEffect(Buttons[i], 1, KMSoundOverride.SoundEffect.ButtonPress);
        if (IsSolved) return;
        if (i == 1)
        {
            answerStep = 0;
            ChangeAllSubmissionTiles(0);
        }
        foreach (GameObject mode in Modes)
            mode.SetActive(!mode.activeSelf);
    }

    private void InitiateMatrix()
    {
        char letter = 'A';
        List<char> word = startingWord.ToList();
        word = word.Select(c => c == 'Q' ? 'Z' : c).ToList();
        word = word.Distinct().ToList();
        if (word.Count() % 2 == 0)
        {
            if (word.Contains('X'))
                word.Remove('X');
            else
                word.Insert(word.Count() / 2, 'X');
        }
        FillMatrix(word);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                while (word.Contains(letter) || (letter == 'Q' && word.Contains('Z'))) letter++;
                if (matrix[i, j] != char.MinValue) continue;
                matrix[i, j] = letter == 'Q' ? 'Z' : letter;
                letter++;
            }
        }
        ShowMatrix("Starting");
        Log("Your starting word is : {0}".Form(startingWord));
    }

    private readonly Vector2Int[] firstHalfSnake = new Vector2Int[] { new Vector2Int(2, 1), new Vector2Int(2, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(1, 3), new Vector2Int(1, 4), new Vector2Int(0, 4), new Vector2Int(0, 3), new Vector2Int(0, 2), new Vector2Int(0, 1), new Vector2Int(0, 0) };
    private readonly Vector2Int[] secondHalfSnake = new Vector2Int[] { new Vector2Int(2, 3), new Vector2Int(2, 4), new Vector2Int(3, 4), new Vector2Int(3, 3), new Vector2Int(3, 2), new Vector2Int(3, 1), new Vector2Int(3, 0), new Vector2Int(4, 0), new Vector2Int(4, 1), new Vector2Int(4, 2), new Vector2Int(4, 3), new Vector2Int(4, 4) };


    private void FillMatrix(List<char> word)
    {
        List<char> firstHalf = word.GetRange(0, word.Count() / 2).Rev(), secondHalf = word.GetRange(word.Count() / 2 + 1, word.Count() / 2);
        char middle = word[word.Count() / 2];
        matrix[2, 2] = middle;
        for (int i = 0; i < firstHalf.Count(); i++)
        {
            matrix[firstHalfSnake[i].x, firstHalfSnake[i].y] = firstHalf[i];
            matrix[secondHalfSnake[i].x, secondHalfSnake[i].y] = secondHalf[i];
        }
    }

    private void RosaceSetup()
    {
        Log("Rosace Cipher :");
        for (int i = 0; i < 3; i++)
        {
            Log("Step {0} :".Form(i + 1));
            int col = RNG.Range(0, 5);
            int row = RNG.Range(0, 5);
            bool isClockwise = RNG.Range(0, 2) == 1;
            displayTexts[1, i + 1] += (char)(col + 'A');
            displayTexts[1, i + 1] += row + 1;
            displayTexts[1, i + 1] += isClockwise ? '>' : '<';
            matrix = Ciphers.RosaceCipher(matrix, row - 2, col - 2, isClockwise);
            Log("Display is {0}".Form(displayTexts[1, i + 1]));
            ShowMatrix();
        }
    }

    private void VitrailSetup()
    {
        Log("Vitrail Cipher :");
        for (int i = 0; i < 3; i++)
        {
            Log("Step {0} :".Form(i + 1));
            int colstart, colend;
            do
            {
                colstart = RNG.Range(0, 5);
                colend = RNG.Range(0, 5);
            }
            while (colstart == colend);
            int overflow = RNG.Range(1, 6);
            displayTexts[2, i + 1] += colstart + 1;
            displayTexts[2, i + 1] += colend + 1;
            displayTexts[2, i + 1] += overflow;
            matrix = Ciphers.VitrailCipher(matrix, colstart, colend, overflow);
            Log("Display is {0}".Form(displayTexts[2, i + 1]));
            ShowMatrix();
        }
    }

    private void CrossSetup()
    {
        Log("Cross Cipher :");
        for (int i = 0; i < 3; i++)
        {
            Log("Step {0} :".Form(i + 1));
            int col = RNG.Range(0, 5);
            int row = RNG.Range(0, 5);
            displayTexts[3, i + 1] += col + 1;
            displayTexts[3, i + 1] += row + 1;
            matrix = Ciphers.CrossCipher(matrix, col, row);
            Log("Display is {0}".Form(displayTexts[3, i + 1]));
            ShowMatrix();
        }
    }

    private void SubmitAttempt(int i)
    {
        ButtonEffect(AnswerButtons[i], 1, KMSoundOverride.SoundEffect.ButtonPress);
        if (!IsSolved)
        {
            if (matrix[i / 5, i % 5] == lettersToPress[answerStep])
            {
                PlaySound("Burn");
                Log("You pressed {0} in row {1} column {2}, which is correct.".Form(matrix[i / 5, i % 5], (i / 5) + 1, (i % 5) + 1));
                AnswerButtonsMat[i].material = TilesMat[1];
                if (++answerStep == lettersToPress.Count())
                {
                    Solve("Module solved!");
                    PlaySound("Solve");
                    ChangeAllSubmissionTiles(1);
                }
            }
            else
            {
                Strike("You pressed {0} when {1} was expected... Strike!".Form(matrix[i / 5, i % 5], lettersToPress[answerStep]));
                PlaySound("Strike");
            }
        }

    }

    private void ChangeAllSubmissionTiles(int i)
    {
        foreach (MeshRenderer tile in AnswerButtonsMat)
        {
            tile.material = TilesMat[i];
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use [!{0} right/left] to press that arrow. Use [!{0} cycle] to cycle through all 4 screens. Use [!{0} submit C1 B2 E3 A2 A2] to navigate to the submit menu and press those coordinates. Use [!{0} cancel] to go exit submission phase and go back to page 1.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string input)
    {
        string command = input.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        string[] directions = new string[] { "LEFT", "PREV", "PREVIOUS", "RIGHT", "NEXT" };
        string[] coordinates = new string[] { "A1", "B1", "C1", "D1", "E1", "A2", "B2", "C2", "D2", "E2", "A3", "B3", "C3", "D3", "E3", "A4", "B4", "C4", "D4", "E4", "A5", "B5", "C5", "D5", "E5", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25" };
        //Optionally allows 1-based numerical indices
        if (directions.Contains(command))
        {
            yield return null;
            if (Modes[1].activeSelf)
            {
                Buttons[1].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
            ((Array.IndexOf(directions, command) < 3) ? Arrows[0] : Arrows[1]).OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else if (command == "CYCLE")
        {
            yield return null;
            if (Modes[1].activeSelf)
            {
                Buttons[1].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
            for (int i = 0; i < 4; i++)
            {
                Arrows[1].OnInteract();
                yield return "trycancel";
                yield return new WaitForSeconds(5);
            }
        }
        else if (Regex.IsMatch(command, @"^SUBMIT(\s+[A-E][1-5])+$", RegexOptions.CultureInvariant))
        {
            yield return null;
            if (Modes[0].activeSelf)
            {
                Buttons[0].OnInteract();
                yield return new WaitForSeconds(0.25f);
            }
            parameters.Remove("SUBMIT");
            foreach (string coord in parameters)
            {
                AnswerButtons[Array.IndexOf(coordinates, coord) % 25].OnInteract();
                yield return new WaitForSeconds(0.33f);
            }
        }
        else if (command == "CANCEL")
        {
            yield return null;
            if (Modes[1].activeSelf)
            {
                Buttons[1].OnInteract();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        while (!IsSolved)
        {
            if (Modes[0].activeSelf)
            {
                Buttons[0].OnInteract();
                yield return new WaitForSeconds(0.65f);
            }
            for (int i = 0; i < 25; i++)
            {
                if (matrix[i / 5, i % 5] == lettersToPress[answerStep])
                {
                    AnswerButtons[i].OnInteract();
                    yield return new WaitForSeconds(0.65f);
                    break;
                }
            }
            yield return null;
        }
    }
}


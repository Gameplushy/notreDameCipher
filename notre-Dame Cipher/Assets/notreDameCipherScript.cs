﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System;
using KeepCodingAndNobodyExplodes;
using RNG = UnityEngine.Random;

public class notreDameCipherScript : ModuleScript {

	public GameObject[] Modes;
	public KMSelectable[] Arrows;
	public KMSelectable[] Buttons;
	public KMSelectable[] AnswerButtons;
	public MeshRenderer[] AnswerButtonsMat;
	public Material[] TilesMat;
	public TextMesh[] Displays;

	private string[,] displayTexts;
	private int page = 0;
	private char[,] matrix = new char[5,5];
	private string startingWord, endingWord;
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

	protected override void OnActivate()
	{
		Arrows.Assign(onInteract: SwapPage);
		Buttons.Assign(onInteract: ChangePhase);
		startingWord = displayTexts[0, 1] = WordList.wl[RNG.Range(0, WordList.wl.Length)];
		endingWord = displayTexts[0, 3] = WordList.wl[RNG.Range(0, WordList.wl.Length)];
		ShowPage();
		InitiateMatrix();
		
		RosaceSetup();
		VitrailSetup();
		CrossSetup();
		ShowMatrix("Final");
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
		ButtonEffect(Arrows[i], 1,KMSoundOverride.SoundEffect.ButtonPress);
		if (i == 0) i = -1;
		page = (page+i % 4 + 4) % 4;
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
        if (i == 1)
        {
			answerStep = 0;
			ChangeAllSubmissionTiles(0);
        }
		foreach(GameObject mode in Modes)
			mode.SetActive(!mode.activeSelf);     
    }

	private void InitiateMatrix()
    {
		char letter = 'A';
		for(int i = 0; i < 5; i++)
        {
			matrix[2, i] = startingWord[i];
        }
		for(int i = 0; i < 5; i++)
        {
			if (i == 2) continue;
			for(int j = 0; j < 5; j++)
            {
				while (startingWord.Contains(letter)||(letter=='Q'&&startingWord.Contains('Z'))) letter++;
				matrix[i, j] = letter=='Q'?'Z':letter;
				letter++;
            }
        }
		ShowMatrix("Starting");
	}

	private void RosaceSetup()
    {
		Log("Rosace Cipher :");
		for(int i = 0; i < 3; i++)
        {
			Log("Step {0} :".Form(i + 1));
			int col = RNG.Range(0, 5);
			int row = RNG.Range(0, 5);
			bool isClockwise = RNG.Range(0, 2) == 1;
			displayTexts[1, i + 1] += (char)(col + 'A');
			displayTexts[1, i + 1] += row+1;
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
		for(int i = 0; i < 3; i++)
        {
			Log("Step {0} :".Form(i + 1));
			int col = RNG.Range(0, 5);
			int row = RNG.Range(0, 5);
			displayTexts[3, i + 1] += col+1;
			displayTexts[3, i + 1] += row+1;
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
			if (matrix[i / 5, i % 5] == endingWord[answerStep])
			{
				PlaySound("Burn");
				Log("You pressed {0} in row {1} column {2}, which is correct.".Form(matrix[i / 5, i % 5], (i / 5) + 1, (i % 5) + 1));
				AnswerButtonsMat[i].material = TilesMat[1];
				if (++answerStep == 5)
				{
					Solve("Module solved!");
					PlaySound("Solve");
					ChangeAllSubmissionTiles(1);
				}
			}
			else
			{
				Strike("You pressed {0} when {1} was expected... Strike!".Form(matrix[i / 5, i % 5],endingWord[answerStep]));
				PlaySound("Strike");
			}
		}

    }

	private void ChangeAllSubmissionTiles(int i)
    {
		foreach(MeshRenderer tile in AnswerButtonsMat)
        {
			tile.material = TilesMat[i];
        }
    }

}

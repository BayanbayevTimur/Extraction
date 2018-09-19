using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileMapGenerator : MonoBehaviour {

	private int width, height, scans, mines, Score; 
	private int MaxReward, MedReward, MinReward, LowReward;
	private int[] RandNumbers;

	[SerializeField] private Tilemap TileMap;
	[SerializeField] private Tilemap TileMapTop;
	[SerializeField] private Tilemap HighlightMap;

	[SerializeField] private TileBase BasicTile;
	[SerializeField] private TileBase MaxGoldTile;
	[SerializeField] private TileBase MedGoldTile;
	[SerializeField] private TileBase SmallGoldTile;
	[SerializeField] private TileBase Highlight;

	[SerializeField] private Text MaxGold;
	[SerializeField] private Text MedGold;
	[SerializeField] private Text LowGold;
	[SerializeField] private Text MinGold;
	[SerializeField] private Text scoreText;
	[SerializeField] private Text ScanText;
	[SerializeField] private Text MineText;
	[SerializeField] private Text GeneralText;

	[SerializeField] private Slider slider;


	private enum GameMode{
		SCAN_MODE,
		MINE_MODE
	};
	GameMode CurrentMode;
		
	private Ray ray;
	private Vector3Int MouseGridCoord;
	private Vector3Int LastMouseLoc;

	private Coroutine ErrorCoroutine;

	void Start () 
	{
		CurrentMode = GameMode.SCAN_MODE;
		RandNumbers = new int[UnityEngine.Random.Range(6,8)];
		MaxReward = Mathf.FloorToInt (UnityEngine.Random.Range (5000, 10000));

		Score = 0;
		scans = 6;
		mines = 3;
		width = 16;
		height = 16;

		MedReward = Mathf.FloorToInt (MaxReward / 2);
		MinReward = Mathf.FloorToInt (MaxReward / 4);
		LowReward = Mathf.FloorToInt (UnityEngine.Random.Range (MaxReward / 16, MaxReward / 8));

		Debug.Log (LowReward);
		for (int i = 0; i < RandNumbers.Length; i++)
		{
			RandNumbers [i] = UnityEngine.Random.Range (0, width * height - 1);
		}

		GenerateMap ();
	}

	void Update () 
	{
		ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		MouseGridCoord = TileMap.WorldToCell(new Vector3(ray.origin.x, ray.origin.y, 0));
		scoreText.text = "Your Gold: " + Score.ToString();
		ScanText.text = "SCAN MODE: " + scans.ToString ();
		MineText.text = "EXTRACT MODE: " + mines.ToString ();

		MaxGold.text = MaxReward.ToString () + " Gold";
		MedGold.text = MedReward.ToString () + " Gold";
		MinGold.text = MinReward.ToString () + " Gold";
		LowGold.text = LowReward.ToString () + " Gold";

		FakeMouseOver ();

		if (slider.value == 1) 
		{
			CurrentMode = GameMode.SCAN_MODE;
			ScanText.color = new Color32(0x2D, 0xFF, 0x00, 0xFF);
			MineText.color = new Color32(0x00, 0xF0, 0xFF, 0xFF);
		}
		if (slider.value == 0)
		{
			CurrentMode = GameMode.MINE_MODE;
			MineText.color = new Color32(0x2D, 0xFF, 0x00, 0xFF);
			ScanText.color = new Color32(0x00, 0xF0, 0xFF, 0xFF);
		}

		if (Input.GetMouseButtonDown (0))
		{
			if (CurrentMode == GameMode.MINE_MODE) 
			{
				if (TileMap.HasTile (MouseGridCoord)) 
				{
					if (mines > 0) 
					{
						if (TileMap.GetTile (MouseGridCoord) == BasicTile) 
						{
							//Score += Mathf.FloorToInt(UnityEngine.Random.Range(MinReward/8, MaxReward/8));
							Score += LowReward;
						}
						if (TileMap.GetTile (MouseGridCoord) == SmallGoldTile) 
						{
							//Score += Mathf.FloorToInt(UnityEngine.Random.Range(MinReward/4, MaxReward/4));
							Score += MinReward;
							LowerSurrounding (MouseGridCoord, 2, 2, TileMap, BasicTile);
						}
						if (TileMap.GetTile (MouseGridCoord) == MedGoldTile) 
						{
							//Score += Mathf.FloorToInt(UnityEngine.Random.Range(MinReward/2, MaxReward/2));
							Score += MedReward;
							LowerSurrounding (MouseGridCoord, 2, 2, TileMap, BasicTile);
						}
						if (TileMap.GetTile (MouseGridCoord) == MaxGoldTile) 
						{
							//Score += Mathf.FloorToInt(UnityEngine.Random.Range(MinReward, MaxReward));
							Score += MaxReward;
							LowerSurrounding (MouseGridCoord, 2, 2, TileMap, BasicTile);

						}
						mines--;
					}
				}
				if (mines == 0) 
				{
					if (ErrorCoroutine != null)
					{
						StopCoroutine (ErrorCoroutine);
					}
					GeneralText.text = "No more mining available, your final score is " + Score.ToString () + " gold.";
				}
			} 

			if (TileMap.HasTile (MouseGridCoord))
			{
				if (CurrentMode == GameMode.SCAN_MODE)
				{
					if (TileMapTop.HasTile (MouseGridCoord)) 
					{
						if (scans > 0)
						{
							if (ErrorCoroutine != null)
							{
								StopCoroutine (ErrorCoroutine);
							}
							RemoveSurrounding (MouseGridCoord, 1, 1, TileMapTop, null);
							ErrorCoroutine = StartCoroutine (SetBlankText());
							GeneralText.text = "Scanned";
							scans--;
						}
						else
						{
							if (ErrorCoroutine != null)
							{
								StopCoroutine (ErrorCoroutine);
							}
							GeneralText.text = "No more scanning available.";
							ErrorCoroutine = StartCoroutine (SetBlankText());
						}
					}
				}
			}
		}
	}

	void GenerateMap()
	{
		for (int i = 0; i < height; i++) 
		{
			for (int j = 0; j < width; j++) 
			{
				Vector3Int Position = new Vector3Int (j, i, 0);

				TileMap.SetTile (Position, BasicTile);
				TileMapTop.SetTile (Position, BasicTile);
			}
		}

		for (int i = 0; i < RandNumbers.Length; i++)
		{
			int TempX = (RandNumbers [i]) % width;
			int TempY = Mathf.FloorToInt((RandNumbers [i]) / height); 

			Vector3Int TempIVec = new Vector3Int (TempX, TempY, 0);
			TileMap.SetTile (TempIVec, MaxGoldTile);

			for (int y = TempY + 1; y > (TempY - 2); y--) 
			{
				if(y > -1 && y < height)
				{
					for (int x = TempX - 1; x < (TempX + 2); x++) 
					{
						if (x > -1 && x < width)
						{
							TempIVec = new Vector3Int (x, y, 0);
							if (TileMap.GetTile (TempIVec) != MaxGoldTile) 
							{
								TileMap.SetTile (TempIVec, MedGoldTile);
							}
						}
					}
				}
			}

			for (int y = TempY + 2; y > (TempY - 3); y--) 
			{
				if (y > -1 && y < height) 
				{
					for (int x = TempX - 2; x < (TempX + 3); x++) 
					{
						if (x > -1 && x < width)
						{
							TempIVec = new Vector3Int (x, y, 0);
							if (TileMap.GetTile (TempIVec) != MaxGoldTile && TileMap.GetTile (TempIVec) != MedGoldTile)
							{
								TileMap.SetTile (TempIVec, SmallGoldTile);
							}
						}
					}
				}
			}
		}
	}

	void RemoveSurrounding(Vector3Int Position, int x, int y, Tilemap TileMap, TileBase Tile)
	{
		for (int i = Position.y + y; i > (Position.y - (y+1)); i--) 
		{
			if (i > -1 && i < height) 
			{
				for (int j = Position.x - x; j < (Position.x + (x+1)); j++) 
				{
					if (j > -1 && j < width)
					{
						Vector3Int TempIVec = new Vector3Int (j, i, 0);
						TileMap.SetTile (TempIVec, Tile);
					}
				}
			}
		}
	}

	void LowerSurrounding(Vector3Int Position, int x, int y, Tilemap TileMap, TileBase Tile)
	{
		for (int i = Position.y + y; i > (Position.y - (y+1)); i--) 
		{
			if (i > -1 && i < height) 
			{
				for (int j = Position.x - x; j < (Position.x + (x+1)); j++) 
				{
					if (j > -1 && j < width)
					{
						Vector3Int TempIVec = new Vector3Int (j, i, 0);
						//TileMap.SetTile (TempIVec, Tile);
						if (TileMap.GetTile (TempIVec) == SmallGoldTile) 
						{
							TileMap.SetTile (TempIVec, BasicTile);
						}
						if (TileMap.GetTile (TempIVec) == MedGoldTile) 
						{
							TileMap.SetTile (TempIVec, SmallGoldTile);
						}
						if (TileMap.GetTile (TempIVec) == MaxGoldTile) 
						{
							TileMap.SetTile (TempIVec, MedGoldTile);
						}
					}
				}
			}
		}
	}

	IEnumerator SetBlankText()
	{
		yield return new WaitForSeconds (2);
		GeneralText.text = "";
	}

	private void FakeMouseOver()
	{
		if (LastMouseLoc != MouseGridCoord) 
		{
			if (HighlightMap.HasTile (LastMouseLoc)) 
			{
				HighlightMap.SetTile (LastMouseLoc, null);
			}
			LastMouseLoc = MouseGridCoord;
		}

		if (TileMap.HasTile (MouseGridCoord)) 
		{
			HighlightMap.SetTile (MouseGridCoord, Highlight);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CommandLine
{
	public class RecipeBook
	{
		public static RecipeBook instance = new RecipeBook();

		static string recipePath = "recipes";

		public Recipe[] recipes;
		public string[] existingTags;
		public int[] tagCount;
		public bool isOpen = true;

		
		//run on execute
		public static void Main()
		{

			//instance.isOpen = true;
			instance.GetData();
			Console.WriteLine("RECIPE BOOK" + System.Environment.NewLine);
			instance.PrintCommandList();

			while (instance.isOpen == true)
			{
				instance.InputCommand();
			}

			Console.WriteLine("Recipe Book Closed");
		}

		//fill data arrays and dictionaries from text files
		void GetData()
		{
			//var txtFiles = Directory.EnumerateFiles(recipePath, "*.txt");
			string[] filePaths = Directory.GetFiles(recipePath);

			recipes = new Recipe[filePaths.Length];
			for(int i = 0; i < filePaths.Length; i++)
			{
				string txt = System.IO.File.ReadAllText(filePaths[i]);
				//parse each index to array
				string[,] d = ConvertToStringArray(txt);
				//get data from array
				string name = d[1, 0];
				int time = Convert.ToUInt16(d[1, 1]);
				string[] tags = d[1, 2].Split(',');
				string[] ingredients = d[1, 3].Split(',');
				string[] seasoning = d[1, 4].Split(',');
				string instructions = d[1, 5];
				//create new Recipe and add to recipe
				recipes[i] = new Recipe(name,time,tags,ingredients,seasoning,instructions);
			}

			GetTagData();
		}

		//converts the inputed string into an array
		string[,] ConvertToStringArray(string input, int initialYIndex = 0, char lineBreak = '\r', char unitBreak = '\t')
		{
			//romve any extra characters from the string
			//string extra = "\r";
			//input = input.Replace(extra, "");

			int top = initialYIndex - 1;
			if (top < 0)
			{
				top = 0;
			}

			string[] rows = input.Split(lineBreak);
			string[] topRow = rows[top].Split(unitBreak);
			int xLength = topRow.Length;
			int yLength = rows.Length - initialYIndex;
			string[,] output = new string[xLength, yLength];
			for (int y = 0; y < yLength; y++)
			{
				string[] splitRow = rows[y + initialYIndex].Split(unitBreak);
				for (int x = 0; x < xLength; x++)
				{
					output[x, y] = splitRow[x];
				}
			}
			return output;
		}

		void InputCommand(string question = "INPUT COMMAND...")
		{
			bool responded = false;
			while (responded == false)
			{
				Console.WriteLine(question + " [c -view commands]");
				string response = Console.ReadLine();

				//separate input and command
				string[] ray = response.Split(':');
				string command = ray[0];
				string input = string.Empty;
				//if input set input
				if (ray.Length > 1)
				{
					input = ray[1];
				}

				switch (command)
				{
					case "c":
						responded = true;
						//run command with input if necessary
						//print command list
						PrintCommandList();
						break;
					case "r":
						responded = true;
						//if input is null print all recipes else print specified recipe
						PrintRecipe(input);
						break;
					case "t":
						responded = true;
						PrintWithTags(input);
						break;
					case "close":
						responded = true;
						instance.isOpen = false;
						break;
					default:
						Console.WriteLine("INVALID COMMAND");
						break;
				}
			}
		}
		
		//prints recipe list or specific recipe
		void PrintRecipe(string recipeName)
		{
			if(recipeName == string.Empty)
			{
				//print recipe list
				for(int i = 0; i < recipes.Length; i++)
				{
					Console.WriteLine('\t' + recipes[i].name + ": " + i);
				}

			}else
			{
				//find inputed recipe
				int n;
				bool isIndex = int.TryParse(recipeName, out n);

				if (isIndex == true)
				{
					if(n < recipes.Length)
					{
						Console.WriteLine(System.Environment.NewLine + recipes[n].OutputData());
					}else
					{
						Console.WriteLine("No recipe found at that index");
					}
				}
				else //isName
				{
					bool isFound = false;
					for (int i = 0; i < recipes.Length; i++)
					{
						if (recipes[i].name == recipeName)
						{
							if (isFound == false)
							{
								//add spacer on before first recipe
								Console.WriteLine();
								isFound = true;
							}
							//write recipe data
							Console.WriteLine(recipes[i].OutputData());
						}
					}

					//if cant find
					if (isFound == false)
					{
						Console.WriteLine("No recipe found by that name");
					}
				}
			}
			//wait for user input
			instance.InputCommand();
		}

		void GetTagData()
		{
			//fill list with all different tags
			List<string> tagList = new List<string>();


			foreach(Recipe r in recipes)
			{
				foreach(string t in r.tags)
				{
					//if not already on existingTags add
					if(!tagList.Contains(t))
					{
						tagList.Add(t);
					}
				}
			}

			existingTags = new string[tagList.Count];
			tagCount = new int[tagList.Count];

			for(int i = 0; i < tagList.Count; i++)
			{
				existingTags[i] = tagList[i];
				tagCount[i] = 0;
			}

			
			//fill tagCount
			foreach(Recipe r in recipes)
			{
				foreach(string t in r.tags)
				{
					for(int i = 0; i < existingTags.Length; i++)
					{
						if(t == existingTags[i])
						{
							tagCount[i]++;
							break;
						}
					}
				}
			}

		}

		void PrintWithTags(string tag)
		{
			if(tag == string.Empty)
			{
				//print list of tags
				for(int i = 0;i< existingTags.Length; i++)
				{
					Console.WriteLine('\t' + existingTags[i] + ": " + tagCount[i]);
				}
				Console.WriteLine();
			}else
			{
				bool isFound = false;
				//print recipes with tag
				//look through recipes and print everyone that contains the tag
				for(int i = 0; i < recipes.Length; i++)
				{
					foreach(string t in recipes[i].tags)
					{
						if(t == tag)
						{
							isFound = true;
							PrintRecipe(i.ToString());
							break;
						}
					}
				}

				if(isFound == false)
				{
					Console.WriteLine("No recipe found with tag");
				}
			}

			InputCommand();
		}

		//prints list of commands
		void PrintCommandList()
		{
			string commands = "	c -view commands" + System.Environment.NewLine + "	r -view all recipe names" + System.Environment.NewLine + "	r:NAME/INDEX -view specific recipe" + System.Environment.NewLine + "	t -view list of tags" + System.Environment.NewLine + "	t:TAG -view all recipes with given tag" + System.Environment.NewLine  + "	close -close application" + System.Environment.NewLine;
			Console.WriteLine(commands);
			instance.InputCommand();
		}

	}

	//store recipe data
	public class Recipe
	{
		public string name {get; private set; }
		public int time { get; private set; }
		public string[] tags { get; private set; }
		public string[] ingredients { get; private set; }
		public string[] seasoning { get; private set; }
		public string instructions { get; private set; }

		//constructor
		public Recipe(string inputName, int inputTime, string[] inputTags, string[] inputIngredients, string[] inputSeasoning, string inputInstructions)
		{
			name = inputName;
			time = inputTime;
			tags = inputTags;
			ingredients = inputIngredients;
			seasoning = inputSeasoning;
			instructions = inputInstructions;
		}

		//returns data in single string to be outputed to console
		public string OutputData()
		{
			string output;

			string allTags = "";
			for(int i = 0; i < tags.Length; i++)
			{
				allTags += tags[i];
				if(i != tags.Length - 1)
				{
					allTags += ", ";
				}
			}

			string allIngrediants = "";
			for(int i = 0; i < ingredients.Length; i++)
			{
				allIngrediants += ingredients[i];
				if (i != ingredients.Length - 1)
				{
					allIngrediants += ", ";
				}
			}

			string allSeasoning = "";
			for (int i = 0; i < seasoning.Length; i++)
			{
				allSeasoning += seasoning[i];
				if (i != seasoning.Length - 1)
				{
					allSeasoning += ", ";
				}
			}

			output = name + System.Environment.NewLine + "	Time: " + time + " min" + System.Environment.NewLine + "	Tags: " + allTags + System.Environment.NewLine + "	Ingredients: " + allIngrediants + System.Environment.NewLine + "	Seasoning: " + allSeasoning + System.Environment.NewLine + "	Instructions: " + instructions + System.Environment.NewLine;

			return output;
		}
	}
}

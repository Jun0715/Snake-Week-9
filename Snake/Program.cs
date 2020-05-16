using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.IO;
using System.Media;

namespace Snake
{
	/// <summary>
	/// Define the position for Food, Snake Head and obstacle
	/// It specify the row and column of its position
	/// </summary>
	struct Position
	{
		public int row;
		public int col;
		public Position(int row, int col)//Constructor for specify the position of the Item
		{
			this.row = row;
			this.col = col;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			string name;
			do
			{
				Console.Write("What is your name with 10 character: ");//Prompt for the player's name
				name = Console.ReadLine();
			} while (name == "" || name.Length >10);


			Console.Clear();
			
			byte right = 0;//Define the direction of right of snake
			byte left = 1;//Define the direction of left of snake
			byte down = 2;//Define the direction of down of snake
			byte up = 3;//Define the direction of up of snake
			int lastFoodTime = 0;//Define the life time of the food 
			int foodDissapearTime = 20000;//Define the disappear time of the food to 20 seconds which made the food disappears slower
			int negativePoints = 0;//Define the score need to be minus if food disappear
			int snakebody_size_origin = 3;//Define the initial length of snake
			int win_score = 500;//Define the score of winning
			
			int lastspecialFoodTime = 0;//Define the life time of the special food 
			int specialfoodDissapearTime = 10000;//Define the disappear time of the special food to 10 seconds which made the food disappears slower
			
			int increment_length = 0;
			
			bool specialfoodflag = false;	//fix the specialfood cannot eating 2 times;
			
			int life = 3;
			
			int current_level = 1;
			
			//create object
			SoundPlayer back_player = new SoundPlayer("Snakesongv2.wav");
			SoundPlayer eat_player = new SoundPlayer("eat.wav");
			SoundPlayer gameover_player = new SoundPlayer("gameover.wav");
			SoundPlayer win_player = new SoundPlayer("win.wav");
			
			//play blackground music by looping
			back_player.PlayLooping();
			
			//Create an array of the coordinates
			Position[] directions = new Position[]
			{
				new Position(0, 1), // right
               			new Position(0, -1), // left
               			new Position(1, 0), // down
                		new Position(-1, 0), // up
            		};
			
			double sleepTime = 100;//Define the speed of the sname
			double nextspeed = sleepTime;
			int direction = right;//Define the moving direction of the snake  at beginning
			Random randomNumbersGenerator = new Random();//Define the random number generator
			Console.BufferHeight = Console.WindowHeight;//Set the screen size of the game to the console size
			lastFoodTime = Environment.TickCount;//Set the timer of the food
			lastspecialFoodTime = Environment.TickCount;//Set the timer of the food
			
			//Initialize the length of the "snake tail"
			Queue<Position> snakeElements = new Queue<Position>();
			for (int i = 0; i <= snakebody_size_origin; i++)
			{
				snakeElements.Enqueue(new Position(1, i));
			}
			
			//Initialize the position of the obstacles
			List<Position> obstacles = new List<Position>();
			for (int i = 0; i < 5; ++i)
			{
				Position obstacle;
				do
				{
					obstacle = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
						randomNumbersGenerator.Next(0, Console.WindowWidth-2));//define a random place the obstacle into the game field
				}
				while (snakeElements.Contains(obstacle) ||
						obstacles.Contains(obstacle));
				obstacles.Add(obstacle);
			}
			
			foreach (Position obstacle in obstacles)
			{
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.SetCursorPosition(obstacle.col, obstacle.row);
				Console.Write("\u2593");
			}
				
			//position of the food
			Position food;
			//randomize the position of the food
			do
			{
				food = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
					randomNumbersGenerator.Next(0, Console.WindowWidth-3));
			}
			//while the snake or the obstacles touches the food, its position will always changed
			while (snakeElements.Contains(food) || obstacles.Contains(food));
			Console.SetCursorPosition(food.col, food.row);//set the column and row position of the food
			Console.ForegroundColor = ConsoleColor.Yellow;//set the foreground color to yellow
			Console.Write("\u2665\u2665");
			
			//position of the special food
			Position specialfood;
			specialfoodflag = true;
			//randomize the position of the special food
			do
			{
				specialfood = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
					randomNumbersGenerator.Next(0, Console.WindowWidth-2));
			}
			//while the snake or the obstacles touches the food, its position will always changed
			while (snakeElements.Contains(specialfood) || obstacles.Contains(specialfood) || (food.row == specialfood.row && food.col == specialfood.col));
			Console.SetCursorPosition(specialfood.col, specialfood.row);//set the column and row position of the food
			Console.ForegroundColor = ConsoleColor.Yellow;//set the foreground color to yellow
			Console.Write("\u2736");
			
			foreach (Position position in snakeElements)
			{
				Console.SetCursorPosition(position.col, position.row);//set the column and row position of snake elements
				Console.ForegroundColor = ConsoleColor.DarkGray;//set the foreground color to dark grey
				Console.Write("*");//this is the body of snake
			}

			while (true)
			{
				negativePoints++;

				if (Console.KeyAvailable)	//To gets a value indicating whether a key press is available in the input stream.
				{
					ConsoleKeyInfo userInput = Console.ReadKey(true);
					if (userInput.Key == ConsoleKey.LeftArrow)
					{
						if (direction != right) direction = left;
					}
					if (userInput.Key == ConsoleKey.RightArrow)
					{
						if (direction != left) direction = right;
					}
					if (userInput.Key == ConsoleKey.UpArrow)
					{
						if (direction != down) direction = up;
					}
					if (userInput.Key == ConsoleKey.DownArrow)
					{
						if (direction != up) direction = down;
					}
				}

				Position snakeHead = snakeElements.Last();//Set the head of the snake to the last item of the body
				Position nextDirection = directions[direction];//Define the next direction of snake move after user enter the arrow key

				Position snakeNewHead = new Position(snakeHead.row + nextDirection.row,
					snakeHead.col + nextDirection.col);//Set the next body movind direction to the head of snake

				if (snakeNewHead.col < 0) snakeNewHead.col = Console.WindowWidth - 2;//if the snake head hit the left wall, the snake will pass through it and appear on the right wall
				if (snakeNewHead.row < 0) snakeNewHead.row = Console.WindowHeight - 1;//if the snake head hit the top wall, the snake will pass through it and appear on the bottom wall
				if (snakeNewHead.row >= Console.WindowHeight) snakeNewHead.row = 0;//if the snake head hit the bottom wall, the snake will pass through it and appear on the top wall
				if (snakeNewHead.col >= Console.WindowWidth-1) snakeNewHead.col = 0;//if the snake head hit the right wall, the snake will pass through it and appear on the left wall

				int current_score = (snakeElements.Count - increment_length - snakebody_size_origin -1) * 100 - negativePoints; //Calculate the score of the player
				current_score = Math.Max(current_score, 0) - 1;   //round the score to int

				if (current_score < 0)//if score less than 0, score equal 0
				{
					current_score = 0;
				}
				string score_title = "Score: ";                                                                           
				Console.SetCursorPosition((Console.WindowWidth - score_title.Length - 4), 0);//Set position to top right corner  
				Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red	
				Console.Write(score_title + current_score.ToString().PadLeft(4, '0'));//Display the text
				
				string level_title = "Level: ";
				Console.SetCursorPosition(0, 0);
				Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red	
				Console.Write(level_title + current_level.ToString().PadLeft(3, '0'));//Display the text
				
				string win_title = "Next level Score: ";
				Console.SetCursorPosition(20, 0);//Set position to top right corner  
				Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red	
				Console.Write(win_title + win_score.ToString().PadLeft(3, '0'));//Display the text


				string length_title = "Size: ";
				Console.SetCursorPosition(50, 0);//Set position to top right corner  
				Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red	
				Console.Write(length_title + snakeElements.Count.ToString().PadLeft(3, '0'));//Display the text	


				string negative_title = "Speed: ";
				Console.SetCursorPosition(70, 0);//Set position to top right corner  
				Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red	
				Console.Write(negative_title + Math.Abs(Math.Round(sleepTime) - 100).ToString().PadLeft(3, '0'));//Display the text

				
				string life_title = "Life: ";
				Console.SetCursorPosition(90, 0);
				Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red	
				Console.Write(life_title + life.ToString().PadLeft(3, '0'));//Display the text
				
				//winning requirement
				if (current_score / win_score > 0 && current_score>0)
				{
					increment_length += 1;
					int currentlength = snakeElements.Count - increment_length;
					current_level += 1;
					negativePoints = 0;
					win_score += 100;
					if (sleepTime <= 10) sleepTime = 10;
					else
					{
						nextspeed -= 10;
						sleepTime = nextspeed;
					}
					foodDissapearTime -= 100;
					for (int i = 1; i <(currentlength-snakebody_size_origin); ++i)
					{
						Position last = snakeElements.Dequeue();//Define the last position of the snake body
						Console.SetCursorPosition(last.col, last.row);//Get the curser of that position
						Console.Write(" ");//Display space at that field

					}
					foreach (Position obstacle in obstacles)
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.SetCursorPosition(obstacle.col, obstacle.row);
						Console.Write(" ");
					}

					obstacles.Clear();
					
					for (int i = 0; i < 5; ++i)
					{
						Position obstacle;
						do
						{
							obstacle = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
								randomNumbersGenerator.Next(0, Console.WindowWidth-2));//define a random place the obstacle into the game field
						}
						while (snakeElements.Contains(obstacle) ||
								obstacles.Contains(obstacle));
						obstacles.Add(obstacle);
					}

					foreach (Position obstacle in obstacles)
					{
						Console.ForegroundColor = ConsoleColor.Cyan;
						Console.SetCursorPosition(obstacle.col, obstacle.row);
						Console.Write("\u2593");
					}
					
					//randomize the position of the special food
					specialfoodflag = true;
					do
					{
						specialfood = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
							randomNumbersGenerator.Next(0, Console.WindowWidth-2));
					}
					//while the snake or the obstacles touches the food, its position will always changed
					while (snakeElements.Contains(specialfood) || obstacles.Contains(specialfood) || (food.row == specialfood.row && food.col == specialfood.col));
					Console.SetCursorPosition(specialfood.col, specialfood.row);//set the column and row position of the food
					Console.ForegroundColor = ConsoleColor.Yellow;//set the foreground color to yellow
					Console.Write("\u2736");

					lastspecialFoodTime = Environment.TickCount;//gets the millisecond count from the computer's system timer
				}
				
				if (snakeElements.Contains(snakeNewHead) || obstacles.Contains(snakeNewHead))//if the head of the snake hit the body of snake or obstacle
				{
					if (life > 0)
					{
						life -= 1;
						
						List<Position> tempobstacles = new List<Position>();
						foreach (Position obstacle in obstacles)
						{
							if (!(obstacle.col == snakeNewHead.col && obstacle.row == snakeNewHead.row)) tempobstacles.Add(obstacle);
						}

						obstacles.Clear();
						foreach (Position obstacle in tempobstacles)
						{
							obstacles.Add(obstacle);
						}
						
						Position last = snakeElements.Dequeue();//Define the last position of the snake body
						Console.SetCursorPosition(last.col, last.row);//Get the curser of that position
						Console.Write(" ");//Display space at that field
					}
					else
					{
						gameover_player.Play();
						string msg = "Game over!";//Game over message
						string level_msg = "Your level are: " + current_level;//Current level message
						string score_msg = "Your points are: " + current_score;//Current score message
						string exit_msg = "Press enter to exit the game.";//Exit message
						Console.SetCursorPosition((Console.WindowWidth - msg.Length) / 2, (Console.WindowHeight / 2));  //Set the cursor position to the beginning
						Console.ForegroundColor = ConsoleColor.Red;//Set the font color to red
						Console.WriteLine(msg);//Display the text
						Console.SetCursorPosition((Console.WindowWidth - level_msg.Length) / 2, (Console.WindowHeight / 2) + 1);
						Console.Write(level_msg);//Display the score
						Console.SetCursorPosition((Console.WindowWidth - score_msg.Length) / 2, (Console.WindowHeight / 2) + 2);
						Console.Write(score_msg);//Display the score
						Console.SetCursorPosition((Console.WindowWidth - exit_msg.Length) / 2, (Console.WindowHeight / 2) + 3);
						Console.Write(exit_msg);//Display the exit message
						string fullPath = Directory.GetCurrentDirectory() + "/score.txt";

						int finalscore = 0;
						for (int index = 2; index <= current_level; ++index)
						{
							finalscore += ((index - 2) * 100) + 500;
						}
						finalscore += current_score;

						if (!File.Exists(fullPath))
						{
							using (StreamWriter writer = new StreamWriter(fullPath))
							{
								writer.WriteLine(name + " " + current_level.ToString().PadLeft(2, '0') + " " + current_score.ToString().PadLeft(3, '0') + " " + finalscore.ToString().PadLeft(3, '0'));
							}
						}
						else
						{
							using (StreamWriter writer = File.AppendText(fullPath))
							{
								writer.WriteLine(name + " " + current_level.ToString().PadLeft(2, '0') + " " + current_score.ToString().PadLeft(3, '0') + " " + finalscore.ToString().PadLeft(3, '0'));
							}
						}
						
						int count;

						count = File.ReadLines(fullPath).Count();

						string[,] arr = new string[count, 4];
						int j;
						using (StreamReader sr = File.OpenText(fullPath))
						{
							string s = "";
							count = 0;
							while ((s = sr.ReadLine()) != null)
							{
								string[] data = s.Split(' ');
								j = 0;
								foreach (string d in data)
								{
									arr[count, j] = d;
									j += 1;
								}
								count += 1;
							}
						}

						for (int i = 0; i < arr.GetLength(0) - 1; i++)
						{
							for (j = i; j < arr.GetLength(0); j++)
							{
								if (int.Parse(arr[i, 3]) < int.Parse(arr[j, 3])) // sort by descending by first index of each row
								{
									for (int k = 0; k < arr.GetLength(1); k++)
									{
										var temp = arr[i, k];
										arr[i, k] = arr[j, k];
										arr[j, k] = temp;
									}
								}
							}
						}

						string rank_title = "     Name | Rank | Level | Score";
						Console.SetCursorPosition((Console.WindowWidth - rank_title.Length) / 2, (Console.WindowHeight / 2) + 4);
						Console.WriteLine(rank_title);

						for (int i = 0; i < count; ++i)
						{
							if (i < 5)
							{
								string record_msg = String.Format("{0,10} | {1,4} | {2,5} | {3,5}", arr[i, 0], (i + 1), arr[i, 1], arr[i, 3]);
								Console.SetCursorPosition((Console.WindowWidth - record_msg.Length) / 2, (Console.WindowHeight / 2) + 5 + i);
								Console.Write(record_msg);
							}
							else continue;
						}
						Console.ReadKey(true);
						return;
					}
				}

				Console.SetCursorPosition(snakeHead.col, snakeHead.row);//set the column and row position of the snake head
				Console.ForegroundColor = ConsoleColor.DarkGray;//set the foreground color to dark grey
				Console.Write("*");//this is the body of snake

				snakeElements.Enqueue(snakeNewHead);
				Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
				Console.ForegroundColor = ConsoleColor.Gray;
				if (direction == right) Console.Write(">");//the snake head turns right
				if (direction == left) Console.Write("<");//the snake head turns left
				if (direction == up) Console.Write("^");//the snake head turns up
				if (direction == down) Console.Write("v");//the snake head turns down
				
				if (snakeNewHead.col == specialfood.col && snakeNewHead.row == specialfood.row && (Environment.TickCount - lastspecialFoodTime) < specialfoodDissapearTime && specialfoodflag == true)
				{
					eat_player.PlaySync();
					back_player.PlayLooping();
					specialfoodflag = false;
					life += 1;
					for (int i = 0; i < (snakebody_size_origin+current_level)-1; ++i)
					{
						Console.SetCursorPosition(snakeHead.col, snakeHead.row);//set the column and row position of the snake head
						Console.ForegroundColor = ConsoleColor.DarkGray;//set the foreground color to dark grey
						Console.Write("*");//this is the body of snake
						snakeElements.Enqueue(snakeNewHead);
						Console.SetCursorPosition(snakeNewHead.col, snakeNewHead.row);
						Console.ForegroundColor = ConsoleColor.Gray;
					}
				}

				if ((snakeNewHead.col == food.col && snakeNewHead.row == food.row)||(snakeNewHead.col == food.col+1 && snakeNewHead.row == food.row))
				{
					if (snakeNewHead.col == food.col)
					{
						Console.SetCursorPosition(food.col + 1, food.row);
						Console.Write(" ");
					}
					if (snakeNewHead.col == food.col+1)
					{
						Console.SetCursorPosition(food.col, food.row);
						Console.Write(" ");
					}
					
					eat_player.PlaySync();
					
					back_player.PlayLooping();
					// feeding the snake
					
					Position obstacle = new Position();//Define a obstacle	position
					do
					{
						obstacle = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
							randomNumbersGenerator.Next(0, Console.WindowWidth-2));//define a random place the obstacle into the game field
					}
					while (snakeElements.Contains(obstacle) ||
						obstacles.Contains(obstacle) ||
						(food.row == obstacle.row && food.col == obstacle.row));//Redo if the position consist the location without snake body, food and existing obstacle
					obstacles.Add(obstacle);//Add the obstacle to its array
					Console.SetCursorPosition(obstacle.col, obstacle.row);//set the cursor to that position
					Console.ForegroundColor = ConsoleColor.Cyan;//set the font color to cyan
					Console.Write("\u2593");//write the obstacle to the screen
					
					do
					{
						food = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),
							randomNumbersGenerator.Next(0, Console.WindowWidth-3));	//define a random place the food into the game field
					}
					while (snakeElements.Contains(food) || obstacles.Contains(food));
					lastFoodTime = Environment.TickCount;
					Console.SetCursorPosition(food.col, food.row);	//set the cursor to that position
					Console.ForegroundColor = ConsoleColor.Yellow;	//set the color of food to Yellow
					Console.Write("\u2665\u2665");	//set the shape of food
					if (sleepTime <= 10) sleepTime = 10;
					else sleepTime--;
				}
				else
				{
					// moving...
					Position last = snakeElements.Dequeue();//Define the last position of the snake body
					Console.SetCursorPosition(last.col, last.row);//Get the curser of that position
					Console.Write(" ");//Display space at that field
				}

				if (Environment.TickCount - lastFoodTime >= foodDissapearTime)
				{
					negativePoints = negativePoints + 50;
					Console.SetCursorPosition(food.col, food.row);
					Console.Write(" ");
					do
					{
						food = new Position(randomNumbersGenerator.Next(1, Console.WindowHeight),//randomize the window height of the food position
							randomNumbersGenerator.Next(0, Console.WindowWidth-3));//randomize the window width of the food position
					}
					while (snakeElements.Contains(food) || obstacles.Contains(food));
					lastFoodTime = Environment.TickCount;//gets the millisecond count from the computer's system timer
				}
				
				if (Environment.TickCount - lastspecialFoodTime >= specialfoodDissapearTime)
				{
					Console.SetCursorPosition(specialfood.col, specialfood.row);
				    	Console.Write(" ");
				}
				
				Console.SetCursorPosition(food.col, food.row);//set the food column and row position
				Console.ForegroundColor = ConsoleColor.Yellow;//set the foreground color to yellow
				Console.Write("\u2665\u2665");
				
				if (snakeElements.Contains(specialfood))
				{
					Console.SetCursorPosition(specialfood.col, specialfood.row);
					Console.ForegroundColor = ConsoleColor.DarkGray;//set the foreground color to dark grey
					Console.Write("*");//this is the body of snake
				}
				
				if (sleepTime <= 10) sleepTime = 10;
				else sleepTime -= 0.01;//deducts the sleep time by 0.01

				Thread.Sleep((int)sleepTime);//suspends the current thread for the specified number of milliseconds
			}
		}
	}
}

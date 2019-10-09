#include <iostream>
#include <string>
#include <vector>
#include <algorithm>
#include <map>

using namespace std;

int mapSize;
int unitsPerPlayer;

enum ActionType
{
	MoveAndBuild,
	PushAndBuild
};

enum Team
{
	Ally,
	Enemy
};

struct Position
{
	int x;
	int y;

	inline Position operator+(Position pos2) 
	{
		pos2.x += x;
		pos2.y += y;
		return pos2;
	}

	inline bool operator==(Position pos2)
	{
		return pos2.x == x && pos2.y == y;
	}
};

struct Action
{
	ActionType type;
	int unitIndex;
	string dir1;
	string dir2;

	Position pos1;
	Position pos2;
};

struct Unit
{
	int index;
	Team team;
	Position pos;

	vector<Action> actions;
};

vector<Position> directionVector =
{
	Position{ 0, -1 },
	Position{ 1, -1 },
	Position{ 1, 0 },
	Position{ 1, 1 },
	Position{ 0, 1 },
	Position{ -1, 1 },
	Position{ -1, 0 },
	Position{ -1, -1 },
};

map<string, ActionType> actionTypeConstant =
{
	{ "MOVE&BUILD", MoveAndBuild },
	{ "PUSH&BUILD", PushAndBuild },
};

map<string, Position> directionConstant =
{
	{ "N", Position{ 0, -1 } },
	{ "NE", Position{ 1, -1 } },
	{ "E", Position{ 1, 0 } },
	{ "SE", Position{ 1, 1 } },
	{ "S", Position{ 0, 1 } },
	{ "SW", Position{ -1, 1 } },
	{ "W", Position{ -1, 0 } },
	{ "NW", Position{ -1, -1 } },
};

struct GameState
{
	vector<Unit> units;
	vector<vector<char>> map;

	char GetSquare(Position pos)
	{
		if (pos.x >= 0 && pos.x < mapSize && pos.y >= 0 && pos.y < mapSize)
		{
			return map[pos.x][pos.y];
		}
		else
		{
			return '.';
		}

	}

	void ApplyAction(Action &action)
	{
		if (action.type == MoveAndBuild)
		{
			Unit* unit = &units[action.unitIndex];
			unit->pos = action.pos1;

			map[action.pos2.x][action.pos2.y] = (char)((int)map[action.pos2.x][action.pos2.y] + 1);

			for (int i = 0; i < units.size(); i++)
			{
				Unit* unit = &units[i];

				for (auto const& dir1 : directionConstant)
				{
					Position pos1 = unit->pos + dir1.second;
					char currentSquare = map[unit->pos.x][unit->pos.y];
					char square1 = GetSquare(pos1);

					if (square1 == '.')
					{
						continue;
					}
					else
					{
						int currentLevel = currentSquare - '0';
						int squareLevel = square1 - '0';

						if (squareLevel >= 4 || abs(currentLevel - squareLevel) > 1)
						{
							continue;
						}

						bool collision = false;
						for (int u = 0; u < units.size(); u++)
						{
							if (units[u].pos == pos1)
							{
								collision = true;
								break;
							}
						}

						if (collision)
						{
							//cerr << "collision" << endl;
							continue;
						}
					}

					for (auto const& dir2 : directionConstant)
					{
						Position pos2 = pos1 + dir2.second;
						char square2 = GetSquare(pos2);

						if (square2 == '.')
						{
							continue;
						}
						else
						{
							int squareLevel2 = square2 - '0';

							if (squareLevel2 > 3)
							{
								continue;
							}
							
							Action action =
							{
								MoveAndBuild,
								unit->index,
								dir1.first,
								dir2.first,
								pos1,
								pos2,
							};

							unit->actions.push_back(action);
						}
					}

				}
			}
		}
	}
};

void PrintBoard(GameState &state)
{
	for (int y = 0; y < mapSize; y++){
		for (int x = 0; x < mapSize; x++){
			cerr << state.map[x][y];
		}

		cerr << endl;
	}

	cerr << endl;
}

void PrintAction(Action &action)
{
	if (action.type == MoveAndBuild)
	{
		cout << "MOVE&BUILD " << action.unitIndex << " " << action.dir1 << " " << action.dir2 << endl;
	}
}

int TryAction(GameState state, int unitIndex, Action &action, int tryCount)
{
	state.ApplyAction(action);

	cerr << "Action: " << action.pos2.x << " " << action.pos2.y << endl;
	//PrintBoard(state);

	Unit* unit = &state.units[unitIndex];

	int possibleRewards = 0;

	if (tryCount > 0)
	{
		for (int i = 0; i < unit->actions.size(); i++)
		{
		Action* nextAction = &unit->actions[i];		
		possibleRewards += TryAction(state, unitIndex, *nextAction, tryCount - 1);
		}
	}

	//cerr << "Action: " << unit->pos.x << " " << unit->pos.y << " square: " << state.GetSquare(unit->pos) << endl;

	if (unit->actions.size() == 0)
	{
		return possibleRewards - 1;
	}
	else if (state.GetSquare(unit->pos) == '3')
	{
		return possibleRewards + 1;
	}
	else
	{
		return 0;
	}
}

struct Reward
{
	int value;
	Action* action;

	bool operator < (const Reward& ball) const
	{
		return (value < ball.value);
	}
};

void MakeMove(GameState &game)
{
	for (int x = 0; x < game.units.size(); x++)
	{
		Unit* unit = &game.units[x];

		if (unit->team == Ally && unit->actions.size() > 0)
		{
			vector<Reward> rewardList;

			for (int i = 0; i < unit->actions.size(); i++)
			{
				//cerr << "Action: " << unit->actions[i].pos1.x << " " << unit->actions[i].pos1.y << endl;

				int value = TryAction(game, unit->index, unit->actions[i], 1);
								
				Reward reward = {
					value,
					&unit->actions[i]
				};

				rewardList.push_back(reward);
			}

			sort(rewardList.begin(), rewardList.end());
			reverse(rewardList.begin(), rewardList.end());

			//PrintAction(unit->actions.front());

			PrintAction(*rewardList.front().action);

			//cerr << unit.actions.front().pos1.x << endl;
			break;
		}
	}
}

int main()
{
	
	cin >> mapSize; cin.ignore();	
	cin >> unitsPerPlayer; cin.ignore();

	// game loop
	while (1) {
		GameState current;

		vector<vector<char>> tempMap;
		for (int i = 0; i < mapSize; i++) {
			string row;
			cin >> row; cin.ignore();

			vector<char> data(row.begin(), row.end());
			
			tempMap.push_back(data);
		}

		for (int x = 0; x < mapSize; x++)
		{
			vector<char> row;
			for (int y = 0; y < mapSize; y++)
			{
				row.push_back(tempMap[y][x]);
			}

			current.map.push_back(row);
		}

		PrintBoard(current);

		for (int i = 0; i < unitsPerPlayer; i++) {
			int unitX;
			int unitY;
			cin >> unitX >> unitY; cin.ignore();

			Position pos = { unitX, unitY };

			Unit unit =
			{
				i,
				Ally,
				pos
			};

			current.units.push_back(unit);
		}
		for (int i = 0; i < unitsPerPlayer; i++) {
			int otherX;
			int otherY;
			cin >> otherX >> otherY; cin.ignore();

			Position pos = { otherX, otherY };

			Unit unit =
			{
				unitsPerPlayer+i,
				Enemy,
				pos
			};

			current.units.push_back(unit);
		}
		int legalActions;
		cin >> legalActions; cin.ignore();
		for (int i = 0; i < legalActions; i++) {
			string atype;
			int index;
			string dir1;
			string dir2;
			cin >> atype >> index >> dir1 >> dir2; cin.ignore();

			Unit* unit = &current.units[index];



			Action action =
			{
				actionTypeConstant[atype],
				index,
				dir1,
				dir2,
				unit->pos + directionConstant[dir1],
				unit->pos + directionConstant[dir1] + directionConstant[dir2],
			};

			if (action.type == MoveAndBuild)
			{
				unit->actions.push_back(action);
			}

		}

		MakeMove(current);
	}
}
#include <iostream>
#include <string>
#include <vector>
#include <algorithm>

using namespace std;

enum Direction { north, east, west, south};

const string directionToChar = "^><v";

struct Position
{
	int x, y;
};

struct Ball
{
	int id;

	int count;
	int current_count;
	int x;
	int y;
	
	bool operator < (const Ball& ball) const
	{
		return (current_count < ball.current_count);
	}
};

struct Hole
{
	int id;
	int x, y;
	vector<Ball*> balls;
};

struct Game
{
	int width;
	int height;

	int numSolved = 0;
	bool gameSolved = false;

	vector<vector<string>> board;
	vector<Ball> balls;
	vector<Hole> holes;
};

void Initialize(Game &g){
	for (int y = 0; y < g.height; y++){
		for (int x = 0; x < g.width; x++){
			string square = g.board[x][y];

			if (isdigit(square[0]))
			{
				Ball ball;
				ball.count = ball.current_count = stoi(square);
				ball.x = x;
				ball.y = y;
				ball.id = g.balls.size();
				g.balls.push_back(ball);
			}

			if (square == "H"){
				Hole hole;
				hole.x = x;
				hole.y = y;
				hole.id = g.balls.size();

				g.holes.push_back(hole);
			}
		}
	}
}

inline bool IsValidSquare(Game &g, int x, int y){
	if (x < 0 || x >= g.width || y < 0 || y >= g.height){
		return false;
	}

	string square = g.board[x][y];
	if (square == "." || square == "H" || square == "X"){
		return true;
	}

	return false;
}

inline vector<int> GetPosition(int x, int y, int d){
	vector<int> ret;

	if (d == north){
		y -= 1;
	}
	else if (d == south){
		y += 1;
	}
	else if (d == east){
		x += 1;
	}
	else if (d == west){
		x -= 1;
	}

	ret.push_back(x);
	ret.push_back(y);

	return ret;
}

inline Hole* GetHoleAt(Game &g, int x, int y){
	for (int i = 0; i < g.holes.size(); i++){
		Hole* hole = &g.holes[i];

		if (hole->x == x && hole->y == y){
			return hole;
		}
	}
}

vector<int> GetPossibleMove(Game &g, Ball b, vector<Position> path){

	if (path.size() > 0){
		Position pos = path.back();
		b.x = pos.x;
		b.y = pos.y;

		b.current_count--;
	}

	vector<int> dir;

	for (int d = 0; d < 4; d++){
		bool canMove = true;

		int x = b.x;
		int y = b.y;

		for (int i = b.current_count-1; i>=0; i--){

			vector<int> result = GetPosition(x, y, d);
			x = result[0];
			y = result[1];

			if (!(IsValidSquare(g, x, y) && (g.board[x][y] != "H" || i == 0) && (g.board[x][y] != "X" || i != 0))
				|| (g.board[x][y] != "H" && b.current_count == 1)){
				canMove = false;
				break;
			}

			bool previousPath = false;
			for (int p = 0; p < path.size(); p++){
				Position pos = path[p];
				if (pos.x == x && pos.y == y){
					canMove = false;
					previousPath = true;
					break;
				}
			}

			if (previousPath){
				break;
			}
		}

		if (canMove){

			if (g.board[x][y] != "H"){ //test paths that's not a hole
				Position pos;
				pos.x = x;
				pos.y = y;

				path.push_back(pos);
				vector<int> possibleDir = GetPossibleMove(g, b, path);

				if (possibleDir.size() > 0){
					dir.push_back(d);
				}
			}
			else{
				dir.push_back(d);

				Hole* hole = GetHoleAt(g, x, y);
				hole->balls.push_back(&g.balls[b.id]);
			}
		}
	}

	return dir;
}

void MakeMove(Game &g, Ball &b, int d){
	int x = b.x;
	int y = b.y;

	for (int i = 0; i < b.current_count; i++){
		g.board[x][y] = directionToChar[d];

		vector<int> result = GetPosition(x, y, d);
		x = result[0];
		y = result[1];
	}

	b.x = x;
	b.y = y;
	b.current_count -= 1;

	if (g.board[x][y] == "H"){
		g.board[x][y] = "Z";
		b.current_count = 0;
		g.numSolved++;

		if (g.numSolved >= g.balls.size()){
			g.gameSolved = true;
		}

	}else if (b.current_count > 0){
		g.board[x][y] = to_string(b.current_count);
	}
}

void PrintBoard(Game &g, bool debug){
	for (int y = 0; y < g.height; y++){
		for (int x = 0; x < g.width; x++){
			if (debug){
				cerr << g.board[x][y];
			}
			else{
				if (directionToChar.find(g.board[x][y]) != string::npos){
					cout << g.board[x][y];
				}
				else{
					cout << ".";
				}
				
			}
		}
		if (debug){
			cerr << endl;
		}
		else{
			cout << endl;
		}
	}

	if (debug){
		cerr << "\n" << endl;
	}
}

struct PossibleMove{
	Ball* ball;
	int direction;
	Hole* hole;
};

inline void SwitchBoard(Game &g, int h_x, int h_y, bool switchBack){
	for (int y = 0; y < g.height; y++){
		for (int x = 0; x < g.width; x++){
			string square = g.board[x][y];

			if (switchBack == false && square == "H" && x != h_x && y != h_y){
				g.board[x][y] = "E";
			}
			else if (switchBack && square == "E"){
				g.board[x][y] = "H";
			}
		}
	}
}

inline void CleanUp(Game &g){
	for (int i = 0; i < g.holes.size(); i++){
		g.holes[i].balls.clear();
	}
}

int GetOneSolution(Game &g){

	vector<PossibleMove> moveList;

	CleanUp(g);

	sort(g.balls.begin(), g.balls.end());
	for (int i = g.balls.size()-1; i >= 0; i--){
		if (g.balls[i].current_count > 0){
			vector<Position> path;
			vector<int> moves = GetPossibleMove(g, g.balls[i], path);

			if (moves.size() == 1){
				MakeMove(g, g.balls[i], moves[0]);
				PrintBoard(g, true);
				return 1;
			}
			else if (moves.size() > 0){
				/*for (int m = 0; m < moves.size(); m++){
					PossibleMove move;
					move.ball = &g.balls[i];
					move.direction = moves[m];
				}*/
			}
			else{
				return 0;
			}
		}		
	}

	for (int h = 0; h < g.holes.size(); h++){
		cerr << g.holes[h].balls.size() << endl;

		if (g.holes[h].balls.size() == 1){
			Hole hole = g.holes[h];
			Ball ball = g.balls[hole.balls[0]->id];

			if (ball.current_count > 0){
				SwitchBoard(g, hole.x, hole.y, false);

				vector<Position> path;
				vector<int> moves = GetPossibleMove(g, ball, path);

				SwitchBoard(g, hole.x, hole.y, true);

				if (moves.size() == 1){
					MakeMove(g, ball, moves[0]);
					PrintBoard(g, true);
					return 1;
				}
			}
			break;
		}
	}

	return -1;
}

int SolveBoard(Game &g){
	int solutions = 0;
	do{
		solutions = GetOneSolution(g);
	}while(solutions == 1);

	return solutions;
}

void SolveGame(Game &g)
{
	int solutions = SolveBoard(g);
	if (g.gameSolved){
		PrintBoard(g, false);
	}
	else if (solutions == 0){

	}
}

void test(Position &pos){
	pos.x = 50;
}

int main()
{
	Game game;

	int width;
	int height;

	cin >> width >> height; cin.ignore();
	
	game.board.resize(width);
	game.width = width;
	game.height = height;

	for (int x = 0; x < width; x++){
		game.board[x].resize(height);
	}
	
	for (int i = 0; i < height; i++) {
		string row;
		cin >> row; cin.ignore();
		cerr << row << "\n";
			
		for (int j = 0; j < row.length(); j++){
			game.board[j][i] = row[j];
		}
	}
	cerr << "\n" << endl;

	Initialize(game);
	SolveBoard(game);
	PrintBoard(game, false);
}
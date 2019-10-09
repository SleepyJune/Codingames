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

struct Game
{
	int width;
	int height;

	int numSolved = 0;
	bool gameSolved = false;

	vector<vector<string>> board;
	vector<Ball> balls;
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

				path.push_back(pos); //sth wrong here, need to push more paths
				vector<int> possibleDir = GetPossibleMove(g, b, path);

				if (possibleDir.size() > 0){
					dir.push_back(d);
				}
			}
			else{
				dir.push_back(d);
			}
		}
	}

	return dir;
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

inline Ball* GetBallByID(Game &g, int id){
	for (int i = 0; i < g.balls.size(); i++){
		if (g.balls[i].id == id){
			return &g.balls[i];
		}
	}
}

struct PossibleMove{
	int ballID;
	int direction;
};

int TrySolution(Game g, PossibleMove move);

int GetOneSolution(Game &g){

	vector<PossibleMove> moveList;

	sort(g.balls.begin(), g.balls.end());
	for (int i = g.balls.size()-1; i >= 0; i--){
		if (g.balls[i].current_count > 0){
			vector<Position> path;
			vector<int> moves = GetPossibleMove(g, g.balls[i], path);

			if (moves.size() == 1){
				MakeMove(g, g.balls[i], moves[0]);
				//PrintBoard(g, true);
				return 1;
			}
			else if (moves.size() > 0){
				for (int m = 0; m < moves.size(); m++){

				PossibleMove move;
				move.ballID = g.balls[i].id;
				move.direction = moves[m];

				moveList.push_back(move);

				}
			}
			else{
				return 0;
			}
		}		
	}

	for (int i = 0; i < moveList.size(); i++){
		PossibleMove move = moveList[i];

		if (TrySolution(g, move)){ //game solved
			Ball* ball = GetBallByID(g, move.ballID);
			MakeMove(g, *ball, move.direction); //do it for this game board
			PrintBoard(g, true);
			return 1;
		}
	}

	return -1;
}

bool SolveOneSolutions(Game &g){
	int solutions = 0;
	do{
		solutions = GetOneSolution(g);
	} while (solutions == 1);

	return g.gameSolved;
}

int TrySolution(Game g, PossibleMove move){
	Ball *ball = GetBallByID(g, move.ballID);
	MakeMove(g, *ball, move.direction);
	return SolveOneSolutions(g);
}

void SolveGame(Game &g)
{
	int solutions = 0;
	do{
		solutions = SolveOneSolutions(g);
		if (solutions > 1){

		}

	} while (g.gameSolved || solutions == 0);
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
	SolveOneSolutions(game);
	PrintBoard(game, false);
}
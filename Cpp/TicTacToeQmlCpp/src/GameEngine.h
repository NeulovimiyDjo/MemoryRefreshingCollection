#ifndef GAME_ENGINE_H
#define GAME_ENGINE_H

#include <vector>

class GameEngine {
public:
  enum class SquareStatus {Clear, Cross, Nought};
  enum class GameStatus {InProgress, Draw, CrossesWon, NoughtsWon};

  GameEngine();

  SquareStatus getSquareStatus(int i, int j);
  GameStatus getGameStatus();
  int getMovesCount();
  bool makeMove(int i, int j);
  void resetGame();
  bool changeSize(int m, int n, int chain);
  std::vector<int>& getVictorySquares();

private:
  GameStatus m_game_status{GameStatus::InProgress};
  int m_moves_count{0};
  std::vector<SquareStatus> m_grid;
  int m_row_count;
  int m_column_count;
  int m_chain_size;
  std::vector<int> m_victory_squares;

  void m_checkGame();
};

#endif // GAME_ENGINE_H

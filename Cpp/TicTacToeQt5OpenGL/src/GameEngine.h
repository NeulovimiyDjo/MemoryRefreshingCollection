#ifndef GAME_ENGINE_H
#define GAME_ENGINE_H

#include <QObject>
#include <vector>


enum class SquareTypes : unsigned char { Clear = 0, Cross, Nought };
enum class WinTypes: unsigned char { Crosses, Noughts, Draw };
enum class Directions: unsigned char { Right, Down, RightDown, LeftDown, None };

struct Conditions {
  WinTypes win_type;
  Directions direction;
  unsigned start_i;
  unsigned start_j;
};


class GameEngine: public QObject {
  Q_OBJECT

public:
  GameEngine();

public slots:
  void newGame(unsigned dim_x, unsigned dim_y, unsigned win_size);
  void makeMove(unsigned i);

signals:
  void gameCreated(unsigned dim_x, unsigned dim_y, unsigned win_size);
  void moveMade(unsigned i, SquareTypes type);
  void gameOver(Conditions conditions);

private:
  void checkGame();
  WinTypes calculateWinner();

  unsigned m_dim_x = 3;
  unsigned m_dim_y = 3;
  unsigned m_win_size = 3;

  std::vector<SquareTypes> m_squares;

  unsigned m_count = 0;
  bool m_over = false;
  Conditions m_over_conditions;
};

#endif // GAME_ENGINE_H

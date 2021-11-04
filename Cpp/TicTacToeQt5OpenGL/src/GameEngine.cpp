#include "GameEngine.h"

GameEngine::GameEngine() {
  m_squares = std::vector<SquareTypes>(m_dim_x * m_dim_y, SquareTypes::Clear);
}


void GameEngine::newGame(unsigned dim_x, unsigned dim_y, unsigned win_size) {
  m_dim_x = dim_x;
  m_dim_y = dim_y;
  m_win_size = win_size;

  m_count = 0;
  m_over = false;
  m_squares = std::vector<SquareTypes>(m_dim_x * m_dim_y, SquareTypes::Clear);

  emit gameCreated(dim_x, dim_y, win_size);
}


void GameEngine::makeMove(unsigned i) {
  if (m_over || m_squares[i] != SquareTypes::Clear) return;

  SquareTypes new_type;
  if (m_count % 2 == 0) {
    new_type = SquareTypes::Cross;
  } else {
    new_type = SquareTypes::Nought;
  }

  m_squares[i] = new_type;
  ++m_count;

  emit moveMade(i, new_type);

  checkGame();

  if (m_over) {
    emit gameOver(m_over_conditions);
    return;
  }
}


WinTypes GameEngine::calculateWinner() {
  if (m_count % 2 == 1) {
    return WinTypes::Crosses;
  } else {
    return WinTypes::Noughts;
  }
}


// ckeck for chain=|winSize squares of same type in a row| in 4 possible directions
// for every valid starting point on the field
void GameEngine::checkGame() {
  // check rows
  for (unsigned i = 0; i < m_dim_y; ++i) {
    for (unsigned j = 0; j < m_dim_x - m_win_size + 1; ++j) {

      SquareTypes start = m_squares[i * m_dim_x + j];
      if (start != SquareTypes::Clear) {

        bool found_chain = true;
        for (unsigned k = 1; k < m_win_size; ++k) {
          if (m_squares[i * m_dim_x + (j + k)] != start)
            found_chain = false;
        }

        if (found_chain) {
          m_over = true;
          m_over_conditions = Conditions{ calculateWinner(), Directions::Right, i, j };
          return;
        }
      }
    }
  }

  // check columns
  for (unsigned i = 0; i < m_dim_y - m_win_size + 1; ++i) {
    for (unsigned j = 0; j < m_dim_x; ++j) {

      SquareTypes start = m_squares[i * m_dim_x + j];
      if (start != SquareTypes::Clear) {

        bool found_chain = true;
        for (unsigned k = 1; k < m_win_size; ++k) {
          if (m_squares[(i + k) * m_dim_x + j] != start)
            found_chain = false;
        }

        if (found_chain) {
          m_over = true;
          m_over_conditions = Conditions{ calculateWinner(), Directions::Down, i, j };
          return;
        }
      }
    }
  }

  // check dioganal right+down
  for (unsigned i = 0; i < m_dim_y - m_win_size + 1; ++i) {
    for (unsigned j = 0; j < m_dim_x - m_win_size + 1; ++j) {

      SquareTypes start = m_squares[i * m_dim_x + j];
      if (start != SquareTypes::Clear) {

        bool found_chain = true;
        for (unsigned k = 1; k < m_win_size; ++k) {
          if (m_squares[(i + k) * m_dim_x + (j + k)] != start)
            found_chain = false;
        }

        if (found_chain) {
          m_over = true;
          m_over_conditions = Conditions{ calculateWinner(), Directions::RightDown, i, j };
          return;
        }
      }
    }
  }

  // check dioganal left+down
  for (unsigned i = 0; i < m_dim_y - m_win_size + 1; ++i) {
    for (unsigned j = m_win_size - 1; j < m_dim_x; ++j) {

      SquareTypes start = m_squares[i * m_dim_x + j];
      if (start != SquareTypes::Clear) {

        bool found_chain = true;
        for (unsigned k = 1; k < m_win_size; ++k) {
          if (m_squares[(i + k) * m_dim_x + (j - k)] != start)
            found_chain = false;
        }

        if (found_chain) {
          m_over = true;
          m_over_conditions = Conditions{ calculateWinner(), Directions::LeftDown, i, j };
          return;
        }
      }
    }
  }

  if (m_count == m_dim_x * m_dim_y) {
    m_over = true;
    m_over_conditions = Conditions{ WinTypes::Draw, Directions::None, 0, 0 };
    return;
  }
}


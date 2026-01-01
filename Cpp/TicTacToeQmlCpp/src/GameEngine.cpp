#include "GameEngine.h"

GameEngine::GameEngine() {
  m_row_count = 3;
  m_column_count = 3;
  m_chain_size = 3;

  for (int i = 0; i < m_row_count*m_column_count; ++i)
    m_grid.push_back(SquareStatus::Clear);
}

void GameEngine::m_checkGame() {
  SquareStatus start_status;
  bool found_chain;

  // check rows
  for (int i = 0; i < m_row_count; ++i)
    for (int j = 0; j < m_column_count - m_chain_size + 1; ++j) {
      start_status = m_grid[i * m_column_count + j];
      if (start_status != SquareStatus::Clear) {
        found_chain = true;
        for (int k = 1; k < m_chain_size; ++k) {
          if (m_grid[i * m_column_count + j + k] != start_status)
            found_chain = false;
        }
        if (found_chain) {
          if (m_moves_count % 2 == 1)
            m_game_status = GameStatus::CrossesWon;
          else if (m_moves_count % 2 == 0)
            m_game_status = GameStatus::NoughtsWon;

          for (int k = 0; k < m_chain_size; ++k)
            m_victory_squares.push_back(i * m_column_count + j + k);

          return;
        }
      }
    }

  // check columns
  for (int i = 0; i < m_row_count - m_chain_size + 1; ++i)
    for (int j = 0; j < m_column_count; ++j) {
      start_status = m_grid[i * m_column_count + j];
      if (start_status != SquareStatus::Clear) {
        found_chain = true;
        for (int k = 1; k < m_chain_size; ++k) {
          if (m_grid[(i + k) * m_column_count + j] != start_status)
            found_chain = false;
        }
        if (found_chain) {
          if (m_moves_count % 2 == 1)
            m_game_status = GameStatus::CrossesWon;
          else if (m_moves_count % 2 == 0)
            m_game_status = GameStatus::NoughtsWon;

          for (int k = 0; k < m_chain_size; ++k)
            m_victory_squares.push_back((i + k) * m_column_count + j);

          return;
        }
      }
    }

  // check dioganal right+down
  for (int i = 0; i < m_row_count - m_chain_size + 1; ++i)
    for (int j = 0; j < m_column_count - m_chain_size + 1; ++j) {
      start_status = m_grid[i * m_column_count + j];
      if (start_status != SquareStatus::Clear) {
        found_chain = true;
        for (int k = 1; k < m_chain_size; ++k) {
          if (m_grid[(i + k) * m_column_count + j + k] != start_status)
            found_chain = false;
        }
        if (found_chain) {
          if (m_moves_count % 2 == 1)
            m_game_status = GameStatus::CrossesWon;
          else if (m_moves_count % 2 == 0)
            m_game_status = GameStatus::NoughtsWon;

          for (int k = 0; k < m_chain_size; ++k)
            m_victory_squares.push_back((i + k) * m_column_count + j + k);

          return;
        }
      }
    }

  // check dioganal left+down
  for (int i = 0; i < m_row_count - m_chain_size + 1; ++i)
    for (int j = m_chain_size - 1; j < m_column_count; ++j) {
      start_status = m_grid[i * m_column_count + j];
      if (start_status != SquareStatus::Clear) {
        found_chain = true;
        for (int k = 1; k < m_chain_size; ++k) {
          if (m_grid[(i + k) * m_column_count + j - k] != start_status)
            found_chain = false;
        }
        if (found_chain) {
          if (m_moves_count % 2 == 1)
            m_game_status = GameStatus::CrossesWon;
          else if (m_moves_count % 2 == 0)
            m_game_status = GameStatus::NoughtsWon;

          for (int k = 0; k < m_chain_size; ++k)
            m_victory_squares.push_back((i + k) * m_column_count + j - k);

          return;
        }
      }
    }

  if (m_moves_count == m_row_count*m_column_count) {
    m_game_status = GameStatus::Draw;
    return;
  }
}

GameEngine::SquareStatus GameEngine::getSquareStatus(int i, int j) {
  return m_grid[i*m_column_count+j];
}

GameEngine::GameStatus GameEngine::getGameStatus() {
  return m_game_status;
}

int GameEngine::getMovesCount() {
  return m_moves_count;
}

std::vector<int>& GameEngine::getVictorySquares() {
  return m_victory_squares;
}

bool GameEngine::makeMove(int i, int j) {
  if (m_grid[i*m_column_count+j] == SquareStatus::Clear && m_game_status == GameStatus::InProgress) {
    if (m_moves_count % 2 == 0) {
      m_grid[i*m_column_count+j] = SquareStatus::Cross;
    }
    else if (m_moves_count % 2 == 1) {
     m_grid[i*m_column_count+j] = SquareStatus::Nought;
    }
    ++m_moves_count;
    m_checkGame();
    return true;
  }

  return false;
}

void GameEngine::resetGame() {
  m_moves_count = 0;
  m_game_status = GameStatus::InProgress;

  m_victory_squares.clear();

  for (auto& square : m_grid)
    square = SquareStatus::Clear;
}

bool GameEngine::changeSize(int m, int n, int chain) {
  if (m>=chain && n>= chain && chain>=2) {
    m_grid.clear();

    m_row_count = m;
    m_column_count = n;
    m_chain_size = chain;

    m_moves_count = 0;
    m_game_status = GameStatus::InProgress;
    m_victory_squares.clear();

    for (int i = 0; i < m_row_count*m_column_count; ++i)
      m_grid.push_back(SquareStatus::Clear);

    return true;
  } else
    return false;
}
